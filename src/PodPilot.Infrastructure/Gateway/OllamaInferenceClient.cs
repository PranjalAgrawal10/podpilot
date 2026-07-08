using System.Diagnostics;
using System.Net.Http.Headers;
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
    public async Task<bool> IsHealthyAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(nameof(OllamaInferenceClient));
        client.Timeout = TimeSpan.FromSeconds(10);

        try
        {
            using var tagsResponse = await client.GetAsync(
                GatewayUrlHelper.Combine(baseUrl, "/api/tags"),
                cancellationToken);

            if (!tagsResponse.IsSuccessStatusCode)
            {
                return false;
            }

            using var versionResponse = await client.GetAsync(
                GatewayUrlHelper.Combine(baseUrl, "/api/version"),
                cancellationToken);

            return versionResponse.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Ollama health check failed for {BaseUrl}", baseUrl);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> WaitForHealthyAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < ApplicationConstants.MaxOllamaHealthCheckAttempts; attempt++)
        {
            if (attempt > 0)
            {
                await Task.Delay(ApplicationConstants.OllamaHealthCheckInterval, cancellationToken);
            }

            if (await IsHealthyAsync(baseUrl, cancellationToken))
            {
                return true;
            }
        }

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
}
