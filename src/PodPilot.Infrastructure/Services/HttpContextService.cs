using Microsoft.AspNetCore.Http;
using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// HTTP context accessor for request metadata.
/// </summary>
public sealed class HttpContextService : IHttpContextService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpContextService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public HttpContextService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public string? IpAddress
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            if (context is null)
            {
                return null;
            }

            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            return context.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }
    }

    /// <inheritdoc />
    public string? CorrelationId =>
        httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString()
        ?? httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-Id"].FirstOrDefault();
}
