using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// One-click AI inference server deployment.
/// </summary>
public class AiDeployment : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization id.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets deployment status.</summary>
    public DeploymentStatus Status { get; set; } = DeploymentStatus.Pending;

    /// <summary>Gets or sets the compute provider id.</summary>
    public Guid ProviderId { get; set; }

    /// <summary>Gets or sets cloud provider kind.</summary>
    public DeploymentCloudProviderKind CloudProvider { get; set; } = DeploymentCloudProviderKind.RunPod;

    /// <summary>Gets or sets region code.</summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>Gets or sets GPU catalog code (e.g. RTX5090).</summary>
    public string GpuCode { get; set; } = string.Empty;

    /// <summary>Gets or sets provider-specific GPU id.</summary>
    public string ProviderGpuId { get; set; } = string.Empty;

    /// <summary>Gets or sets inference runtime.</summary>
    public InferenceRuntimeKind Runtime { get; set; } = InferenceRuntimeKind.Ollama;

    /// <summary>Gets or sets CUDA major.minor version target (e.g. 12.4).</summary>
    public string CudaVersion { get; set; } = "12.4";

    /// <summary>Gets or sets optional template id.</summary>
    public Guid? TemplateId { get; set; }

    /// <summary>Gets or sets linked GPU pod id once provisioned.</summary>
    public Guid? GpuPodId { get; set; }

    /// <summary>Gets or sets gateway route id once registered.</summary>
    public Guid? GatewayRouteId { get; set; }

    /// <summary>Gets or sets container image used.</summary>
    public string? ImageName { get; set; }

    /// <summary>Gets or sets progress percent 0-100.</summary>
    public int ProgressPercent { get; set; }

    /// <summary>Gets or sets current stage message.</summary>
    public string? StatusMessage { get; set; }

    /// <summary>Gets or sets last error message.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets or sets retry count.</summary>
    public int RetryCount { get; set; }

    /// <summary>Gets or sets whether cancellation was requested.</summary>
    public bool CancellationRequested { get; set; }

    /// <summary>Gets or sets when the deployment became ready.</summary>
    public DateTime? ReadyAt { get; set; }

    /// <summary>Gets or sets estimated hourly cost USD.</summary>
    public decimal EstimatedHourlyCostUsd { get; set; }

    /// <summary>Gets or sets environment variables JSON.</summary>
    public string? EnvironmentVariablesJson { get; set; }

    /// <summary>Gets the organization.</summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>Gets the compute provider.</summary>
    public ComputeProvider Provider { get; set; } = null!;

    /// <summary>Gets the optional template.</summary>
    public DeploymentTemplate? Template { get; set; }

    /// <summary>Gets the linked pod.</summary>
    public GpuPod? GpuPod { get; set; }

    /// <summary>Gets selected models.</summary>
    public ICollection<DeploymentModel> Models { get; set; } = [];

    /// <summary>Gets deployment logs.</summary>
    public ICollection<DeploymentLog> Logs { get; set; } = [];

    /// <summary>Gets health snapshot.</summary>
    public DeploymentHealth? Health { get; set; }

    /// <summary>Gets history rows.</summary>
    public ICollection<DeploymentHistory> History { get; set; } = [];
}

/// <summary>
/// Reusable deployment template definition.
/// </summary>
public class DeploymentTemplate : Common.AuditableEntity
{
    /// <summary>Gets or sets template code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets template kind.</summary>
    public DeploymentTemplateKind Kind { get; set; }

    /// <summary>Gets or sets description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets runtime.</summary>
    public InferenceRuntimeKind Runtime { get; set; } = InferenceRuntimeKind.Ollama;

    /// <summary>Gets or sets container image.</summary>
    public string ContainerImage { get; set; } = string.Empty;

    /// <summary>Gets or sets startup command.</summary>
    public string? StartupCommand { get; set; }

    /// <summary>Gets or sets environment variables JSON.</summary>
    public string? EnvironmentVariablesJson { get; set; }

    /// <summary>Gets or sets health check path.</summary>
    public string HealthCheckPath { get; set; } = "/";

    /// <summary>Gets or sets health check port.</summary>
    public int HealthCheckPort { get; set; } = 11434;

    /// <summary>Gets or sets recommended GPU code.</summary>
    public string RecommendedGpuCode { get; set; } = "RTX4090";

    /// <summary>Gets or sets default model codes CSV/JSON.</summary>
    public string DefaultModelCodesJson { get; set; } = "[]";

    /// <summary>Gets or sets whether publicly listed.</summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>Gets or sets sort order.</summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// Model selected for a deployment.
/// </summary>
public class DeploymentModel : Common.BaseEntity
{
    /// <summary>Gets or sets deployment id.</summary>
    public Guid DeploymentId { get; set; }

    /// <summary>Gets or sets model catalog id.</summary>
    public Guid? ModelCatalogId { get; set; }

    /// <summary>Gets or sets model reference (e.g. qwen2.5-coder:7b).</summary>
    public string ModelReference { get; set; } = string.Empty;

    /// <summary>Gets or sets download status.</summary>
    public DeploymentStatus DownloadStatus { get; set; } = DeploymentStatus.Pending;

    /// <summary>Gets or sets download progress percent.</summary>
    public int ProgressPercent { get; set; }

    /// <summary>Gets or sets whether this is the primary model.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>Gets or sets error message.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets the deployment.</summary>
    public AiDeployment Deployment { get; set; } = null!;

    /// <summary>Gets the catalog entry.</summary>
    public ModelCatalogEntry? ModelCatalog { get; set; }
}

/// <summary>
/// Append-only deployment log entry.
/// </summary>
public class DeploymentLog : Common.BaseEntity
{
    /// <summary>Gets or sets deployment id.</summary>
    public Guid DeploymentId { get; set; }

    /// <summary>Gets or sets log level.</summary>
    public DeploymentLogLevel Level { get; set; } = DeploymentLogLevel.Info;

    /// <summary>Gets or sets stage / event name.</summary>
    public string Stage { get; set; } = string.Empty;

    /// <summary>Gets or sets message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets timestamp UTC.</summary>
    public DateTime TimestampUtc { get; set; }

    /// <summary>Gets the deployment.</summary>
    public AiDeployment Deployment { get; set; } = null!;
}

/// <summary>
/// Known runtime version and CUDA requirements.
/// </summary>
public class RuntimeVersion : Common.AuditableEntity
{
    /// <summary>Gets or sets runtime kind.</summary>
    public InferenceRuntimeKind Runtime { get; set; }

    /// <summary>Gets or sets version string.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets CUDA version (e.g. 12.4).</summary>
    public string CudaVersion { get; set; } = "12.4";

    /// <summary>Gets or sets default container image.</summary>
    public string ContainerImage { get; set; } = string.Empty;

    /// <summary>Gets or sets default port.</summary>
    public int Port { get; set; }

    /// <summary>Gets or sets health path.</summary>
    public string HealthPath { get; set; } = "/";

    /// <summary>Gets or sets whether this is the recommended version.</summary>
    public bool IsRecommended { get; set; }

    /// <summary>Gets or sets whether active.</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Centralized GPU metadata catalog.
/// </summary>
public class GpuCatalogEntry : Common.AuditableEntity
{
    /// <summary>Gets or sets GPU code (RTX4090, H100, …).</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets mapped GpuType when known.</summary>
    public GpuType GpuType { get; set; } = GpuType.Custom;

    /// <summary>Gets or sets VRAM in GB.</summary>
    public int VramGb { get; set; }

    /// <summary>Gets or sets CUDA compute capability (e.g. 8.9).</summary>
    public string CudaCapability { get; set; } = string.Empty;

    /// <summary>Gets or sets estimated hourly cost USD.</summary>
    public decimal EstimatedHourlyCostUsd { get; set; }

    /// <summary>Gets or sets provider availability JSON.</summary>
    public string ProviderAvailabilityJson { get; set; } = "[]";

    /// <summary>Gets or sets whether custom (user-defined).</summary>
    public bool IsCustom { get; set; }

    /// <summary>Gets or sets whether publicly listed.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets sort order.</summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// Centralized AI model catalog entry (discoverable without code changes).
/// </summary>
public class ModelCatalogEntry : Common.AuditableEntity
{
    /// <summary>Gets or sets catalog code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets pull/provider reference (e.g. qwen2.5-coder:32b).</summary>
    public string ModelReference { get; set; } = string.Empty;

    /// <summary>Gets or sets display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets publisher / provider label.</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>Gets or sets version.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets model family.</summary>
    public string Family { get; set; } = string.Empty;

    /// <summary>Gets or sets parameter count label.</summary>
    public string Parameters { get; set; } = string.Empty;

    /// <summary>Gets or sets quantization.</summary>
    public string? Quantization { get; set; }

    /// <summary>Gets or sets context length.</summary>
    public int ContextLength { get; set; }

    /// <summary>Gets or sets required VRAM GB.</summary>
    public int RequiredVramGb { get; set; }

    /// <summary>Gets or sets recommended GPU code.</summary>
    public string RecommendedGpuCode { get; set; } = "RTX4090";

    /// <summary>Gets or sets minimum GPU code.</summary>
    public string MinimumGpuCode { get; set; } = "RTX4090";

    /// <summary>Gets or sets whether vision is supported.</summary>
    public bool SupportsVision { get; set; }

    /// <summary>Gets or sets whether tools/function calling is supported.</summary>
    public bool SupportsTools { get; set; }

    /// <summary>Gets or sets whether embeddings are supported.</summary>
    public bool SupportsEmbeddings { get; set; }

    /// <summary>Gets or sets license text.</summary>
    public string? License { get; set; }

    /// <summary>Gets or sets download size GB estimate.</summary>
    public decimal DownloadSizeGb { get; set; }

    /// <summary>Gets or sets preferred runtime.</summary>
    public InferenceRuntimeKind PreferredRuntime { get; set; } = InferenceRuntimeKind.Ollama;

    /// <summary>Gets or sets whether active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets sort order.</summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// Latest health snapshot for a deployment.
/// </summary>
public class DeploymentHealth : Common.AuditableEntity
{
    /// <summary>Gets or sets deployment id.</summary>
    public Guid DeploymentId { get; set; }

    /// <summary>Gets or sets aggregate state.</summary>
    public DeploymentHealthState State { get; set; } = DeploymentHealthState.Unknown;

    /// <summary>Gets or sets whether GPU is available.</summary>
    public bool GpuAvailable { get; set; }

    /// <summary>Gets or sets whether CUDA is available.</summary>
    public bool CudaAvailable { get; set; }

    /// <summary>Gets or sets whether runtime is running.</summary>
    public bool RuntimeRunning { get; set; }

    /// <summary>Gets or sets whether models are available.</summary>
    public bool ModelAvailable { get; set; }

    /// <summary>Gets or sets whether gateway is reachable.</summary>
    public bool GatewayReachable { get; set; }

    /// <summary>Gets or sets whether streaming works.</summary>
    public bool StreamingWorks { get; set; }

    /// <summary>Gets or sets last checked UTC.</summary>
    public DateTime? LastCheckedAt { get; set; }

    /// <summary>Gets or sets details JSON.</summary>
    public string? DetailsJson { get; set; }

    /// <summary>Gets the deployment.</summary>
    public AiDeployment Deployment { get; set; } = null!;
}

/// <summary>
/// Status transition history for deployments.
/// </summary>
public class DeploymentHistory : Common.BaseEntity
{
    /// <summary>Gets or sets deployment id.</summary>
    public Guid DeploymentId { get; set; }

    /// <summary>Gets or sets previous status.</summary>
    public DeploymentStatus? FromStatus { get; set; }

    /// <summary>Gets or sets new status.</summary>
    public DeploymentStatus ToStatus { get; set; }

    /// <summary>Gets or sets message.</summary>
    public string? Message { get; set; }

    /// <summary>Gets or sets timestamp UTC.</summary>
    public DateTime TimestampUtc { get; set; }

    /// <summary>Gets the deployment.</summary>
    public AiDeployment Deployment { get; set; } = null!;
}
