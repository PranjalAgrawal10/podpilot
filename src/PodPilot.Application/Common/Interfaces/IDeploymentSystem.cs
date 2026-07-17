using PodPilot.Application.Models.Deployments;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Orchestrates one-click AI pod deployments.
/// </summary>
public interface IDeploymentService
{
    /// <summary>Creates a deployment and queues provisioning.</summary>
    Task<DeploymentDetail> CreateAsync(CreateDeploymentOptions options, CancellationToken cancellationToken = default);

    /// <summary>Lists deployments for an organization.</summary>
    Task<IReadOnlyList<DeploymentSummary>> ListAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>Gets a deployment by id.</summary>
    Task<DeploymentDetail> GetAsync(Guid organizationId, Guid deploymentId, CancellationToken cancellationToken = default);

    /// <summary>Requests deletion of a deployment and its pod.</summary>
    Task DeleteAsync(Guid organizationId, Guid deploymentId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Restarts a failed or ready deployment pipeline.</summary>
    Task<DeploymentDetail> RestartAsync(Guid organizationId, Guid deploymentId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Runs an immediate health check.</summary>
    Task<DeploymentHealthInfo> RunHealthCheckAsync(Guid organizationId, Guid deploymentId, CancellationToken cancellationToken = default);

    /// <summary>Gets dashboard aggregates.</summary>
    Task<DeploymentDashboardInfo> GetDashboardAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>Advances a single deployment state machine step (worker).</summary>
    Task ProcessPendingStepAsync(Guid deploymentId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Catalog and recommendation APIs for deployments.
/// </summary>
public interface IDeploymentCatalogService
{
    /// <summary>Ensures seed data exists.</summary>
    Task EnsureSeededAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists GPU catalog.</summary>
    Task<IReadOnlyList<GpuCatalogInfo>> ListGpusAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists model catalog.</summary>
    Task<IReadOnlyList<ModelCatalogInfo>> ListModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists deployment templates.</summary>
    Task<IReadOnlyList<DeploymentTemplateInfo>> ListTemplatesAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists regions for a provider (org-scoped compute provider).</summary>
    Task<IReadOnlyList<DeploymentRegionInfo>> ListRegionsAsync(
        Guid organizationId,
        Guid providerId,
        string? sortBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>Recommends GPUs for selected models.</summary>
    Task<GpuRecommendationResult> RecommendGpuAsync(
        IReadOnlyList<string> modelCodesOrReferences,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Inference runtime provider (Ollama, vLLM, llama.cpp).
/// </summary>
public interface IRuntimeProvider
{
    /// <summary>Gets the runtime kind.</summary>
    InferenceRuntimeKind Kind { get; }

    /// <summary>Gets the default listen port.</summary>
    int DefaultPort { get; }

    /// <summary>Gets the default health path.</summary>
    string HealthPath { get; }

    /// <summary>Gets the default container image for CUDA 12.x.</summary>
    string GetDefaultImage(string cudaVersion);

    /// <summary>Validates GPU / CUDA compatibility before deploy.</summary>
    Task ValidateAsync(RuntimeValidationContext context, CancellationToken cancellationToken = default);

    /// <summary>Ensures the runtime is installed and reachable on the pod.</summary>
    Task EnsureInstalledAsync(RuntimeExecutionContext context, CancellationToken cancellationToken = default);

    /// <summary>Pulls a model onto the runtime.</summary>
    Task PullModelAsync(
        RuntimeExecutionContext context,
        string modelReference,
        IProgress<int>? progress,
        CancellationToken cancellationToken = default);

    /// <summary>Checks whether a model is available.</summary>
    Task<bool> IsModelAvailableAsync(
        RuntimeExecutionContext context,
        string modelReference,
        CancellationToken cancellationToken = default);

    /// <summary>Runs runtime health probes.</summary>
    Task<RuntimeHealthResult> CheckHealthAsync(
        RuntimeExecutionContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolves runtime providers by kind.
/// </summary>
public interface IRuntimeProviderFactory
{
    /// <summary>Gets a runtime provider.</summary>
    IRuntimeProvider GetProvider(InferenceRuntimeKind kind);
}

/// <summary>
/// Cloud adapter for one-click deployments (RunPod today; extensible).
/// </summary>
public interface IDeploymentCloudAdapter
{
    /// <summary>Gets the cloud provider kind.</summary>
    DeploymentCloudProviderKind Kind { get; }

    /// <summary>Whether this adapter is implemented.</summary>
    bool IsImplemented { get; }

    /// <summary>Maps to <see cref="ProviderType"/>.</summary>
    Domain.Enums.ProviderType ToProviderType();
}

/// <summary>
/// Factory for cloud adapters.
/// </summary>
public interface IDeploymentCloudAdapterFactory
{
    /// <summary>Gets an adapter.</summary>
    IDeploymentCloudAdapter GetAdapter(DeploymentCloudProviderKind kind);
}

/// <summary>
/// Real-time deployment notifications.
/// </summary>
public interface IDeploymentNotificationService
{
    /// <summary>Notifies deployment started.</summary>
    Task NotifyStartedAsync(Guid organizationId, Guid deploymentId, CancellationToken cancellationToken = default);

    /// <summary>Notifies progress.</summary>
    Task NotifyProgressAsync(
        Guid organizationId,
        Guid deploymentId,
        DeploymentStatus status,
        int progressPercent,
        string? message,
        CancellationToken cancellationToken = default);

    /// <summary>Notifies model download progress.</summary>
    Task NotifyModelProgressAsync(
        Guid organizationId,
        Guid deploymentId,
        string modelReference,
        int progressPercent,
        CancellationToken cancellationToken = default);

    /// <summary>Notifies health update.</summary>
    Task NotifyHealthAsync(
        Guid organizationId,
        Guid deploymentId,
        DeploymentHealthState state,
        CancellationToken cancellationToken = default);

    /// <summary>Notifies ready.</summary>
    Task NotifyReadyAsync(Guid organizationId, Guid deploymentId, CancellationToken cancellationToken = default);

    /// <summary>Notifies failed.</summary>
    Task NotifyFailedAsync(
        Guid organizationId,
        Guid deploymentId,
        string error,
        CancellationToken cancellationToken = default);
}
