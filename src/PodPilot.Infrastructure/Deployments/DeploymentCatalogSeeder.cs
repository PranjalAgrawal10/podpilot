using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Deployments;

/// <summary>
/// Seeds GPU, model, template, and runtime catalog data when empty.
/// </summary>
public sealed class DeploymentCatalogSeeder
{
    private static readonly string RunPodAvailability = JsonSerializer.Serialize(new[] { "RunPod" });

    private readonly IApplicationDbContext db;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentCatalogSeeder"/> class.
    /// </summary>
    public DeploymentCatalogSeeder(IApplicationDbContext db, IDateTimeService dateTimeService)
    {
        this.db = db;
        this.dateTimeService = dateTimeService;
    }

    /// <summary>
    /// Ensures catalog seed data exists.
    /// </summary>
    public async Task EnsureAsync(CancellationToken cancellationToken = default)
    {
        var now = dateTimeService.UtcNow;
        var added = false;

        if (!await db.GpuCatalogEntries.AnyAsync(cancellationToken))
        {
            foreach (var gpu in CreateGpus(now))
            {
                await db.AddGpuCatalogEntryAsync(gpu, cancellationToken);
            }

            added = true;
        }

        if (!await db.ModelCatalogEntries.AnyAsync(cancellationToken))
        {
            foreach (var model in CreateModels(now))
            {
                await db.AddModelCatalogEntryAsync(model, cancellationToken);
            }

            added = true;
        }

        if (!await db.DeploymentTemplates.AnyAsync(cancellationToken))
        {
            foreach (var template in CreateTemplates(now))
            {
                await db.AddDeploymentTemplateAsync(template, cancellationToken);
            }

            added = true;
        }

        if (!await db.RuntimeVersions.AnyAsync(cancellationToken))
        {
            foreach (var runtime in CreateRuntimeVersions(now))
            {
                await db.AddRuntimeVersionAsync(runtime, cancellationToken);
            }

            added = true;
        }

        if (added)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static IEnumerable<GpuCatalogEntry> CreateGpus(DateTime now) =>
    [
        Gpu("RTX4090", "NVIDIA RTX 4090", GpuType.RTX4090, 24, "8.9", 0.44m, 10, now),
        Gpu("RTX5090", "NVIDIA RTX 5090", GpuType.RTX5090, 32, "12.0", 0.69m, 20, now),
        Gpu("L40S", "NVIDIA L40S", GpuType.L40S, 48, "8.9", 0.99m, 30, now),
        Gpu("A100", "NVIDIA A100 80GB", GpuType.A100, 80, "8.0", 1.39m, 40, now),
        Gpu("H100", "NVIDIA H100 80GB", GpuType.H100, 80, "9.0", 2.49m, 50, now),
        Gpu("H200", "NVIDIA H200 141GB", GpuType.H200, 141, "9.0", 3.49m, 60, now),
        Gpu("B200", "NVIDIA B200 192GB", GpuType.B200, 192, "10.0", 4.99m, 70, now),
    ];

    private static GpuCatalogEntry Gpu(
        string code,
        string name,
        GpuType type,
        int vram,
        string capability,
        decimal cost,
        int sort,
        DateTime now) =>
        new()
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            GpuType = type,
            VramGb = vram,
            CudaCapability = capability,
            EstimatedHourlyCostUsd = cost,
            ProviderAvailabilityJson = RunPodAvailability,
            IsCustom = false,
            IsActive = true,
            SortOrder = sort,
            CreatedAt = now,
        };

    private static IEnumerable<ModelCatalogEntry> CreateModels(DateTime now) =>
    [
        Model("qwen-coder-32b", "qwen2.5-coder:32b", "Qwen2.5 Coder 32B", "Alibaba", "2.5", "Qwen", "32B", null, 32768, 24, "RTX5090", "RTX4090", false, true, false, 20m, 10, now),
        Model("qwen-coder-7b", "qwen2.5-coder:7b", "Qwen2.5 Coder 7B", "Alibaba", "2.5", "Qwen", "7B", null, 32768, 8, "RTX4090", "RTX4090", false, true, false, 5m, 20, now),
        Model("deepseek-r1", "deepseek-r1:70b", "DeepSeek R1", "DeepSeek", "1", "DeepSeek", "70B", null, 65536, 48, "L40S", "A100", false, true, false, 40m, 30, now),
        Model("deepseek-coder", "deepseek-coder:33b", "DeepSeek Coder", "DeepSeek", "2", "DeepSeek", "33B", null, 16384, 24, "RTX5090", "RTX4090", false, true, false, 20m, 40, now),
        Model("llama32", "llama3.2", "Llama 3.2", "Meta", "3.2", "Llama", "3B", null, 128000, 6, "RTX4090", "RTX4090", false, true, false, 3m, 50, now),
        Model("llama31", "llama3.1", "Llama 3.1", "Meta", "3.1", "Llama", "8B", null, 128000, 10, "RTX4090", "RTX4090", false, true, false, 6m, 60, now),
        Model("glm", "glm4:9b", "GLM-4", "Zhipu", "4", "GLM", "9B", null, 128000, 12, "RTX4090", "RTX4090", false, true, false, 8m, 70, now),
        Model("kimi", "kimi-k2:latest", "Kimi", "Moonshot", "k2", "Kimi", "unknown", null, 128000, 24, "RTX5090", "RTX4090", false, true, false, 18m, 80, now),
        Model("gemma", "gemma2:27b", "Gemma 2", "Google", "2", "Gemma", "27B", null, 8192, 20, "RTX5090", "RTX4090", false, true, false, 16m, 90, now),
        Model("mistral", "mistral:latest", "Mistral", "Mistral AI", "latest", "Mistral", "7B", null, 32768, 8, "RTX4090", "RTX4090", false, true, false, 5m, 100, now),
        Model("llava", "llava:latest", "LLaVA Vision", "LLaVA", "latest", "LLaVA", "7B", null, 4096, 12, "RTX4090", "RTX4090", true, false, false, 6m, 110, now),
    ];

    private static ModelCatalogEntry Model(
        string code,
        string reference,
        string name,
        string provider,
        string version,
        string family,
        string parameters,
        string? quantization,
        int context,
        int vram,
        string recommended,
        string minimum,
        bool vision,
        bool tools,
        bool embeddings,
        decimal sizeGb,
        int sort,
        DateTime now) =>
        new()
        {
            Id = Guid.NewGuid(),
            Code = code,
            ModelReference = reference,
            Name = name,
            Provider = provider,
            Version = version,
            Family = family,
            Parameters = parameters,
            Quantization = quantization,
            ContextLength = context,
            RequiredVramGb = vram,
            RecommendedGpuCode = recommended,
            MinimumGpuCode = minimum,
            SupportsVision = vision,
            SupportsTools = tools,
            SupportsEmbeddings = embeddings,
            DownloadSizeGb = sizeGb,
            PreferredRuntime = InferenceRuntimeKind.Ollama,
            IsActive = true,
            SortOrder = sort,
            CreatedAt = now,
        };

    private static IEnumerable<DeploymentTemplate> CreateTemplates(DateTime now) =>
    [
        Template("qwen-coding", "Qwen Coding", DeploymentTemplateKind.QwenCoding, "Qwen coding assistants via Ollama.", ["qwen-coder-7b", "qwen-coder-32b"], "RTX4090", 10, now),
        Template("deepseek-coding", "DeepSeek Coding", DeploymentTemplateKind.DeepSeekCoding, "DeepSeek coding / reasoning stack.", ["deepseek-coder", "deepseek-r1"], "L40S", 20, now),
        Template("general-chat", "General Chat", DeploymentTemplateKind.GeneralChat, "General-purpose chat models.", ["llama32", "mistral"], "RTX4090", 30, now),
        Template("vision", "Vision", DeploymentTemplateKind.Vision, "Multimodal vision models.", ["llava"], "RTX4090", 40, now),
        Template("reasoning", "Reasoning", DeploymentTemplateKind.Reasoning, "Chain-of-thought / reasoning models.", ["deepseek-r1"], "A100", 50, now),
        Template("custom", "Custom", DeploymentTemplateKind.Custom, "Bring your own model references.", [], "RTX4090", 60, now),
    ];

    private static DeploymentTemplate Template(
        string code,
        string name,
        DeploymentTemplateKind kind,
        string description,
        string[] models,
        string gpu,
        int sort,
        DateTime now) =>
        new()
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Kind = kind,
            Description = description,
            Runtime = InferenceRuntimeKind.Ollama,
            ContainerImage = "ollama/ollama:latest",
            HealthCheckPath = "/api/tags",
            HealthCheckPort = 11434,
            RecommendedGpuCode = gpu,
            DefaultModelCodesJson = JsonSerializer.Serialize(models),
            IsPublic = true,
            SortOrder = sort,
            CreatedAt = now,
        };

    private static IEnumerable<RuntimeVersion> CreateRuntimeVersions(DateTime now) =>
    [
        new RuntimeVersion
        {
            Id = Guid.NewGuid(),
            Runtime = InferenceRuntimeKind.Ollama,
            Version = "latest",
            CudaVersion = "12.4",
            ContainerImage = "ollama/ollama:latest",
            Port = 11434,
            HealthPath = "/api/tags",
            IsRecommended = true,
            IsActive = true,
            CreatedAt = now,
        },
        new RuntimeVersion
        {
            Id = Guid.NewGuid(),
            Runtime = InferenceRuntimeKind.Vllm,
            Version = "latest",
            CudaVersion = "12.4",
            ContainerImage = "vllm/vllm-openai:latest",
            Port = 8000,
            HealthPath = "/health",
            IsRecommended = true,
            IsActive = true,
            CreatedAt = now,
        },
        new RuntimeVersion
        {
            Id = Guid.NewGuid(),
            Runtime = InferenceRuntimeKind.LlamaCpp,
            Version = "server-cuda",
            CudaVersion = "12.4",
            ContainerImage = "ghcr.io/ggerganov/llama.cpp:server-cuda",
            Port = 8080,
            HealthPath = "/health",
            IsRecommended = true,
            IsActive = true,
            CreatedAt = now,
        },
    ];
}
