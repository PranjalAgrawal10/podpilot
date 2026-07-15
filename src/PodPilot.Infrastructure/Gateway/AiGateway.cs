using System.Text;
using System.Text.Json;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Gateway;
using PodPilot.Application.Models.Scheduler;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Orchestrates end-to-end AI gateway request handling via AI providers or the scheduler.
/// </summary>
public sealed class AiGateway : IAiGateway
{
    private readonly IRequestScheduler scheduler;
    private readonly IAiInferenceRouter inferenceRouter;
    private readonly IAiInferenceDispatcher inferenceDispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiGateway"/> class.
    /// </summary>
    public AiGateway(
        IRequestScheduler scheduler,
        IAiInferenceRouter inferenceRouter,
        IAiInferenceDispatcher inferenceDispatcher)
    {
        this.scheduler = scheduler;
        this.inferenceRouter = inferenceRouter;
        this.inferenceDispatcher = inferenceDispatcher;
    }

    /// <inheritdoc />
    public Task<GatewayRequestResult> HandleRequestAsync(
        GatewayAuthContext auth,
        string path,
        string method,
        IReadOnlyDictionary<string, string> headers,
        Stream requestBody,
        Stream responseBody,
        GatewayErrorFormat errorFormat,
        string? correlationId,
        Func<GatewayProxyResult, Task>? onResponseHeaders = null,
        CancellationToken cancellationToken = default) =>
        HandleRequestAsync(
            auth,
            path,
            method,
            headers,
            requestBody,
            responseBody,
            errorFormat,
            correlationId,
            headers.TryGetValue("X-Request-Id", out var clientRequestId) ? clientRequestId : null,
            onResponseHeaders,
            cancellationToken);

    private async Task<GatewayRequestResult> HandleRequestAsync(
        GatewayAuthContext auth,
        string path,
        string method,
        IReadOnlyDictionary<string, string> headers,
        Stream requestBody,
        Stream responseBody,
        GatewayErrorFormat errorFormat,
        string? correlationId,
        string? clientRequestId,
        Func<GatewayProxyResult, Task>? onResponseHeaders,
        CancellationToken cancellationToken)
    {
        var bufferedBody = new MemoryStream();
        if (requestBody.CanSeek)
        {
            requestBody.Position = 0;
        }

        await requestBody.CopyToAsync(bufferedBody, cancellationToken);
        bufferedBody.Position = 0;

        var bodyJson = Encoding.UTF8.GetString(bufferedBody.ToArray());
        bufferedBody.Position = 0;

        var model = TryExtractModel(bodyJson);
        var route = await inferenceRouter.TryResolveAsync(
            auth.OrganizationId,
            model,
            path,
            bodyJson,
            cancellationToken);
        if (route is not null)
        {
            var streamRequested = false;
            try
            {
                using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(bodyJson) ? "{}" : bodyJson);
                streamRequested = doc.RootElement.TryGetProperty("stream", out var streamEl) &&
                                  streamEl.ValueKind == JsonValueKind.True;
            }
            catch (JsonException)
            {
                streamRequested = false;
            }

            if (onResponseHeaders is not null)
            {
                await onResponseHeaders(new GatewayProxyResult
                {
                    StatusCode = 200,
                    Headers = new Dictionary<string, string>
                    {
                        ["Content-Type"] = streamRequested ? "text/event-stream" : "application/json",
                    },
                });
            }

            var dispatch = await inferenceDispatcher.DispatchAsync(
                new AiDispatchContext
                {
                    OrganizationId = auth.OrganizationId,
                    Route = route,
                    Path = path,
                    Method = method,
                    BodyJson = bodyJson,
                    ResponseBody = responseBody,
                    Stream = streamRequested,
                },
                cancellationToken);

            return new GatewayRequestResult
            {
                Success = dispatch.Success,
                StatusCode = dispatch.StatusCode,
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = streamRequested ? "text/event-stream" : "application/json",
                },
                ErrorCode = dispatch.Success ? null : "ai_provider_error",
                ErrorMessage = dispatch.ErrorMessage,
            };
        }

        var result = await scheduler.ProcessAsync(
            new ScheduleRequestContext
            {
                Auth = auth,
                Path = path,
                Method = method,
                Headers = headers,
                Body = bufferedBody,
                ResponseBody = responseBody,
                ErrorFormat = errorFormat,
                CorrelationId = correlationId,
                ClientRequestId = clientRequestId,
                OnResponseHeaders = onResponseHeaders,
            },
            cancellationToken);

        return new GatewayRequestResult
        {
            Success = result.Success,
            StatusCode = result.StatusCode,
            Headers = result.Headers,
            ErrorCode = result.ErrorCode,
            ErrorMessage = result.ErrorMessage,
        };
    }

    private static string? TryExtractModel(string bodyJson)
    {
        if (string.IsNullOrWhiteSpace(bodyJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(bodyJson);
            return doc.RootElement.TryGetProperty("model", out var modelEl)
                ? modelEl.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
