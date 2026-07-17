using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Deployments;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Deployments.Runtimes;

/// <summary>
/// llama.cpp server inference runtime provider.
/// </summary>
public sealed class LlamaCppRuntimeProvider : IRuntimeProvider
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<LlamaCppRuntimeProvider> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LlamaCppRuntimeProvider"/> class.
    /// </summary>
    public LlamaCppRuntimeProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<LlamaCppRuntimeProvider> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    public InferenceRuntimeKind Kind => InferenceRuntimeKind.LlamaCpp;

    /// <inheritdoc />
    public int DefaultPort => 8080;

    /// <inheritdoc />
    public string HealthPath => "/health";

    /// <inheritdoc />
    public string GetDefaultImage(string cudaVersion) => "ghcr.io/ggerganov/llama.cpp:server-cuda";

    /// <inheritdoc />
    public Task ValidateAsync(RuntimeValidationContext context, CancellationToken cancellationToken = default)
    {
        if (!context.CudaVersion.StartsWith("12.", StringComparison.Ordinal))
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(context.CudaVersion), "llama.cpp deployments require CUDA 12.x."),
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
        var client = httpClientFactory.CreateClient(nameof(LlamaCppRuntimeProvider));
        var deadline = DateTime.UtcNow.AddMinutes(5);

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var response = await client.GetAsync(Combine(context.BaseUrl, HealthPath), cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                logger.LogDebug(ex, "Waiting for llama.cpp at {BaseUrl}", context.BaseUrl);
            }

            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        }

        throw new InvalidOperationException($"llama.cpp runtime did not become healthy at {context.BaseUrl}.");
    }

    /// <inheritdoc />
    public Task PullModelAsync(
        RuntimeExecutionContext context,
        string modelReference,
        IProgress<int>? progress,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "llama.cpp PullModel is a no-op; GGUF weights for {Model} expected via mount (deployment {DeploymentId}).",
            modelReference,
            context.DeploymentId);
        progress?.Report(100);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> IsModelAvailableAsync(
        RuntimeExecutionContext context,
        string modelReference,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(!string.IsNullOrWhiteSpace(modelReference));

    /// <inheritdoc />
    public async Task<RuntimeHealthResult> CheckHealthAsync(
        RuntimeExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(nameof(LlamaCppRuntimeProvider));
            var response = await client.GetAsync(Combine(context.BaseUrl, HealthPath), cancellationToken);
            var healthy = response.IsSuccessStatusCode;
            return new RuntimeHealthResult
            {
                RuntimeRunning = healthy,
                GpuAvailable = healthy,
                CudaAvailable = healthy,
                StreamingWorks = healthy,
                Message = healthy ? "llama.cpp healthy." : $"llama.cpp health returned {(int)response.StatusCode}.",
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "llama.cpp health check failed for {BaseUrl}", context.BaseUrl);
            return new RuntimeHealthResult { RuntimeRunning = false, Message = ex.Message };
        }
    }

    private static string Combine(string baseUrl, string path)
    {
        var normalizedBase = baseUrl.TrimEnd('/');
        var normalizedPath = path.StartsWith('/') ? path : $"/{path}";
        return $"{normalizedBase}{normalizedPath}";
    }
}
