using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Gateway;
using PodPilot.Application.Models.Scheduler;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Scheduler;

/// <summary>
/// Enterprise request scheduler for the AI gateway.
/// </summary>
public sealed class RequestScheduler : IRequestScheduler
{
    private readonly IApplicationDbContext dbContext;
    private readonly IRequestQueue requestQueue;
    private readonly IRequestDispatcher dispatcher;
    private readonly IRequestTracker requestTracker;
    private readonly IRequestPriorityResolver priorityResolver;
    private readonly IGatewayRouter router;
    private readonly ISchedulerNotificationService notificationService;
    private readonly IGatewayNotificationService gatewayNotificationService;
    private readonly IDateTimeService dateTimeService;
    private readonly ILogger<RequestScheduler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestScheduler"/> class.
    /// </summary>
    public RequestScheduler(
        IApplicationDbContext dbContext,
        IRequestQueue requestQueue,
        IRequestDispatcher dispatcher,
        IRequestTracker requestTracker,
        IRequestPriorityResolver priorityResolver,
        IGatewayRouter router,
        ISchedulerNotificationService notificationService,
        IGatewayNotificationService gatewayNotificationService,
        IDateTimeService dateTimeService,
        ILogger<RequestScheduler> logger)
    {
        this.dbContext = dbContext;
        this.requestQueue = requestQueue;
        this.dispatcher = dispatcher;
        this.requestTracker = requestTracker;
        this.priorityResolver = priorityResolver;
        this.router = router;
        this.notificationService = notificationService;
        this.gatewayNotificationService = gatewayNotificationService;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<ScheduledRequestResult> ProcessAsync(
        ScheduleRequestContext context,
        CancellationToken cancellationToken = default)
    {
        var auth = context.Auth;
        var now = dateTimeService.UtcNow;

        if (context.Body.Length > ApplicationConstants.SchedulerMaxRequestBodyBytes)
        {
            return Failure("request_too_large", "Request body exceeds the maximum allowed size.", 413);
        }

        if (!string.IsNullOrWhiteSpace(context.ClientRequestId)
            && await requestQueue.IsDuplicateAsync(auth.OrganizationId, context.ClientRequestId, cancellationToken))
        {
            return Failure("duplicate_request", "A request with this identifier has already been submitted.", 409);
        }

        context.Body.Position = 0;
        var model = ExtractModel(context.Body);
        context.Body.Position = 0;

        var route = await router.ResolveAsync(auth.OrganizationId, model, cancellationToken);
        var isStreaming = DetectStreaming(context.Body);
        var priority = priorityResolver.Resolve(auth, context.Path, isStreaming);
        var bodyHash = await ComputeBodyHashAsync(context.Body, cancellationToken);
        context.Body.Position = 0;

        var gatewayRequest = new GatewayRequest
        {
            OrganizationId = auth.OrganizationId,
            UserId = auth.UserId,
            ApiKeyId = auth.ApiKeyId,
            GpuPodId = route.Pod.Id,
            Model = route.Model,
            HttpMethod = context.Method,
            Path = context.Path,
            Status = GatewayRequestStatus.Pending,
            Priority = priority,
            CreatedAt = now,
            StartedAt = now,
            CorrelationId = context.CorrelationId,
            ClientRequestId = context.ClientRequestId,
            UpstreamBaseUrl = route.BaseUrl,
            RequestBodyHash = bodyHash,
        };

        await dbContext.AddGatewayRequestAsync(gatewayRequest, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var atCapacity = !await dispatcher.IsPodAvailableAsync(auth.OrganizationId, route.Pod.Id, cancellationToken);
        var canExecuteImmediately = !atCapacity
            && (route.Pod.Status == PodStatus.Running || route.Pod.Status == PodStatus.Stopped);

        if (canExecuteImmediately
            && await dispatcher.TryReservePodAsync(auth.OrganizationId, route.Pod.Id, gatewayRequest.Id, cancellationToken))
        {
            return await ExecuteImmediatelyAsync(context, gatewayRequest, route, cancellationToken);
        }

        return await QueueAndWaitAsync(context, gatewayRequest, route, priority, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> CancelAsync(Guid requestId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        var request = await dbContext.GatewayRequests
            .Where(r => r.Id == requestId && r.OrganizationId == organizationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (request is null)
        {
            return false;
        }

        if (request.Status is GatewayRequestStatus.Completed or GatewayRequestStatus.Failed or GatewayRequestStatus.Cancelled)
        {
            return false;
        }

        await requestQueue.RemoveAsync(requestId, organizationId, cancellationToken);
        request.Status = GatewayRequestStatus.Cancelled;
        request.CompletedAt = dateTimeService.UtcNow;
        await AddSchedulerEventAsync(request, SchedulerEventType.Cancelled, "Request cancelled.", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        requestTracker.Cancel(requestId);
        await notificationService.NotifyRequestFailedAsync(organizationId, requestId, "Cancelled", cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RetryAsync(Guid requestId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        var request = await dbContext.GatewayRequests
            .Where(r => r.Id == requestId && r.OrganizationId == organizationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (request is null || request.Status != GatewayRequestStatus.Failed)
        {
            return false;
        }

        if (request.RetryCount >= ApplicationConstants.SchedulerMaxRetryAttempts)
        {
            return false;
        }

        request.RetryCount++;
        request.Status = GatewayRequestStatus.Queued;
        request.CompletedAt = null;
        request.StartedAt = dateTimeService.UtcNow;
        await AddSchedulerEventAsync(request, SchedulerEventType.Retried, $"Retry attempt {request.RetryCount}.", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var enqueue = await requestQueue.EnqueueAsync(
            new QueuedRequestItem
            {
                RequestId = request.Id,
                OrganizationId = organizationId,
                PodId = request.GpuPodId,
                Priority = request.Priority,
                EnqueuedAt = dateTimeService.UtcNow,
                ClientRequestId = request.ClientRequestId,
            },
            cancellationToken);

        return enqueue.Success;
    }

    /// <inheritdoc />
    public async Task CompleteAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await dbContext.GatewayRequests.FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);
        if (request is null)
        {
            return;
        }

        request.Status = GatewayRequestStatus.Completed;
        request.CompletedAt = dateTimeService.UtcNow;
        await dispatcher.ReleasePodAsync(request.OrganizationId, request.GpuPodId, requestId, cancellationToken);
        await AddSchedulerEventAsync(request, SchedulerEventType.Completed, "Request completed.", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyRequestCompletedAsync(request.OrganizationId, requestId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task FailAsync(Guid requestId, string errorMessage, CancellationToken cancellationToken = default)
    {
        var request = await dbContext.GatewayRequests.FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);
        if (request is null)
        {
            return;
        }

        request.Status = GatewayRequestStatus.Failed;
        request.CompletedAt = dateTimeService.UtcNow;
        await dispatcher.ReleasePodAsync(request.OrganizationId, request.GpuPodId, requestId, cancellationToken);
        await AddSchedulerEventAsync(request, SchedulerEventType.Failed, errorMessage, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyRequestFailedAsync(request.OrganizationId, requestId, errorMessage, cancellationToken);
    }

    /// <inheritdoc />
    public async Task TimeoutAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await dbContext.GatewayRequests.FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);
        if (request is null)
        {
            return;
        }

        request.Status = GatewayRequestStatus.TimedOut;
        request.CompletedAt = dateTimeService.UtcNow;
        await requestQueue.RemoveAsync(requestId, request.OrganizationId, cancellationToken);
        await dispatcher.ReleasePodAsync(request.OrganizationId, request.GpuPodId, requestId, cancellationToken);
        await AddSchedulerEventAsync(request, SchedulerEventType.TimedOut, "Request timed out.", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        requestTracker.Fail(requestId, "Request timed out.", 504);
        await notificationService.NotifyRequestFailedAsync(request.OrganizationId, requestId, "Timed out", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ReassignAsync(
        Guid requestId,
        Guid newPodId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var request = await dbContext.GatewayRequests
            .Where(r => r.Id == requestId && r.OrganizationId == organizationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (request is null)
        {
            return false;
        }

        var oldPodId = request.GpuPodId;
        request.GpuPodId = newPodId;
        await dispatcher.ReleasePodAsync(organizationId, oldPodId, requestId, cancellationToken);
        await AddSchedulerEventAsync(
            request,
            SchedulerEventType.Reassigned,
            $"Reassigned from pod {oldPodId} to {newPodId}.",
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Processes a dequeued request from background workers.
    /// </summary>
    public async Task ProcessQueuedItemAsync(QueuedRequestItem item, CancellationToken cancellationToken = default)
    {
        var request = await dbContext.GatewayRequests
            .FirstOrDefaultAsync(r => r.Id == item.RequestId && r.OrganizationId == item.OrganizationId, cancellationToken);

        if (request is null || request.Status is GatewayRequestStatus.Cancelled or GatewayRequestStatus.Completed)
        {
            return;
        }

        RequestPayloadStore.TryTake(item.RequestId, out var payload);

        var selection = await dispatcher.SelectPodAsync(item.OrganizationId, request.Model, item.PodId, cancellationToken);
        if (selection is null || !await dispatcher.TryReservePodAsync(item.OrganizationId, selection.PodId, request.Id, cancellationToken))
        {
            if (payload is not null)
            {
                RequestPayloadStore.Store(item.RequestId, payload);
            }

            await requestQueue.EnqueueAsync(item, cancellationToken);
            return;
        }

        request.GpuPodId = selection.PodId;
        request.UpstreamBaseUrl = selection.BaseUrl;
        request.Status = GatewayRequestStatus.Waking;
        request.StartedAt = dateTimeService.UtcNow;
        request.QueueTimeMs = (int)(request.StartedAt - request.CreatedAt).TotalMilliseconds;
        await AddSchedulerEventAsync(request, SchedulerEventType.Dispatched, "Request dispatched from queue.", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyRequestStartedAsync(item.OrganizationId, request.Id, selection.PodId, cancellationToken);

        DispatchContext dispatchContext;
        if (payload is not null)
        {
            payload.Body.Position = 0;
            dispatchContext = new DispatchContext
            {
                RequestId = request.Id,
                OrganizationId = item.OrganizationId,
                UserId = request.UserId,
                ApiKeyId = request.ApiKeyId,
                PodId = selection.PodId,
                BaseUrl = selection.BaseUrl,
                Path = payload.Path,
                Method = payload.Method,
                Headers = payload.Headers,
                Body = payload.Body,
                ResponseBody = payload.ResponseBody,
                Model = request.Model,
                AttemptNumber = request.RetryCount + 1,
                OnResponseHeaders = payload.OnResponseHeaders,
            };
        }
        else
        {
            dispatchContext = new DispatchContext
            {
                RequestId = request.Id,
                OrganizationId = item.OrganizationId,
                UserId = request.UserId,
                ApiKeyId = request.ApiKeyId,
                PodId = selection.PodId,
                BaseUrl = selection.BaseUrl,
                Path = request.Path,
                Method = request.HttpMethod,
                Headers = new Dictionary<string, string>(),
                Body = Stream.Null,
                Model = request.Model,
                AttemptNumber = request.RetryCount + 1,
            };
        }

        var executionStopwatch = Stopwatch.StartNew();
        var result = await dispatcher.DispatchAsync(dispatchContext, cancellationToken);
        executionStopwatch.Stop();
        request.ExecutionTimeMs = (int)executionStopwatch.ElapsedMilliseconds;
        await FinalizeDispatchAsync(request, result, cancellationToken);
        requestTracker.Complete(request.Id, result);
    }

    private async Task<ScheduledRequestResult> ExecuteImmediatelyAsync(
        ScheduleRequestContext context,
        GatewayRequest request,
        GatewayRouteResult route,
        CancellationToken cancellationToken)
    {
        var executionStopwatch = Stopwatch.StartNew();
        request.Status = GatewayRequestStatus.Forwarding;
        await AddSchedulerEventAsync(request, SchedulerEventType.Started, "Immediate execution.", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await gatewayNotificationService.NotifyRequestStartedAsync(
            context.Auth.OrganizationId,
            request.Id,
            route.Pod.Id,
            context.Path,
            cancellationToken);
        await notificationService.NotifyRequestStartedAsync(context.Auth.OrganizationId, request.Id, route.Pod.Id, cancellationToken);

        context.Body.Position = 0;
        var dispatchContext = new DispatchContext
        {
            RequestId = request.Id,
            OrganizationId = context.Auth.OrganizationId,
            UserId = context.Auth.UserId,
            ApiKeyId = context.Auth.ApiKeyId,
            PodId = route.Pod.Id,
            BaseUrl = route.BaseUrl,
            Path = context.Path,
            Method = context.Method,
            Headers = context.Headers,
            Body = context.Body,
            ResponseBody = context.ResponseBody,
            Model = route.Model,
            OnResponseHeaders = context.OnResponseHeaders,
        };

        var result = await dispatcher.DispatchAsync(dispatchContext, cancellationToken);
        executionStopwatch.Stop();
        request.ExecutionTimeMs = (int)executionStopwatch.ElapsedMilliseconds;
        await FinalizeDispatchAsync(request, result, cancellationToken);

        return ToScheduledResult(request.Id, result);
    }

    private async Task<ScheduledRequestResult> QueueAndWaitAsync(
        ScheduleRequestContext context,
        GatewayRequest request,
        GatewayRouteResult route,
        RequestPriority priority,
        CancellationToken cancellationToken)
    {
        request.Status = GatewayRequestStatus.Queued;
        await AddSchedulerEventAsync(request, SchedulerEventType.Queued, "Request queued waiting for pod.", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var enqueue = await requestQueue.EnqueueAsync(
            new QueuedRequestItem
            {
                RequestId = request.Id,
                OrganizationId = context.Auth.OrganizationId,
                PodId = route.Pod.Id,
                Priority = priority,
                EnqueuedAt = dateTimeService.UtcNow,
                ClientRequestId = context.ClientRequestId,
            },
            cancellationToken);

        if (!enqueue.Success)
        {
            return Failure("queue_full", enqueue.ErrorMessage ?? "Queue is full.", 503);
        }

        await notificationService.NotifyRequestQueuedAsync(context.Auth.OrganizationId, request.Id, enqueue.Position, cancellationToken);
        var queueLength = await requestQueue.GetLengthAsync(context.Auth.OrganizationId, cancellationToken);
        await notificationService.NotifyQueueUpdatedAsync(context.Auth.OrganizationId, queueLength, cancellationToken);

        await dbContext.AddRequestQueueEntryAsync(
            new RequestQueueEntry
            {
                GatewayRequestId = request.Id,
                OrganizationId = context.Auth.OrganizationId,
                Priority = priority,
                EnqueuedAt = dateTimeService.UtcNow,
                Position = enqueue.Position,
                ClientRequestId = context.ClientRequestId,
            },
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var waitHandle = await requestTracker.RegisterWaitAsync(
            request.Id,
            ApplicationConstants.SchedulerQueueTimeout,
            cancellationToken);

        context.Body.Position = 0;
        await StoreRequestPayloadAsync(request.Id, context, cancellationToken);

        DispatchResult dispatchResult;
        try
        {
            dispatchResult = await waitHandle.CompletionTask;
        }
        catch (OperationCanceledException)
        {
            await TimeoutAsync(request.Id, cancellationToken);
            throw;
        }
        finally
        {
            waitHandle.LinkedCancellation?.Dispose();
        }

        if (context.ResponseBody.CanWrite && dispatchResult.Success)
        {
            // Response already written during dispatch for waited requests processed by worker with stored payload.
        }

        return ToScheduledResult(request.Id, dispatchResult, wasQueued: true);
    }

    private async Task FinalizeDispatchAsync(
        GatewayRequest request,
        DispatchResult result,
        CancellationToken cancellationToken)
    {
        if (result.Success)
        {
            request.Status = result.IsStreaming ? GatewayRequestStatus.Streaming : GatewayRequestStatus.Completed;
            request.IsStreaming = result.IsStreaming;
            request.CompletedAt = dateTimeService.UtcNow;
            request.Latency = new GatewayLatency
            {
                GatewayRequestId = request.Id,
                WakeLatencyMs = result.WakeLatencyMs,
                HealthCheckLatencyMs = result.HealthCheckLatencyMs,
                ForwardLatencyMs = result.ForwardLatencyMs,
                TotalLatencyMs = (result.WakeLatencyMs ?? 0) + (result.HealthCheckLatencyMs ?? 0) + result.ForwardLatencyMs,
            };

            await dbContext.AddRequestExecutionAsync(
                new RequestExecution
                {
                    GatewayRequestId = request.Id,
                    GpuPodId = request.GpuPodId,
                    AttemptNumber = request.RetryCount + 1,
                    Status = result.IsStreaming ? SchedulerRequestStatus.Streaming : SchedulerRequestStatus.Completed,
                    StartedAt = request.StartedAt,
                    CompletedAt = request.CompletedAt,
                },
                cancellationToken);

            await AddSchedulerEventAsync(
                request,
                result.IsStreaming ? SchedulerEventType.Streaming : SchedulerEventType.Completed,
                "Request execution finished.",
                cancellationToken);

            await gatewayNotificationService.NotifyRequestFinishedAsync(
                request.OrganizationId,
                request.Id,
                request.Status.ToString(),
                request.Latency?.TotalLatencyMs ?? 0,
                cancellationToken);
        }
        else
        {
            request.Status = GatewayRequestStatus.Failed;
            request.CompletedAt = dateTimeService.UtcNow;
            request.Error = new GatewayError
            {
                GatewayRequestId = request.Id,
                ErrorFormat = GatewayErrorFormat.OpenAi,
                ErrorCode = "dispatch_failed",
                Message = result.ErrorMessage ?? "Dispatch failed.",
                OccurredAt = dateTimeService.UtcNow,
            };

            await AddSchedulerEventAsync(request, SchedulerEventType.Failed, result.ErrorMessage ?? "Dispatch failed.", cancellationToken);
            await gatewayNotificationService.NotifyGatewayErrorAsync(
                request.OrganizationId,
                request.Id,
                "dispatch_failed",
                result.ErrorMessage ?? "Dispatch failed.",
                cancellationToken);
        }

        await dispatcher.ReleasePodAsync(request.OrganizationId, request.GpuPodId, request.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AddSchedulerEventAsync(
        GatewayRequest request,
        SchedulerEventType eventType,
        string message,
        CancellationToken cancellationToken)
    {
        await dbContext.AddSchedulerEventAsync(
            new SchedulerEvent
            {
                GatewayRequestId = request.Id,
                OrganizationId = request.OrganizationId,
                EventType = eventType,
                Message = message,
                Timestamp = dateTimeService.UtcNow,
            },
            cancellationToken);

        logger.LogInformation(
            "Scheduler {EventType} for request {RequestId}: {Message}",
            eventType,
            request.Id,
            message);
    }

    private static ScheduledRequestResult ToScheduledResult(
        Guid requestId,
        DispatchResult result,
        bool wasQueued = false) =>
        new()
        {
            Success = result.Success,
            StatusCode = result.StatusCode,
            RequestId = requestId,
            WasQueued = wasQueued,
            Headers = result.Headers,
            ErrorMessage = result.ErrorMessage,
            ErrorCode = result.Success ? null : "dispatch_failed",
        };

    private static ScheduledRequestResult Failure(string code, string message, int statusCode) =>
        new()
        {
            Success = false,
            ErrorCode = code,
            ErrorMessage = message,
            StatusCode = statusCode,
        };

    private static string? ExtractModel(Stream body)
    {
        if (body is not MemoryStream memory || memory.Length == 0)
        {
            return null;
        }

        var content = Encoding.UTF8.GetString(memory.ToArray());
        try
        {
            using var document = JsonDocument.Parse(content);
            return document.RootElement.TryGetProperty("model", out var modelProp)
                ? modelProp.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool DetectStreaming(Stream body)
    {
        if (body is not MemoryStream memory || memory.Length == 0)
        {
            return false;
        }

        var content = Encoding.UTF8.GetString(memory.ToArray());
        try
        {
            using var document = JsonDocument.Parse(content);
            return document.RootElement.TryGetProperty("stream", out var streamProp)
                && streamProp.ValueKind == JsonValueKind.True;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static async Task<string> ComputeBodyHashAsync(Stream body, CancellationToken cancellationToken)
    {
        body.Position = 0;
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(body, cancellationToken);
        return Convert.ToHexString(hash);
    }

    private static Task StoreRequestPayloadAsync(Guid requestId, ScheduleRequestContext context, CancellationToken cancellationToken)
    {
        // Payload is re-read from buffered body by dispatch worker through in-memory store.
        RequestPayloadStore.Store(requestId, context);
        return Task.CompletedTask;
    }
}
