using System.Diagnostics;

namespace PodPilot.Api.Middleware;

/// <summary>
/// Middleware that logs HTTP request and response metadata.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<RequestLoggingMiddleware> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware delegate.</param>
    /// <param name="logger">The logger instance.</param>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation(
            "HTTP {Method} {Path} started. CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        await next(context);

        stopwatch.Stop();

        logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms. CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId);
    }
}
