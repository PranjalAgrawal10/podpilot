using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Deployments;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Deployments.Runtimes;

/// <summary>
/// Ollama inference runtime provider.
/// </summary>
public sealed class OllamaRuntimeProvider : IRuntimeProvider
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IOllamaClient ollamaClient;
    private readonly ILogger<OllamaRuntimeProvider> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaRuntimeProvider"/> class.
    /// </summary>
    public OllamaRuntimeProvider(
        IHttpClientFactory httpClientFactory,
        IOllamaClient ollamaClient,
        ILogger<OllamaRuntimeProvider> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.ollamaClient = ollamaClient;
        this.logger = logger;
    }

    /// <inheritdoc />
    public InferenceRuntimeKind Kind => InferenceRuntimeKind.Ollama;

    /// <inheritdoc />
    public int DefaultPort => 11434;

    /// <inheritdoc />
    public string HealthPath => "/api/tags";

    /// <inheritdoc />
    public string GetDefaultImage(string cudaVersion) => "ollama/ollama:latest";

    /// <inheritdoc />
    public Task ValidateAsync(RuntimeValidationContext context, CancellationToken cancellationToken = default)
    {
        if (!context.CudaVersion.StartsWith("12.", StringComparison.Ordinal))
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(context.CudaVersion), "Ollama deployments require CUDA 12.x."),
            ]);
        }

        if (context.GpuVramGb < context.RequiredVramGb)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    "GpuVramGb",
                    $"GPU VRAM {context.GpuVramGb}GB is below required {context.RequiredVramGb}GB for selected models."),
            ]);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task EnsureInstalledAsync(RuntimeExecutionContext context, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(nameof(OllamaRuntimeProvider));
        var deadline = DateTime.UtcNow.AddMinutes(5);

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var response = await client.GetAsync(
                    Combine(context.BaseUrl, HealthPath),
                    cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation(
                        "Ollama runtime healthy for deployment {DeploymentId}",
                        context.DeploymentId);
                    return;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                logger.LogDebug(ex, "Waiting for Ollama at {BaseUrl}", context.BaseUrl);
            }

            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        }

        throw new InvalidOperationException($"Ollama runtime did not become healthy at {context.BaseUrl}.");
    }

    /// <inheritdoc />
    public async Task PullModelAsync(
        RuntimeExecutionContext context,
        string modelReference,
        IProgress<int>? progress,
        CancellationToken cancellationToken = default)
    {
        await ollamaClient.PullModelAsync(
            context.BaseUrl,
            modelReference,
            p =>
            {
                if (p.Completed.HasValue && p.Total.HasValue && p.Total.Value > 0)
                {
                    var percent = (int)Math.Clamp(
                        p.Completed.Value * 100L / p.Total.Value,
                        0,
                        100);
                    progress?.Report(percent);
                }
                else if (!string.IsNullOrWhiteSpace(p.Status)
                         && p.Status.Contains("success", StringComparison.OrdinalIgnoreCase))
                {
                    progress?.Report(100);
                }

                return Task.CompletedTask;
            },
            cancellationToken);

        progress?.Report(100);
    }

    /// <inheritdoc />
    public async Task<bool> IsModelAvailableAsync(
        RuntimeExecutionContext context,
        string modelReference,
        CancellationToken cancellationToken = default)
    {
        var tags = await ollamaClient.GetTagsAsync(context.BaseUrl, cancellationToken);
        return tags.Any(t =>
            string.Equals(t.Name, modelReference, StringComparison.OrdinalIgnoreCase)
            || t.Name.StartsWith(modelReference + ":", StringComparison.OrdinalIgnoreCase)
            || modelReference.StartsWith(t.Name, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public async Task<RuntimeHealthResult> CheckHealthAsync(
        RuntimeExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reachable = await ollamaClient.IsReachableAsync(context.BaseUrl, cancellationToken);
            if (!reachable)
            {
                return new RuntimeHealthResult
                {
                    RuntimeRunning = false,
                    Message = "Ollama health endpoint unreachable.",
                };
            }

            return new RuntimeHealthResult
            {
                RuntimeRunning = true,
                GpuAvailable = true,
                CudaAvailable = true,
                StreamingWorks = true,
                Message = "Ollama healthy.",
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Ollama health check failed for {BaseUrl}", context.BaseUrl);
            return new RuntimeHealthResult
            {
                RuntimeRunning = false,
                Message = ex.Message,
            };
        }
    }

    private static string Combine(string baseUrl, string path)
    {
        var normalizedBase = baseUrl.TrimEnd('/');
        var normalizedPath = path.StartsWith('/') ? path : $"/{path}";
        return $"{normalizedBase}{normalizedPath}";
    }
}
