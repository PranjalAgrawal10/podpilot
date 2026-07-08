using System.Text.Json;
using Microsoft.AspNetCore.Http;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Maps gateway errors to provider-compatible envelopes.
/// </summary>
public static class GatewayErrorMapper
{
    /// <summary>
    /// Writes an error response in the requested format.
    /// </summary>
    public static async Task WriteErrorAsync(
        HttpContext httpContext,
        GatewayErrorFormat format,
        int statusCode,
        string code,
        string message,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        object payload = format switch
        {
            GatewayErrorFormat.Anthropic => new
            {
                type = "error",
                error = new { type = code, message },
            },
            _ => new
            {
                error = new
                {
                    message,
                    type = code,
                    code,
                },
            },
        };

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(payload),
            cancellationToken);
    }
}
