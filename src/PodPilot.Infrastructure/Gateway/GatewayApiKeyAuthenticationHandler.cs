using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Gateway;

namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Authenticates AI gateway requests using API keys.
/// </summary>
public sealed class GatewayApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayApiKeyAuthenticationHandler"/> class.
    /// </summary>
    public GatewayApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    /// <inheritdoc />
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var apiKey = ExtractApiKey(Request);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return AuthenticateResult.NoResult();
        }

        var apiKeyService = Context.RequestServices.GetRequiredService<IGatewayApiKeyService>();
        var authContext = await apiKeyService.ValidateKeyAsync(apiKey, Context.RequestAborted);
        if (authContext is null)
        {
            return AuthenticateResult.Fail("Invalid API key.");
        }

        Context.Items[GatewayAuthConstants.AuthContextItemKey] = authContext;

        var claims = new List<System.Security.Claims.Claim>
        {
            new("gateway_api_key_id", authContext.ApiKeyId.ToString()),
            new(ApplicationConstants.OrganizationIdClaim, authContext.OrganizationId.ToString()),
        };

        if (authContext.UserId.HasValue)
        {
            claims.Add(new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, authContext.UserId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, GatewayAuthConstants.SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, GatewayAuthConstants.SchemeName);
        return AuthenticateResult.Success(ticket);
    }

    private static string? ExtractApiKey(HttpRequest request)
    {
        if (request.Headers.TryGetValue("x-api-key", out var headerKey)
            && !string.IsNullOrWhiteSpace(headerKey))
        {
            return headerKey.ToString();
        }

        var authorization = request.Headers.Authorization.ToString();
        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authorization["Bearer ".Length..].Trim();
        }

        return null;
    }
}
