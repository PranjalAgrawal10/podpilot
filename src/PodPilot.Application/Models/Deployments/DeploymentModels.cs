using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Deployments;

/// <summary>Options to create a one-click deployment.</summary>
public sealed class CreateDeploymentOptions
{
    /// <summary>Gets or sets organization id.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets requesting user id.</summary>
    public Guid UserId { get; init; }

    /// <summary>Gets or sets name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets compute provider id.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets region.</summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>Gets or sets GPU catalog code.</summary>
    public string GpuCode { get; init; } = string.Empty;

    /// <summary>Gets or sets optional provider GPU id override.</summary>
    public string? ProviderGpuId { get; init; }

    /// <summary>Gets or sets runtime (default Ollama).</summary>
    public InferenceRuntimeKind Runtime { get; init; } = InferenceRuntimeKind.Ollama;

    /// <summary>Gets or sets model references or catalog codes.</summary>
    public IReadOnlyList<string> Models { get; init; } = [];

    /// <summary>Gets or sets optional template code.</summary>
    public string? TemplateCode { get; init; }

    /// <summary>Gets or sets environment variables.</summary>
    public IReadOnlyDictionary<string, string>? EnvironmentVariables { get; init; }
}

/// <summary>Deployment list item.</summary>
public class DeploymentSummary
{
    /// <summary>Gets or sets id.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets status.</summary>
    public DeploymentStatus Status { get; init; }

    /// <summary>Gets or sets runtime.</summary>
    public InferenceRuntimeKind Runtime { get; init; }

    /// <summary>Gets or sets GPU code.</summary>
    public string GpuCode { get; init; } = string.Empty;

    /// <summary>Gets or sets region.</summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>Gets or sets progress.</summary>
    public int ProgressPercent { get; init; }

    /// <summary>Gets or sets status message.</summary>
    public string? StatusMessage { get; init; }

    /// <summary>Gets or sets health state.</summary>
    public DeploymentHealthState HealthState { get; init; }

    /// <summary>Gets or sets estimated hourly cost.</summary>
    public decimal EstimatedHourlyCostUsd { get; init; }

    /// <summary>Gets or sets created at.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets or sets pod id.</summary>
    public Guid? GpuPodId { get; init; }
}

/// <summary>Full deployment detail.</summary>
public sealed class DeploymentDetail : DeploymentSummary
{
    /// <summary>Gets or sets provider id.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets cloud provider.</summary>
    public DeploymentCloudProviderKind CloudProvider { get; init; }

    /// <summary>Gets or sets CUDA version.</summary>
    public string CudaVersion { get; init; } = "12.4";

    /// <summary>Gets or sets image.</summary>
    public string? ImageName { get; init; }

    /// <summary>Gets or sets error.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Gets or sets gateway route id.</summary>
    public Guid? GatewayRouteId { get; init; }

    /// <summary>Gets or sets models.</summary>
    public IReadOnlyList<DeploymentModelInfo> Models { get; init; } = [];

    /// <summary>Gets or sets recent logs.</summary>
    public IReadOnlyList<DeploymentLogInfo> Logs { get; init; } = [];

    /// <summary>Gets or sets health.</summary>
    public DeploymentHealthInfo? Health { get; init; }

    /// <summary>Gets or sets ready at.</summary>
    public DateTime? ReadyAt { get; init; }
}

/// <summary>Deployment model row.</summary>
public sealed class DeploymentModelInfo
{
    /// <summary>Gets or sets id.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets reference.</summary>
    public string ModelReference { get; init; } = string.Empty;

    /// <summary>Gets or sets download status.</summary>
    public DeploymentStatus DownloadStatus { get; init; }

    /// <summary>Gets or sets progress.</summary>
    public int ProgressPercent { get; init; }

    /// <summary>Gets or sets whether primary.</summary>
    public bool IsPrimary { get; init; }

    /// <summary>Gets or sets error.</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>Deployment log row.</summary>
public sealed class DeploymentLogInfo
{
    /// <summary>Gets or sets id.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets level.</summary>
    public DeploymentLogLevel Level { get; init; }

    /// <summary>Gets or sets stage.</summary>
    public string Stage { get; init; } = string.Empty;

    /// <summary>Gets or sets message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets or sets timestamp.</summary>
    public DateTime TimestampUtc { get; init; }
}

/// <summary>Health snapshot.</summary>
public sealed class DeploymentHealthInfo
{
    /// <summary>Gets or sets state.</summary>
    public DeploymentHealthState State { get; init; }

    /// <summary>Gets or sets GPU available.</summary>
    public bool GpuAvailable { get; init; }

    /// <summary>Gets or sets CUDA available.</summary>
    public bool CudaAvailable { get; init; }

    /// <summary>Gets or sets runtime running.</summary>
    public bool RuntimeRunning { get; init; }

    /// <summary>Gets or sets model available.</summary>
    public bool ModelAvailable { get; init; }

    /// <summary>Gets or sets gateway reachable.</summary>
    public bool GatewayReachable { get; init; }

    /// <summary>Gets or sets streaming works.</summary>
    public bool StreamingWorks { get; init; }

    /// <summary>Gets or sets last checked.</summary>
    public DateTime? LastCheckedAt { get; init; }
}

/// <summary>GPU catalog row.</summary>
public sealed class GpuCatalogInfo
{
    /// <summary>Gets or sets id.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets code.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Gets or sets name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets GPU type.</summary>
    public GpuType GpuType { get; init; }

    /// <summary>Gets or sets VRAM GB.</summary>
    public int VramGb { get; init; }

    /// <summary>Gets or sets CUDA capability.</summary>
    public string CudaCapability { get; init; } = string.Empty;

    /// <summary>Gets or sets estimated hourly cost.</summary>
    public decimal EstimatedHourlyCostUsd { get; init; }

    /// <summary>Gets or sets provider availability.</summary>
    public IReadOnlyList<string> ProviderAvailability { get; init; } = [];

    /// <summary>Gets or sets whether custom.</summary>
    public bool IsCustom { get; init; }
}

/// <summary>Model catalog row.</summary>
public sealed class ModelCatalogInfo
{
    /// <summary>Gets or sets id.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets code.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Gets or sets reference.</summary>
    public string ModelReference { get; init; } = string.Empty;

    /// <summary>Gets or sets name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets provider.</summary>
    public string Provider { get; init; } = string.Empty;

    /// <summary>Gets or sets version.</summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>Gets or sets family.</summary>
    public string Family { get; init; } = string.Empty;

    /// <summary>Gets or sets parameters.</summary>
    public string Parameters { get; init; } = string.Empty;

    /// <summary>Gets or sets quantization.</summary>
    public string? Quantization { get; init; }

    /// <summary>Gets or sets context length.</summary>
    public int ContextLength { get; init; }

    /// <summary>Gets or sets required VRAM.</summary>
    public int RequiredVramGb { get; init; }

    /// <summary>Gets or sets recommended GPU.</summary>
    public string RecommendedGpuCode { get; init; } = string.Empty;

    /// <summary>Gets or sets minimum GPU.</summary>
    public string MinimumGpuCode { get; init; } = string.Empty;

    /// <summary>Gets or sets vision.</summary>
    public bool SupportsVision { get; init; }

    /// <summary>Gets or sets tools.</summary>
    public bool SupportsTools { get; init; }

    /// <summary>Gets or sets embeddings.</summary>
    public bool SupportsEmbeddings { get; init; }

    /// <summary>Gets or sets license.</summary>
    public string? License { get; init; }

    /// <summary>Gets or sets download size.</summary>
    public decimal DownloadSizeGb { get; init; }

    /// <summary>Gets or sets preferred runtime.</summary>
    public InferenceRuntimeKind PreferredRuntime { get; init; }
}

/// <summary>Template info.</summary>
public sealed class DeploymentTemplateInfo
{
    /// <summary>Gets or sets id.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets code.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Gets or sets name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets kind.</summary>
    public DeploymentTemplateKind Kind { get; init; }

    /// <summary>Gets or sets description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets runtime.</summary>
    public InferenceRuntimeKind Runtime { get; init; }

    /// <summary>Gets or sets image.</summary>
    public string ContainerImage { get; init; } = string.Empty;

    /// <summary>Gets or sets recommended GPU.</summary>
    public string RecommendedGpuCode { get; init; } = string.Empty;

    /// <summary>Gets or sets default models.</summary>
    public IReadOnlyList<string> DefaultModelCodes { get; init; } = [];
}

/// <summary>Region info for deployment wizard.</summary>
public sealed class DeploymentRegionInfo
{
    /// <summary>Gets or sets region code.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Gets or sets display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets continent / area.</summary>
    public string Area { get; init; } = string.Empty;

    /// <summary>Gets or sets estimated latency ms.</summary>
    public int? EstimatedLatencyMs { get; init; }

    /// <summary>Gets or sets relative price score (1 = cheapest).</summary>
    public decimal? PriceScore { get; init; }

    /// <summary>Gets or sets availability score 0-100.</summary>
    public int AvailabilityScore { get; init; } = 100;
}

/// <summary>GPU recommendation result.</summary>
public sealed class GpuRecommendationResult
{
    /// <summary>Gets or sets recommended GPU code.</summary>
    public string RecommendedGpuCode { get; init; } = string.Empty;

    /// <summary>Gets or sets minimum GPU code.</summary>
    public string MinimumGpuCode { get; init; } = string.Empty;

    /// <summary>Gets or sets required VRAM.</summary>
    public int RequiredVramGb { get; init; }

    /// <summary>Gets or sets estimated tokens/sec hint.</summary>
    public string EstimatedPerformance { get; init; } = string.Empty;

    /// <summary>Gets or sets warnings.</summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

/// <summary>Dashboard aggregates.</summary>
public sealed class DeploymentDashboardInfo
{
    /// <summary>Gets or sets running count.</summary>
    public int RunningDeployments { get; init; }

    /// <summary>Gets or sets downloading count.</summary>
    public int DownloadingModels { get; init; }

    /// <summary>Gets or sets healthy count.</summary>
    public int HealthyDeployments { get; init; }

    /// <summary>Gets or sets failed count.</summary>
    public int FailedDeployments { get; init; }

    /// <summary>Gets or sets estimated monthly cost.</summary>
    public decimal EstimatedMonthlyCostUsd { get; init; }

    /// <summary>Gets or sets recent deployments.</summary>
    public IReadOnlyList<DeploymentSummary> Recent { get; init; } = [];
}

/// <summary>Runtime validation context.</summary>
public sealed class RuntimeValidationContext
{
    /// <summary>Gets or sets runtime.</summary>
    public InferenceRuntimeKind Runtime { get; init; }

    /// <summary>Gets or sets CUDA version.</summary>
    public string CudaVersion { get; init; } = "12.4";

    /// <summary>Gets or sets GPU VRAM.</summary>
    public int GpuVramGb { get; init; }

    /// <summary>Gets or sets CUDA capability.</summary>
    public string CudaCapability { get; init; } = string.Empty;

    /// <summary>Gets or sets required VRAM for models.</summary>
    public int RequiredVramGb { get; init; }
}

/// <summary>Runtime execution context on a live pod.</summary>
public sealed class RuntimeExecutionContext
{
    /// <summary>Gets or sets organization id.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets deployment id.</summary>
    public Guid DeploymentId { get; init; }

    /// <summary>Gets or sets pod id.</summary>
    public Guid GpuPodId { get; init; }

    /// <summary>Gets or sets base URL of the runtime HTTP API.</summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>Gets or sets CUDA version.</summary>
    public string CudaVersion { get; init; } = "12.4";
}

/// <summary>Runtime health probe result.</summary>
public sealed class RuntimeHealthResult
{
    /// <summary>Gets or sets whether runtime is running.</summary>
    public bool RuntimeRunning { get; init; }

    /// <summary>Gets or sets whether GPU is visible.</summary>
    public bool GpuAvailable { get; init; }

    /// <summary>Gets or sets whether CUDA is available.</summary>
    public bool CudaAvailable { get; init; }

    /// <summary>Gets or sets whether streaming works.</summary>
    public bool StreamingWorks { get; init; }

    /// <summary>Gets or sets detail message.</summary>
    public string? Message { get; init; }
}
