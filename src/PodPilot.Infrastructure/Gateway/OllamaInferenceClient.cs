using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Ollama inference client for health checks and model listing.
/// </summary>
public sealed class OllamaInferenceClient : IInferenceClient
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<OllamaInferenceClient> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaInferenceClient"/> class.
    /// </summary>
    public OllamaInferenceClient(
        IHttpClientFactory httpClientFactory,
        ILogger<OllamaInferenceClient> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> IsHealthyAsync(
        string baseUrl,
        CancellationToken cancellationToken = default,
        TimeSpan? requestTimeout = null)
    {
        var timeout = requestTimeout ?? ApplicationConstants.OllamaHealthCheckTimeout;
        var (success, _) = await TryHealthCheckAsync(baseUrl, timeout, cancellationToken);
        return success;
    }

    /// <inheritdoc />
    public async Task<bool> WaitForHealthyAsync(
        string baseUrl,
        CancellationToken cancellationToken = default,
        int? maxAttempts = null,
        TimeSpan? checkInterval = null,
        TimeSpan? requestTimeout = null)
    {
        var attempts = maxAttempts ?? ApplicationConstants.MaxOllamaHealthCheckAttempts;
        var interval = checkInterval ?? ApplicationConstants.OllamaHealthCheckInterval;
        var timeout = requestTimeout ?? ApplicationConstants.OllamaHealthCheckTimeout;

        for (var attempt = 0; attempt < attempts; attempt++)
        {
            if (attempt > 0)
            {
                await Task.Delay(interval, cancellationToken);
            }

            var (success, immediatelyUnreachable) = await TryHealthCheckAsync(
                baseUrl,
                timeout,
                cancellationToken);

            if (success)
            {
                return true;
            }

            if (immediatelyUnreachable)
            {
                logger.LogDebug(
                    "Ollama at {BaseUrl} is unreachable; skipping further health checks",
                    baseUrl);
                break;
            }
        }

        logger.LogWarning(
            "Ollama did not become healthy at {BaseUrl} after {Attempts} attempt(s)",
            baseUrl,
            attempts);

        return false;
    }

    /// <inheritdoc />
    public async Task<string> GetModelsAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(nameof(OllamaInferenceClient));
        client.Timeout = TimeSpan.FromSeconds(30);

        using var response = await client.GetAsync(
            GatewayUrlHelper.Combine(baseUrl, "/api/tags"),
            cancellationToken);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private async Task<(bool Success, bool ImmediatelyUnreachable)> TryHealthCheckAsync(
        string baseUrl,
        TimeSpan requestTimeout,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(nameof(OllamaInferenceClient));
        client.Timeout = requestTimeout;

        try
        {
            using var tagsResponse = await client.GetAsync(
                GatewayUrlHelper.Combine(baseUrl, "/api/tags"),
                cancellationToken);

            if (!tagsResponse.IsSuccessStatusCode)
            {
                return (false, false);
            }

            using var versionResponse = await client.GetAsync(
                GatewayUrlHelper.Combine(baseUrl, "/api/version"),
                cancellationToken);

            return (versionResponse.IsSuccessStatusCode, false);
        }
        catch (Exception ex)
        {
            logger.LogDebug(
                "Ollama health check failed for {BaseUrl}: {Error}",
                baseUrl,
                ex.Message);

            return (false, IsImmediatelyUnreachable(ex));
        }
    }

    private static bool IsImmediatelyUnreachable(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is SocketException { SocketErrorCode: SocketError.ConnectionRefused })
            {
                return true;
            }
        }

        return false;
    }
}
