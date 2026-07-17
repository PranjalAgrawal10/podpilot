namespace PodPilot.Domain.Enums;

/// <summary>
/// Lifecycle status for a one-click AI pod deployment.
/// </summary>
public enum DeploymentStatus
{
    /// <summary>Queued, not yet started.</summary>
    Pending = 0,

    /// <summary>Provisioning GPU pod on the cloud provider.</summary>
    Provisioning = 1,

    /// <summary>Waiting for the pod to start.</summary>
    Starting = 2,

    /// <summary>Installing or verifying the inference runtime.</summary>
    InstallingRuntime = 3,

    /// <summary>Downloading selected models.</summary>
    DownloadingModels = 4,

    /// <summary>Configuring gateway and endpoints.</summary>
    Configuring = 5,

    /// <summary>Running deployment health checks.</summary>
    HealthCheck = 6,

    /// <summary>Deployment is ready for inference.</summary>
    Ready = 7,

    /// <summary>Deployment failed.</summary>
    Failed = 8,

    /// <summary>Deployment is being deleted.</summary>
    Deleting = 9,

    /// <summary>Deployment was cancelled.</summary>
    Cancelled = 10,
}

/// <summary>
/// Supported AI inference runtimes for one-click deployments.
/// </summary>
public enum InferenceRuntimeKind
{
    /// <summary>Ollama runtime (default).</summary>
    Ollama = 0,

    /// <summary>vLLM high-throughput serving.</summary>
    Vllm = 1,

    /// <summary>llama.cpp HTTP server.</summary>
    LlamaCpp = 2,
}

/// <summary>
/// Cloud provider kinds for one-click deployments (maps to compute providers).
/// </summary>
public enum DeploymentCloudProviderKind
{
    /// <summary>RunPod.</summary>
    RunPod = 0,

    /// <summary>Vast.ai (future).</summary>
    VastAi = 1,

    /// <summary>Lambda Labs (future).</summary>
    LambdaLabs = 2,

    /// <summary>Azure GPU (future).</summary>
    AzureGpu = 3,

    /// <summary>AWS GPU (future).</summary>
    AwsGpu = 4,

    /// <summary>Google Cloud GPU (future).</summary>
    GoogleCloudGpu = 5,

    /// <summary>Kubernetes GPU (future).</summary>
    Kubernetes = 6,
}

/// <summary>
/// Prefabricated deployment template categories.
/// </summary>
public enum DeploymentTemplateKind
{
    /// <summary>Qwen coding assistants.</summary>
    QwenCoding = 0,

    /// <summary>DeepSeek coding assistants.</summary>
    DeepSeekCoding = 1,

    /// <summary>General chat.</summary>
    GeneralChat = 2,

    /// <summary>Vision-capable models.</summary>
    Vision = 3,

    /// <summary>Reasoning / chain-of-thought models.</summary>
    Reasoning = 4,

    /// <summary>Custom user-defined template.</summary>
    Custom = 5,
}

/// <summary>
/// Severity for deployment log entries.
/// </summary>
public enum DeploymentLogLevel
{
    /// <summary>Informational.</summary>
    Info = 0,

    /// <summary>Warning.</summary>
    Warning = 1,

    /// <summary>Error.</summary>
    Error = 2,
}

/// <summary>
/// Health check component for a deployment.
/// </summary>
public enum DeploymentHealthCheckKind
{
    /// <summary>GPU is available on the pod.</summary>
    GpuAvailable = 0,

    /// <summary>CUDA runtime is available.</summary>
    CudaAvailable = 1,

    /// <summary>Inference runtime process is running.</summary>
    RuntimeRunning = 2,

    /// <summary>Selected model(s) are available.</summary>
    ModelAvailable = 3,

    /// <summary>AI Gateway can reach the endpoint.</summary>
    GatewayReachable = 4,

    /// <summary>Streaming inference works.</summary>
    StreamingWorks = 5,
}

/// <summary>
/// Aggregate health state for a deployment.
/// </summary>
public enum DeploymentHealthState
{
    /// <summary>Unknown / not yet checked.</summary>
    Unknown = 0,

    /// <summary>Healthy.</summary>
    Healthy = 1,

    /// <summary>Degraded.</summary>
    Degraded = 2,

    /// <summary>Unhealthy.</summary>
    Unhealthy = 3,
}
