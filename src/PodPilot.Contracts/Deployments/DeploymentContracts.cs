namespace PodPilot.Contracts.Deployments;

/// <summary>Create deployment request.</summary>
public sealed class CreateDeploymentRequest
{
    /// <summary>Gets or sets name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets compute provider id.</summary>
    public Guid ProviderId { get; set; }

    /// <summary>Gets or sets region.</summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>Gets or sets GPU catalog code.</summary>
    public string GpuCode { get; set; } = string.Empty;

    /// <summary>Gets or sets optional provider GPU id.</summary>
    public string? ProviderGpuId { get; set; }

    /// <summary>Gets or sets runtime (default Ollama).</summary>
    public string Runtime { get; set; } = "Ollama";

    /// <summary>Gets or sets model references or catalog codes.</summary>
    public List<string> Models { get; set; } = [];

    /// <summary>Gets or sets optional template code.</summary>
    public string? TemplateCode { get; set; }

    /// <summary>Gets or sets optional environment variables.</summary>
    public Dictionary<string, string>? EnvironmentVariables { get; set; }
}

/// <summary>Deployment summary response.</summary>
public sealed class DeploymentResponse
{
    /// <summary>Gets or sets id.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets runtime.</summary>
    public string Runtime { get; set; } = string.Empty;

    /// <summary>Gets or sets GPU code.</summary>
    public string GpuCode { get; set; } = string.Empty;

    /// <summary>Gets or sets region.</summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>Gets or sets progress.</summary>
    public int ProgressPercent { get; set; }

    /// <summary>Gets or sets status message.</summary>
    public string? StatusMessage { get; set; }

    /// <summary>Gets or sets health state.</summary>
    public string HealthState { get; set; } = string.Empty;

    /// <summary>Gets or sets estimated hourly cost.</summary>
    public decimal EstimatedHourlyCostUsd { get; set; }

    /// <summary>Gets or sets created at.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets pod id.</summary>
    public Guid? GpuPodId { get; set; }

    /// <summary>Gets or sets provider id.</summary>
    public Guid ProviderId { get; set; }

    /// <summary>Gets or sets cloud provider.</summary>
    public string CloudProvider { get; set; } = string.Empty;

    /// <summary>Gets or sets CUDA version.</summary>
    public string CudaVersion { get; set; } = string.Empty;

    /// <summary>Gets or sets image.</summary>
    public string? ImageName { get; set; }

    /// <summary>Gets or sets error.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets or sets gateway route id.</summary>
    public Guid? GatewayRouteId { get; set; }

    /// <summary>Gets or sets ready at.</summary>
    public DateTime? ReadyAt { get; set; }

    /// <summary>Gets or sets models.</summary>
    public List<DeploymentModelResponse> Models { get; set; } = [];

    /// <summary>Gets or sets logs.</summary>
    public List<DeploymentLogResponse> Logs { get; set; } = [];

    /// <summary>Gets or sets health.</summary>
    public DeploymentHealthResponse? Health { get; set; }
}

/// <summary>Deployment model response.</summary>
public sealed class DeploymentModelResponse
{
    /// <summary>Gets or sets id.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets reference.</summary>
    public string ModelReference { get; set; } = string.Empty;

    /// <summary>Gets or sets download status.</summary>
    public string DownloadStatus { get; set; } = string.Empty;

    /// <summary>Gets or sets progress.</summary>
    public int ProgressPercent { get; set; }

    /// <summary>Gets or sets whether primary.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>Gets or sets error.</summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>Deployment log response.</summary>
public sealed class DeploymentLogResponse
{
    /// <summary>Gets or sets id.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets level.</summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>Gets or sets stage.</summary>
    public string Stage { get; set; } = string.Empty;

    /// <summary>Gets or sets message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets timestamp.</summary>
    public DateTime TimestampUtc { get; set; }
}

/// <summary>Health response.</summary>
public sealed class DeploymentHealthResponse
{
    /// <summary>Gets or sets state.</summary>
    public string State { get; set; } = string.Empty;

    /// <summary>Gets or sets GPU available.</summary>
    public bool GpuAvailable { get; set; }

    /// <summary>Gets or sets CUDA available.</summary>
    public bool CudaAvailable { get; set; }

    /// <summary>Gets or sets runtime running.</summary>
    public bool RuntimeRunning { get; set; }

    /// <summary>Gets or sets model available.</summary>
    public bool ModelAvailable { get; set; }

    /// <summary>Gets or sets gateway reachable.</summary>
    public bool GatewayReachable { get; set; }

    /// <summary>Gets or sets streaming works.</summary>
    public bool StreamingWorks { get; set; }

    /// <summary>Gets or sets last checked.</summary>
    public DateTime? LastCheckedAt { get; set; }
}

/// <summary>GPU catalog response.</summary>
public sealed class GpuCatalogResponse
{
    /// <summary>Gets or sets id.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets GPU type.</summary>
    public string GpuType { get; set; } = string.Empty;

    /// <summary>Gets or sets VRAM GB.</summary>
    public int VramGb { get; set; }

    /// <summary>Gets or sets CUDA capability.</summary>
    public string CudaCapability { get; set; } = string.Empty;

    /// <summary>Gets or sets estimated hourly cost.</summary>
    public decimal EstimatedHourlyCostUsd { get; set; }

    /// <summary>Gets or sets provider availability.</summary>
    public List<string> ProviderAvailability { get; set; } = [];

    /// <summary>Gets or sets whether custom.</summary>
    public bool IsCustom { get; set; }
}

/// <summary>Model catalog response.</summary>
public sealed class ModelCatalogResponse
{
    /// <summary>Gets or sets id.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets reference.</summary>
    public string ModelReference { get; set; } = string.Empty;

    /// <summary>Gets or sets name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets provider.</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>Gets or sets version.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets family.</summary>
    public string Family { get; set; } = string.Empty;

    /// <summary>Gets or sets parameters.</summary>
    public string Parameters { get; set; } = string.Empty;

    /// <summary>Gets or sets quantization.</summary>
    public string? Quantization { get; set; }

    /// <summary>Gets or sets context length.</summary>
    public int ContextLength { get; set; }

    /// <summary>Gets or sets required VRAM.</summary>
    public int RequiredVramGb { get; set; }

    /// <summary>Gets or sets recommended GPU.</summary>
    public string RecommendedGpuCode { get; set; } = string.Empty;

    /// <summary>Gets or sets minimum GPU.</summary>
    public string MinimumGpuCode { get; set; } = string.Empty;

    /// <summary>Gets or sets vision.</summary>
    public bool SupportsVision { get; set; }

    /// <summary>Gets or sets tools.</summary>
    public bool SupportsTools { get; set; }

    /// <summary>Gets or sets embeddings.</summary>
    public bool SupportsEmbeddings { get; set; }

    /// <summary>Gets or sets license.</summary>
    public string? License { get; set; }

    /// <summary>Gets or sets download size.</summary>
    public decimal DownloadSizeGb { get; set; }

    /// <summary>Gets or sets preferred runtime.</summary>
    public string PreferredRuntime { get; set; } = string.Empty;
}

/// <summary>Template response.</summary>
public sealed class DeploymentTemplateResponse
{
    /// <summary>Gets or sets id.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets kind.</summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>Gets or sets description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets runtime.</summary>
    public string Runtime { get; set; } = string.Empty;

    /// <summary>Gets or sets image.</summary>
    public string ContainerImage { get; set; } = string.Empty;

    /// <summary>Gets or sets recommended GPU.</summary>
    public string RecommendedGpuCode { get; set; } = string.Empty;

    /// <summary>Gets or sets default models.</summary>
    public List<string> DefaultModelCodes { get; set; } = [];
}

/// <summary>Region response.</summary>
public sealed class DeploymentRegionResponse
{
    /// <summary>Gets or sets code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets area.</summary>
    public string Area { get; set; } = string.Empty;

    /// <summary>Gets or sets latency.</summary>
    public int? EstimatedLatencyMs { get; set; }

    /// <summary>Gets or sets price score.</summary>
    public decimal? PriceScore { get; set; }

    /// <summary>Gets or sets availability.</summary>
    public int AvailabilityScore { get; set; }
}

/// <summary>GPU recommendation request.</summary>
public sealed class RecommendGpuRequest
{
    /// <summary>Gets or sets model codes or references.</summary>
    public List<string> Models { get; set; } = [];
}

/// <summary>GPU recommendation response.</summary>
public sealed class GpuRecommendationResponse
{
    /// <summary>Gets or sets recommended GPU.</summary>
    public string RecommendedGpuCode { get; set; } = string.Empty;

    /// <summary>Gets or sets minimum GPU.</summary>
    public string MinimumGpuCode { get; set; } = string.Empty;

    /// <summary>Gets or sets required VRAM.</summary>
    public int RequiredVramGb { get; set; }

    /// <summary>Gets or sets performance estimate.</summary>
    public string EstimatedPerformance { get; set; } = string.Empty;

    /// <summary>Gets or sets warnings.</summary>
    public List<string> Warnings { get; set; } = [];
}

/// <summary>Dashboard response.</summary>
public sealed class DeploymentDashboardResponse
{
    /// <summary>Gets or sets running.</summary>
    public int RunningDeployments { get; set; }

    /// <summary>Gets or sets downloading.</summary>
    public int DownloadingModels { get; set; }

    /// <summary>Gets or sets healthy.</summary>
    public int HealthyDeployments { get; set; }

    /// <summary>Gets or sets failed.</summary>
    public int FailedDeployments { get; set; }

    /// <summary>Gets or sets estimated monthly cost.</summary>
    public decimal EstimatedMonthlyCostUsd { get; set; }

    /// <summary>Gets or sets recent.</summary>
    public List<DeploymentResponse> Recent { get; set; } = [];
}
