using System.Text.Json;
using PodPilot.Application.Models.Deployments;
using PodPilot.Contracts.Deployments;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Deployments;

/// <summary>
/// Maps deployment domain models to contracts.
/// </summary>
internal static class DeploymentMapper
{
    /// <summary>Maps summary to response.</summary>
    public static DeploymentResponse ToResponse(DeploymentSummary summary) =>
        new()
        {
            Id = summary.Id,
            Name = summary.Name,
            Status = summary.Status.ToString(),
            Runtime = summary.Runtime.ToString(),
            GpuCode = summary.GpuCode,
            Region = summary.Region,
            ProgressPercent = summary.ProgressPercent,
            StatusMessage = summary.StatusMessage,
            HealthState = summary.HealthState.ToString(),
            EstimatedHourlyCostUsd = summary.EstimatedHourlyCostUsd,
            CreatedAt = summary.CreatedAt,
            GpuPodId = summary.GpuPodId,
        };

    /// <summary>Maps detail to response.</summary>
    public static DeploymentResponse ToResponse(DeploymentDetail detail)
    {
        var response = ToResponse((DeploymentSummary)detail);
        response.ProviderId = detail.ProviderId;
        response.CloudProvider = detail.CloudProvider.ToString();
        response.CudaVersion = detail.CudaVersion;
        response.ImageName = detail.ImageName;
        response.ErrorMessage = detail.ErrorMessage;
        response.GatewayRouteId = detail.GatewayRouteId;
        response.ReadyAt = detail.ReadyAt;
        response.Models = detail.Models.Select(m => new DeploymentModelResponse
        {
            Id = m.Id,
            ModelReference = m.ModelReference,
            DownloadStatus = m.DownloadStatus.ToString(),
            ProgressPercent = m.ProgressPercent,
            IsPrimary = m.IsPrimary,
            ErrorMessage = m.ErrorMessage,
        }).ToList();
        response.Logs = detail.Logs.Select(l => new DeploymentLogResponse
        {
            Id = l.Id,
            Level = l.Level.ToString(),
            Stage = l.Stage,
            Message = l.Message,
            TimestampUtc = l.TimestampUtc,
        }).ToList();
        response.Health = detail.Health is null ? null : ToHealthResponse(detail.Health);
        return response;
    }

    /// <summary>Maps health.</summary>
    public static DeploymentHealthResponse ToHealthResponse(DeploymentHealthInfo health) =>
        new()
        {
            State = health.State.ToString(),
            GpuAvailable = health.GpuAvailable,
            CudaAvailable = health.CudaAvailable,
            RuntimeRunning = health.RuntimeRunning,
            ModelAvailable = health.ModelAvailable,
            GatewayReachable = health.GatewayReachable,
            StreamingWorks = health.StreamingWorks,
            LastCheckedAt = health.LastCheckedAt,
        };

    /// <summary>Maps GPU catalog.</summary>
    public static GpuCatalogResponse ToGpuResponse(GpuCatalogInfo info) =>
        new()
        {
            Id = info.Id,
            Code = info.Code,
            Name = info.Name,
            GpuType = info.GpuType.ToString(),
            VramGb = info.VramGb,
            CudaCapability = info.CudaCapability,
            EstimatedHourlyCostUsd = info.EstimatedHourlyCostUsd,
            ProviderAvailability = info.ProviderAvailability.ToList(),
            IsCustom = info.IsCustom,
        };

    /// <summary>Maps model catalog.</summary>
    public static ModelCatalogResponse ToModelResponse(ModelCatalogInfo info) =>
        new()
        {
            Id = info.Id,
            Code = info.Code,
            ModelReference = info.ModelReference,
            Name = info.Name,
            Provider = info.Provider,
            Version = info.Version,
            Family = info.Family,
            Parameters = info.Parameters,
            Quantization = info.Quantization,
            ContextLength = info.ContextLength,
            RequiredVramGb = info.RequiredVramGb,
            RecommendedGpuCode = info.RecommendedGpuCode,
            MinimumGpuCode = info.MinimumGpuCode,
            SupportsVision = info.SupportsVision,
            SupportsTools = info.SupportsTools,
            SupportsEmbeddings = info.SupportsEmbeddings,
            License = info.License,
            DownloadSizeGb = info.DownloadSizeGb,
            PreferredRuntime = info.PreferredRuntime.ToString(),
        };

    /// <summary>Maps template.</summary>
    public static DeploymentTemplateResponse ToTemplateResponse(DeploymentTemplateInfo info) =>
        new()
        {
            Id = info.Id,
            Code = info.Code,
            Name = info.Name,
            Kind = info.Kind.ToString(),
            Description = info.Description,
            Runtime = info.Runtime.ToString(),
            ContainerImage = info.ContainerImage,
            RecommendedGpuCode = info.RecommendedGpuCode,
            DefaultModelCodes = info.DefaultModelCodes.ToList(),
        };

    /// <summary>Maps region.</summary>
    public static DeploymentRegionResponse ToRegionResponse(DeploymentRegionInfo info) =>
        new()
        {
            Code = info.Code,
            Name = info.Name,
            Area = info.Area,
            EstimatedLatencyMs = info.EstimatedLatencyMs,
            PriceScore = info.PriceScore,
            AvailabilityScore = info.AvailabilityScore,
        };

    /// <summary>Maps recommendation.</summary>
    public static GpuRecommendationResponse ToRecommendationResponse(GpuRecommendationResult result) =>
        new()
        {
            RecommendedGpuCode = result.RecommendedGpuCode,
            MinimumGpuCode = result.MinimumGpuCode,
            RequiredVramGb = result.RequiredVramGb,
            EstimatedPerformance = result.EstimatedPerformance,
            Warnings = result.Warnings.ToList(),
        };

    /// <summary>Maps dashboard.</summary>
    public static DeploymentDashboardResponse ToDashboardResponse(DeploymentDashboardInfo info) =>
        new()
        {
            RunningDeployments = info.RunningDeployments,
            DownloadingModels = info.DownloadingModels,
            HealthyDeployments = info.HealthyDeployments,
            FailedDeployments = info.FailedDeployments,
            EstimatedMonthlyCostUsd = info.EstimatedMonthlyCostUsd,
            Recent = info.Recent.Select(ToResponse).ToList(),
        };

    /// <summary>Parses provider availability JSON.</summary>
    public static IReadOnlyList<string> ParseProviders(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>Parses model codes JSON.</summary>
    public static IReadOnlyList<string> ParseModelCodes(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
