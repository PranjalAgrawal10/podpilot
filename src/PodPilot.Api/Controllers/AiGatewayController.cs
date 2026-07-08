using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Gateway;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Gateway;

namespace PodPilot.Api.Controllers;

/// <summary>
/// OpenAI and Anthropic compatible AI gateway endpoints.
/// </summary>
[ApiController]
[Route("v1")]
[Authorize(AuthenticationSchemes = GatewayAuthConstants.SchemeName)]
public sealed class AiGatewayController : ControllerBase
{
    private readonly IAiGateway aiGateway;
    private readonly IGatewayRouter gatewayRouter;
    private readonly IInferenceClient inferenceClient;
    private readonly IGatewayRateLimitService rateLimitService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiGatewayController"/> class.
    /// </summary>
    public AiGatewayController(
        IAiGateway aiGateway,
        IGatewayRouter gatewayRouter,
        IInferenceClient inferenceClient,
        IGatewayRateLimitService rateLimitService)
    {
        this.aiGateway = aiGateway;
        this.gatewayRouter = gatewayRouter;
        this.inferenceClient = inferenceClient;
        this.rateLimitService = rateLimitService;
    }

    /// <summary>
    /// OpenAI-compatible chat completions endpoint.
    /// </summary>
    [HttpPost("chat/completions")]
    public Task ProxyOpenAiChat(CancellationToken cancellationToken) =>
        ProxyAsync("/v1/chat/completions", GatewayErrorFormat.OpenAi, cancellationToken);

    /// <summary>
    /// OpenAI-compatible responses endpoint.
    /// </summary>
    [HttpPost("responses")]
    public Task ProxyOpenAiResponses(CancellationToken cancellationToken) =>
        ProxyAsync("/v1/responses", GatewayErrorFormat.OpenAi, cancellationToken);

    /// <summary>
    /// Anthropic-compatible messages endpoint.
    /// </summary>
    [HttpPost("messages")]
    public Task ProxyAnthropicMessages(CancellationToken cancellationToken) =>
        ProxyAsync("/v1/messages", GatewayErrorFormat.Anthropic, cancellationToken);

    /// <summary>
    /// OpenAI-compatible models endpoint.
    /// </summary>
    [HttpGet("models")]
    public Task ListOpenAiModels(CancellationToken cancellationToken) =>
        ListModelsAsync(GatewayErrorFormat.OpenAi, cancellationToken);

    private async Task ProxyAsync(
        string upstreamPath,
        GatewayErrorFormat errorFormat,
        CancellationToken cancellationToken)
    {
        var auth = GetAuthContext();
        var rateLimit = rateLimitService.TryAcquire(auth);
        if (!rateLimit.Allowed)
        {
            Response.Headers.RetryAfter = rateLimit.RetryAfterSeconds.ToString();
            await GatewayErrorMapper.WriteErrorAsync(
                HttpContext,
                errorFormat,
                StatusCodes.Status429TooManyRequests,
                "rate_limit_exceeded",
                "Rate limit exceeded for this API key.",
                cancellationToken);
            return;
        }

        var headers = Request.Headers
            .Where(h => !string.Equals(h.Key, "Host", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        var result = await aiGateway.HandleRequestAsync(
            auth,
            upstreamPath,
            Request.Method,
            headers,
            Request.Body,
            Response.Body,
            errorFormat,
            GetCorrelationId(),
            ApplyResponseHeadersAsync,
            cancellationToken);

        if (!result.Success)
        {
            if (!Response.HasStarted)
            {
                await GatewayErrorMapper.WriteErrorAsync(
                    HttpContext,
                    errorFormat,
                    result.StatusCode,
                    result.ErrorCode ?? "gateway_error",
                    result.ErrorMessage ?? "Gateway request failed.",
                    cancellationToken);
            }
        }
    }

    private Task ApplyResponseHeadersAsync(GatewayProxyResult proxyResult)
    {
        Response.StatusCode = proxyResult.StatusCode;
        foreach (var header in proxyResult.Headers)
        {
            if (GatewayHeaderFilter.ShouldSkip(header.Key))
            {
                continue;
            }

            Response.Headers[header.Key] = header.Value;
        }

        return Task.CompletedTask;
    }

    private async Task ListModelsAsync(GatewayErrorFormat format, CancellationToken cancellationToken)
    {
        var auth = GetAuthContext();
        var rateLimit = rateLimitService.TryAcquire(auth);
        if (!rateLimit.Allowed)
        {
            Response.Headers.RetryAfter = rateLimit.RetryAfterSeconds.ToString();
            await GatewayErrorMapper.WriteErrorAsync(
                HttpContext,
                format,
                StatusCodes.Status429TooManyRequests,
                "rate_limit_exceeded",
                "Rate limit exceeded for this API key.",
                cancellationToken);
            return;
        }

        try
        {
            var route = await gatewayRouter.ResolveAsync(auth.OrganizationId, null, cancellationToken);
            var tagsJson = await inferenceClient.GetModelsAsync(route.BaseUrl, cancellationToken);
            var payload = GatewayModelsFormatter.Format(tagsJson, format);
            Response.StatusCode = StatusCodes.Status200OK;
            Response.ContentType = "application/json";
            await Response.WriteAsync(payload, cancellationToken);
        }
        catch (Exception)
        {
            await GatewayErrorMapper.WriteErrorAsync(
                HttpContext,
                format,
                StatusCodes.Status502BadGateway,
                "gateway_error",
                "Failed to list models from the inference backend.",
                cancellationToken);
        }
    }

    private GatewayAuthContext GetAuthContext()
    {
        if (HttpContext.Items.TryGetValue(GatewayAuthConstants.AuthContextItemKey, out var value)
            && value is GatewayAuthContext auth)
        {
            return auth;
        }

        throw new UnauthorizedAccessException("Gateway API key is required.");
    }

    private string? GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString();
}
