using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Gateway;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Orchestrates end-to-end AI gateway request handling.
/// </summary>
public sealed class AiGateway : IAiGateway
{
    private readonly IApplicationDbContext dbContext;
    private readonly IGatewayRouter router;
    private readonly IStreamingProxy streamingProxy;
    private readonly IInferenceClient inferenceClient;
    private readonly IPodLifecycleService lifecycleService;
    private readonly IGatewayNotificationService notificationService;
    private readonly IDateTimeService dateTimeService;
    private readonly ILogger<AiGateway> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiGateway"/> class.
    /// </summary>
    public AiGateway(
        IApplicationDbContext dbContext,
        IGatewayRouter router,
        IStreamingProxy streamingProxy,
        IInferenceClient inferenceClient,
        IPodLifecycleService lifecycleService,
        IGatewayNotificationService notificationService,
        IDateTimeService dateTimeService,
        ILogger<AiGateway> logger)
    {
        this.dbContext = dbContext;
        this.router = router;
        this.streamingProxy = streamingProxy;
        this.inferenceClient = inferenceClient;
        this.lifecycleService = lifecycleService;
        this.notificationService = notificationService;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<GatewayRequestResult> HandleRequestAsync(
        GatewayAuthContext auth,
        string path,
        string method,
        IReadOnlyDictionary<string, string> headers,
        Stream requestBody,
        Stream responseBody,
        GatewayErrorFormat errorFormat,
        string? correlationId,
        Func<GatewayProxyResult, Task>? onResponseHeaders = null,
        CancellationToken cancellationToken = default)
    {
        var totalStopwatch = Stopwatch.StartNew();
        var wakeStopwatch = new Stopwatch();
        var healthStopwatch = new Stopwatch();
        GatewayRequest? gatewayRequest = null;

        try
        {
            var bufferedBody = await BufferRequestBodyAsync(requestBody, cancellationToken);
            var model = ExtractModel(bufferedBody);
            bufferedBody.Position = 0;

            var route = await router.ResolveAsync(auth.OrganizationId, model, cancellationToken);
            var pod = route.Pod;

            gatewayRequest = new GatewayRequest
            {
                OrganizationId = auth.OrganizationId,
                ApiKeyId = auth.ApiKeyId,
                GpuPodId = pod.Id,
                HttpMethod = method,
                Path = path,
                Model = route.Model,
                Status = GatewayRequestStatus.Pending,
                StartedAt = dateTimeService.UtcNow,
                CorrelationId = correlationId,
                UpstreamBaseUrl = route.BaseUrl,
            };

            await dbContext.AddGatewayRequestAsync(gatewayRequest, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            await notificationService.NotifyRequestStartedAsync(
                auth.OrganizationId,
                gatewayRequest.Id,
                pod.Id,
                path,
                cancellationToken);

            logger.LogInformation(
                "Gateway request {RequestId} received for pod {PodId} path {Path}",
                gatewayRequest.Id,
                pod.Id,
                path);

            if (pod.Status != PodStatus.Running)
            {
                gatewayRequest.Status = GatewayRequestStatus.Waking;
                gatewayRequest.WakeTriggered = true;
                await dbContext.SaveChangesAsync(cancellationToken);

                await notificationService.NotifyPodWakeAsync(auth.OrganizationId, pod.Id, cancellationToken);
                logger.LogInformation("Wake triggered for pod {PodId} by gateway", pod.Id);

                wakeStopwatch.Start();
                var wakeResult = await lifecycleService.WakePodAsync(
                    pod.Id,
                    auth.OrganizationId,
                    "gateway",
                    auth.UserId,
                    processImmediately: true,
                    cancellationToken);
                wakeStopwatch.Stop();

                if (!wakeResult.Success)
                {
                    throw new InvalidOperationException(wakeResult.ErrorMessage ?? "Failed to wake pod.");
                }

                pod = await dbContext.GpuPods.FirstAsync(p => p.Id == route.Pod.Id, cancellationToken);
            }

            gatewayRequest.Status = GatewayRequestStatus.WaitingHealthy;
            await dbContext.SaveChangesAsync(cancellationToken);

            healthStopwatch.Start();
            var healthy = await inferenceClient.WaitForHealthyAsync(route.BaseUrl, cancellationToken);
            healthStopwatch.Stop();

            if (!healthy)
            {
                throw new TimeoutException("Inference backend did not become healthy in time.");
            }

            gatewayRequest.Status = GatewayRequestStatus.Forwarding;
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Forwarding gateway request {RequestId} to {BaseUrl}", gatewayRequest.Id, route.BaseUrl);

            var proxyResult = await streamingProxy.ForwardAsync(
                new GatewayProxyOptions
                {
                    BaseUrl = route.BaseUrl,
                    Path = path,
                    Method = method,
                    Headers = headers,
                    Body = bufferedBody,
                    CancellationToken = cancellationToken,
                    Timeout = ApplicationConstants.GatewayForwardTimeout,
                },
                responseBody,
                onResponseHeaders,
                cancellationToken);

            gatewayRequest.Status = proxyResult.IsStreaming
                ? GatewayRequestStatus.Streaming
                : GatewayRequestStatus.Completed;
            gatewayRequest.IsStreaming = proxyResult.IsStreaming;
            gatewayRequest.CompletedAt = dateTimeService.UtcNow;

            totalStopwatch.Stop();
            gatewayRequest.Latency = new GatewayLatency
            {
                GatewayRequestId = gatewayRequest.Id,
                WakeLatencyMs = wakeStopwatch.ElapsedMilliseconds > 0 ? (int)wakeStopwatch.ElapsedMilliseconds : null,
                HealthCheckLatencyMs = healthStopwatch.ElapsedMilliseconds > 0 ? (int)healthStopwatch.ElapsedMilliseconds : null,
                ForwardLatencyMs = proxyResult.ForwardLatencyMs,
                TotalLatencyMs = (int)totalStopwatch.ElapsedMilliseconds,
            };

            await dbContext.SaveChangesAsync(cancellationToken);

            await lifecycleService.RecordActivityAsync(
                pod.Id,
                PodActivityType.ApiRequest,
                "gateway",
                auth.UserId,
                metadata: JsonSerializer.Serialize(new { path, model = route.Model }),
                cancellationToken: cancellationToken);

            await notificationService.NotifyRequestFinishedAsync(
                auth.OrganizationId,
                gatewayRequest.Id,
                gatewayRequest.Status.ToString(),
                gatewayRequest.Latency.TotalLatencyMs,
                cancellationToken);

            logger.LogInformation(
                "Gateway request {RequestId} completed in {LatencyMs}ms",
                gatewayRequest.Id,
                gatewayRequest.Latency.TotalLatencyMs);

            return new GatewayRequestResult
            {
                Success = true,
                StatusCode = proxyResult.StatusCode,
                Headers = proxyResult.Headers,
            };
        }
        catch (OperationCanceledException)
        {
            if (gatewayRequest is not null)
            {
                gatewayRequest.Status = GatewayRequestStatus.Cancelled;
                gatewayRequest.CompletedAt = dateTimeService.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            throw;
        }
        catch (Exception ex)
        {
            if (gatewayRequest is not null)
            {
                gatewayRequest.Status = GatewayRequestStatus.Failed;
                gatewayRequest.CompletedAt = dateTimeService.UtcNow;
                gatewayRequest.Error = new GatewayError
                {
                    GatewayRequestId = gatewayRequest.Id,
                    ErrorFormat = errorFormat,
                    ErrorCode = MapErrorCode(ex),
                    Message = GetClientMessage(ex),
                    InternalDetails = ex.ToString(),
                    OccurredAt = dateTimeService.UtcNow,
                };

                totalStopwatch.Stop();
                gatewayRequest.Latency = new GatewayLatency
                {
                    GatewayRequestId = gatewayRequest.Id,
                    WakeLatencyMs = wakeStopwatch.ElapsedMilliseconds > 0 ? (int)wakeStopwatch.ElapsedMilliseconds : null,
                    HealthCheckLatencyMs = healthStopwatch.ElapsedMilliseconds > 0 ? (int)healthStopwatch.ElapsedMilliseconds : null,
                    TotalLatencyMs = (int)totalStopwatch.ElapsedMilliseconds,
                };

                await dbContext.SaveChangesAsync(cancellationToken);

                await notificationService.NotifyGatewayErrorAsync(
                    auth.OrganizationId,
                    gatewayRequest.Id,
                    gatewayRequest.Error.ErrorCode,
                    gatewayRequest.Error.Message,
                    cancellationToken);
            }

            logger.LogWarning(ex, "Gateway request failed");

            return new GatewayRequestResult
            {
                Success = false,
                StatusCode = MapStatusCode(ex),
                ErrorCode = MapErrorCode(ex),
                ErrorMessage = GetClientMessage(ex),
            };
        }
    }

    private static async Task<MemoryStream> BufferRequestBodyAsync(Stream requestBody, CancellationToken cancellationToken)
    {
        var bufferedBody = new MemoryStream();
        if (requestBody.CanSeek)
        {
            requestBody.Position = 0;
        }

        await requestBody.CopyToAsync(bufferedBody, cancellationToken);
        bufferedBody.Position = 0;
        return bufferedBody;
    }

    private static string? ExtractModel(MemoryStream body)
    {
        if (body.Length == 0)
        {
            return null;
        }

        var content = Encoding.UTF8.GetString(body.ToArray());
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.TryGetProperty("model", out var modelProp))
            {
                return modelProp.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static string MapErrorCode(Exception ex) =>
        ex switch
        {
            TimeoutException => "timeout",
            InvalidOperationException => "pod_unavailable",
            _ => "gateway_error",
        };

    private static int MapStatusCode(Exception ex) =>
        ex switch
        {
            TimeoutException => 504,
            InvalidOperationException => 503,
            _ => 502,
        };

    private static string GetClientMessage(Exception ex) =>
        ex switch
        {
            TimeoutException => "The request timed out while waiting for the inference backend.",
            InvalidOperationException => ex.Message,
            _ => "An error occurred while processing the gateway request.",
        };
}
