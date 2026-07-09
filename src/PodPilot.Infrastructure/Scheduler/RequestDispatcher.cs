using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Gateway;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Application.Models.Scheduler;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using StackExchange.Redis;

namespace PodPilot.Infrastructure.Scheduler;

/// <summary>
/// Dispatches scheduled requests to GPU pods.
/// </summary>
public sealed class RequestDispatcher : IRequestDispatcher
{
    private readonly IApplicationDbContext dbContext;
    private readonly IGatewayRouter router;
    private readonly IPodOrchestrator podOrchestrator;
    private readonly IStreamingProxy streamingProxy;
    private readonly IInferenceClient inferenceClient;
    private readonly IPodLifecycleService lifecycleService;
    private readonly IConnectionMultiplexer? redis;
    private readonly IDateTimeService dateTimeService;
    private readonly ILogger<RequestDispatcher> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestDispatcher"/> class.
    /// </summary>
    public RequestDispatcher(
        IApplicationDbContext dbContext,
        IGatewayRouter router,
        IPodOrchestrator podOrchestrator,
        IStreamingProxy streamingProxy,
        IInferenceClient inferenceClient,
        IPodLifecycleService lifecycleService,
        IDateTimeService dateTimeService,
        ILogger<RequestDispatcher> logger,
        IConnectionMultiplexer? redis = null)
    {
        this.dbContext = dbContext;
        this.router = router;
        this.podOrchestrator = podOrchestrator;
        this.streamingProxy = streamingProxy;
        this.inferenceClient = inferenceClient;
        this.lifecycleService = lifecycleService;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
        this.redis = redis;
    }

    /// <inheritdoc />
    public async Task<DispatchResult> DispatchAsync(DispatchContext context, CancellationToken cancellationToken = default)
    {
        var wakeStopwatch = new Stopwatch();
        var healthStopwatch = new Stopwatch();

        try
        {
            var pod = await dbContext.GpuPods.FirstAsync(p => p.Id == context.PodId, cancellationToken);

            if (pod.Status != PodStatus.Running)
            {
                wakeStopwatch.Start();
                var wakeResult = await lifecycleService.WakePodAsync(
                    context.PodId,
                    context.OrganizationId,
                    "scheduler",
                    context.UserId,
                    processImmediately: true,
                    cancellationToken);
                wakeStopwatch.Stop();

                if (!wakeResult.Success)
                {
                    return new DispatchResult
                    {
                        Success = false,
                        StatusCode = 503,
                        ErrorMessage = wakeResult.ErrorMessage ?? "Failed to wake pod.",
                        IsRetryable = true,
                        WakeLatencyMs = (int)wakeStopwatch.ElapsedMilliseconds,
                    };
                }
            }

            healthStopwatch.Start();
            var healthy = await inferenceClient.WaitForHealthyAsync(context.BaseUrl, cancellationToken);
            healthStopwatch.Stop();

            if (!healthy)
            {
                return new DispatchResult
                {
                    Success = false,
                    StatusCode = 504,
                    ErrorMessage = "Inference backend did not become healthy in time.",
                    IsRetryable = true,
                    WakeLatencyMs = wakeStopwatch.ElapsedMilliseconds > 0 ? (int)wakeStopwatch.ElapsedMilliseconds : null,
                    HealthCheckLatencyMs = (int)healthStopwatch.ElapsedMilliseconds,
                };
            }

            var forwardStopwatch = Stopwatch.StartNew();
            MemoryStream? responseBuffer = null;
            Stream targetStream;

            if (context.ResponseBody is not null)
            {
                targetStream = context.ResponseBody;
            }
            else
            {
                responseBuffer = new MemoryStream();
                targetStream = responseBuffer;
            }

            var proxyResult = await streamingProxy.ForwardAsync(
                new GatewayProxyOptions
                {
                    BaseUrl = context.BaseUrl,
                    Path = context.Path,
                    Method = context.Method,
                    Headers = context.Headers,
                    Body = context.Body,
                    CancellationToken = cancellationToken,
                    Timeout = ApplicationConstants.GatewayForwardTimeout,
                },
                targetStream,
                context.OnResponseHeaders,
                cancellationToken);

            forwardStopwatch.Stop();

            await lifecycleService.RecordActivityAsync(
                context.PodId,
                PodActivityType.ApiRequest,
                "scheduler",
                context.UserId,
                metadata: context.Model,
                cancellationToken: cancellationToken);

            return new DispatchResult
            {
                Success = true,
                StatusCode = proxyResult.StatusCode,
                Headers = proxyResult.Headers,
                IsStreaming = proxyResult.IsStreaming,
                ForwardLatencyMs = (int)forwardStopwatch.ElapsedMilliseconds,
                WakeLatencyMs = wakeStopwatch.ElapsedMilliseconds > 0 ? (int)wakeStopwatch.ElapsedMilliseconds : null,
                HealthCheckLatencyMs = healthStopwatch.ElapsedMilliseconds > 0 ? (int)healthStopwatch.ElapsedMilliseconds : null,
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Dispatch failed for request {RequestId}", context.RequestId);
            return new DispatchResult
            {
                Success = false,
                StatusCode = 502,
                ErrorMessage = ex.Message,
                IsRetryable = IsRetryable(ex),
            };
        }
    }

    /// <inheritdoc />
    public async Task<PodSelectionResult?> SelectPodAsync(
        Guid organizationId,
        string? modelName,
        Guid? preferredPodId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orchestratorResult = await podOrchestrator.ResolvePodAsync(
                new OrchestratorRouteRequest
                {
                    OrganizationId = organizationId,
                    ModelName = modelName,
                    PreferredPodId = preferredPodId,
                },
                cancellationToken);

            if (orchestratorResult is not null
                && orchestratorResult.CurrentLoad < ApplicationConstants.SchedulerMaxConcurrentPerPod)
            {
                return new PodSelectionResult
                {
                    PodId = orchestratorResult.Pod.Id,
                    BaseUrl = orchestratorResult.BaseUrl,
                    Model = orchestratorResult.Model,
                    ModelId = null,
                    CurrentLoad = orchestratorResult.CurrentLoad,
                };
            }

            var route = await router.ResolveAsync(organizationId, modelName, cancellationToken);
            var candidates = new List<(GpuPod Pod, string BaseUrl, string? Model, Guid? ModelId)>
            {
                (route.Pod, route.BaseUrl, route.Model, null),
            };

            if (preferredPodId.HasValue && preferredPodId.Value != route.Pod.Id)
            {
                var preferred = await dbContext.GpuPods
                    .Where(p => p.OrganizationId == organizationId && p.Id == preferredPodId.Value)
                    .FirstOrDefaultAsync(cancellationToken);

                if (preferred is not null)
                {
                    try
                    {
                        var preferredRoute = await router.ResolveAsync(organizationId, modelName, cancellationToken);
                        candidates.Insert(0, (preferred, preferredRoute.BaseUrl, preferredRoute.Model, null));
                    }
                    catch
                    {
                        // Preferred pod unavailable for model.
                    }
                }
            }

            PodSelectionResult? best = null;
            foreach (var (pod, baseUrl, model, modelId) in candidates.DistinctBy(c => c.Pod.Id))
            {
                if (pod.Status is PodStatus.Deleted or PodStatus.Deleting)
                {
                    continue;
                }

                var load = await GetPodLoadAsync(organizationId, pod.Id, cancellationToken);
                if (load >= ApplicationConstants.SchedulerMaxConcurrentPerPod)
                {
                    continue;
                }

                if (best is null || load < best.CurrentLoad)
                {
                    best = new PodSelectionResult
                    {
                        PodId = pod.Id,
                        BaseUrl = baseUrl,
                        Model = model,
                        ModelId = modelId,
                        CurrentLoad = load,
                    };
                }
            }

            return best;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Pod selection failed for organization {OrganizationId}", organizationId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsPodAvailableAsync(
        Guid organizationId,
        Guid podId,
        CancellationToken cancellationToken = default)
    {
        var pod = await dbContext.GpuPods
            .Where(p => p.OrganizationId == organizationId && p.Id == podId)
            .Select(p => new { p.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (pod is null || pod.Status is PodStatus.Deleted or PodStatus.Deleting)
        {
            return false;
        }

        var load = await GetPodLoadAsync(organizationId, podId, cancellationToken);
        return load < ApplicationConstants.SchedulerMaxConcurrentPerPod;
    }

    /// <inheritdoc />
    public async Task<bool> TryReservePodAsync(
        Guid organizationId,
        Guid podId,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        if (redis is null)
        {
            return await IsPodAvailableAsync(organizationId, podId, cancellationToken);
        }

        var db = redis.GetDatabase();
        var key = SchedulerRedisKeys.PodLoad(organizationId, podId);
        var load = await db.StringIncrementAsync(key);
        if (load > ApplicationConstants.SchedulerMaxConcurrentPerPod)
        {
            await db.StringDecrementAsync(key);
            return false;
        }

        await db.StringSetAsync(SchedulerRedisKeys.Processing(requestId), $"{podId}", TimeSpan.FromHours(1));
        return true;
    }

    /// <inheritdoc />
    public async Task ReleasePodAsync(
        Guid organizationId,
        Guid podId,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        if (redis is null)
        {
            return;
        }

        var db = redis.GetDatabase();
        await db.StringDecrementAsync(SchedulerRedisKeys.PodLoad(organizationId, podId));
        await db.KeyDeleteAsync(SchedulerRedisKeys.Processing(requestId));
    }

    private async Task<int> GetPodLoadAsync(Guid organizationId, Guid podId, CancellationToken cancellationToken)
    {
        if (redis is not null)
        {
            var db = redis.GetDatabase();
            var value = await db.StringGetAsync(SchedulerRedisKeys.PodLoad(organizationId, podId));
            return value.HasValue && int.TryParse(value.ToString(), out var load) ? load : 0;
        }

        return await dbContext.GatewayRequests.CountAsync(
            r => r.OrganizationId == organizationId
                && r.GpuPodId == podId
                && (r.Status == GatewayRequestStatus.Forwarding
                    || r.Status == GatewayRequestStatus.Streaming
                    || r.Status == GatewayRequestStatus.Waking
                    || r.Status == GatewayRequestStatus.WaitingHealthy),
            cancellationToken);
    }

    private static bool IsRetryable(Exception ex) =>
        ex is TimeoutException or HttpRequestException or InvalidOperationException;
}
