using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Gateway;
using PodPilot.Application.Models.Scheduler;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Orchestrates end-to-end AI gateway request handling via the scheduler.
/// </summary>
public sealed class AiGateway : IAiGateway
{
    private readonly IRequestScheduler scheduler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiGateway"/> class.
    /// </summary>
    public AiGateway(IRequestScheduler scheduler)
    {
        this.scheduler = scheduler;
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
}
