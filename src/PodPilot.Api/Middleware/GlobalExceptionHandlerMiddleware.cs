using System.Net;
using System.Text.Json;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Contracts.Common;

namespace PodPilot.Api.Middleware;

/// <summary>
/// Global exception handling middleware.
/// </summary>
public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> logger;
    private readonly IHostEnvironment environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandlerMiddleware"/> class.
    /// </summary>
    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        this.next = next;
        this.logger = logger;
        this.environment = environment;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString();

        logger.LogError(
            exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}",
            correlationId);

        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ApiResponse<object>
                {
                    Success = false,
                    Message = validationEx.Message,
                    Errors = validationEx.Errors,
                    CorrelationId = correlationId,
                }),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.Fail(notFoundEx.Message, correlationId)),
            UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.Fail(unauthorizedEx.Message, correlationId)),
            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail(
                    environment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred.",
                    correlationId)),
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
