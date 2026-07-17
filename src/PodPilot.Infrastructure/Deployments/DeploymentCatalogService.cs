using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Deployments;

namespace PodPilot.Infrastructure.Deployments;

/// <summary>
/// Catalog and GPU recommendation service for deployments.
/// </summary>
public sealed class DeploymentCatalogService : IDeploymentCatalogService
{
    private readonly IApplicationDbContext db;
    private readonly DeploymentCatalogSeeder seeder;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentCatalogService"/> class.
    /// </summary>
    public DeploymentCatalogService(IApplicationDbContext db, DeploymentCatalogSeeder seeder)
    {
        this.db = db;
        this.seeder = seeder;
    }

    /// <inheritdoc />
    public Task EnsureSeededAsync(CancellationToken cancellationToken = default) =>
        seeder.EnsureAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<GpuCatalogInfo>> ListGpusAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken);
        var rows = await db.GpuCatalogEntries
            .AsNoTracking()
            .Where(g => g.IsActive)
            .OrderBy(g => g.SortOrder)
            .ToListAsync(cancellationToken);

        return rows.Select(g => new GpuCatalogInfo
        {
            Id = g.Id,
            Code = g.Code,
            Name = g.Name,
            GpuType = g.GpuType,
            VramGb = g.VramGb,
            CudaCapability = g.CudaCapability,
            EstimatedHourlyCostUsd = g.EstimatedHourlyCostUsd,
            ProviderAvailability = ParseStringList(g.ProviderAvailabilityJson),
            IsCustom = g.IsCustom,
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ModelCatalogInfo>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken);
        var rows = await db.ModelCatalogEntries
            .AsNoTracking()
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);

        return rows.Select(m => new ModelCatalogInfo
        {
            Id = m.Id,
            Code = m.Code,
            ModelReference = m.ModelReference,
            Name = m.Name,
            Provider = m.Provider,
            Version = m.Version,
            Family = m.Family,
            Parameters = m.Parameters,
            Quantization = m.Quantization,
            ContextLength = m.ContextLength,
            RequiredVramGb = m.RequiredVramGb,
            RecommendedGpuCode = m.RecommendedGpuCode,
            MinimumGpuCode = m.MinimumGpuCode,
            SupportsVision = m.SupportsVision,
            SupportsTools = m.SupportsTools,
            SupportsEmbeddings = m.SupportsEmbeddings,
            License = m.License,
            DownloadSizeGb = m.DownloadSizeGb,
            PreferredRuntime = m.PreferredRuntime,
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DeploymentTemplateInfo>> ListTemplatesAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken);
        var rows = await db.DeploymentTemplates
            .AsNoTracking()
            .Where(t => t.IsPublic)
            .OrderBy(t => t.SortOrder)
            .ToListAsync(cancellationToken);

        return rows.Select(t => new DeploymentTemplateInfo
        {
            Id = t.Id,
            Code = t.Code,
            Name = t.Name,
            Kind = t.Kind,
            Description = t.Description,
            Runtime = t.Runtime,
            ContainerImage = t.ContainerImage,
            RecommendedGpuCode = t.RecommendedGpuCode,
            DefaultModelCodes = ParseStringList(t.DefaultModelCodesJson),
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DeploymentRegionInfo>> ListRegionsAsync(
        Guid organizationId,
        Guid providerId,
        string? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        var providerExists = await db.ComputeProviders.AnyAsync(
            p => p.Id == providerId && p.OrganizationId == organizationId,
            cancellationToken);

        if (!providerExists)
        {
            throw new NotFoundException("Provider", providerId);
        }

        var regions = await db.ProviderRegions
            .AsNoTracking()
            .Where(r => r.ComputeProviderId == providerId && r.IsAvailable)
            .ToListAsync(cancellationToken);

        var mapped = regions.Select((r, index) =>
        {
            var area = InferArea(r.Name, r.RegionId);
            var latency = InferLatencyMs(area);
            var priceScore = 1m + (index % 5);
            return new DeploymentRegionInfo
            {
                Code = r.RegionId,
                Name = string.IsNullOrWhiteSpace(r.Name) ? r.RegionId : r.Name,
                Area = area,
                EstimatedLatencyMs = latency,
                PriceScore = priceScore,
                AvailabilityScore = 100,
            };
        }).ToList();

        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "price" => mapped.OrderBy(r => r.PriceScore).ToList(),
            "availability" => mapped.OrderByDescending(r => r.AvailabilityScore).ToList(),
            "latency" => mapped.OrderBy(r => r.EstimatedLatencyMs ?? int.MaxValue).ToList(),
            _ => mapped.OrderBy(r => r.Name).ToList(),
        };
    }

    /// <inheritdoc />
    public async Task<GpuRecommendationResult> RecommendGpuAsync(
        IReadOnlyList<string> modelCodesOrReferences,
        CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken);

        if (modelCodesOrReferences.Count == 0)
        {
            throw new ValidationException("At least one model is required for GPU recommendation.");
        }

        var catalog = await db.ModelCatalogEntries
            .AsNoTracking()
            .Where(m => m.IsActive)
            .ToListAsync(cancellationToken);

        var matched = new List<Domain.Entities.ModelCatalogEntry>();
        var warnings = new List<string>();

        foreach (var key in modelCodesOrReferences.Where(s => !string.IsNullOrWhiteSpace(s)))
        {
            var trimmed = key.Trim();
            var entry = catalog.FirstOrDefault(m =>
                m.Code.Equals(trimmed, StringComparison.OrdinalIgnoreCase)
                || m.ModelReference.Equals(trimmed, StringComparison.OrdinalIgnoreCase));

            if (entry is null)
            {
                warnings.Add($"Model '{trimmed}' is not in the catalog; VRAM estimate may be incomplete.");
                continue;
            }

            matched.Add(entry);
        }

        var requiredVram = matched.Count == 0 ? 24 : matched.Max(m => m.RequiredVramGb);
        var gpus = await db.GpuCatalogEntries
            .AsNoTracking()
            .Where(g => g.IsActive)
            .OrderBy(g => g.VramGb)
            .ThenBy(g => g.EstimatedHourlyCostUsd)
            .ToListAsync(cancellationToken);

        var minimum = gpus.FirstOrDefault(g => g.VramGb >= requiredVram)
            ?? gpus.LastOrDefault()
            ?? throw new InvalidOperationException("GPU catalog is empty.");

        var recommendedCode = matched
            .Select(m => m.RecommendedGpuCode)
            .GroupBy(c => c, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        var recommended = gpus.FirstOrDefault(g =>
            g.Code.Equals(recommendedCode, StringComparison.OrdinalIgnoreCase)
            && g.VramGb >= requiredVram)
            ?? minimum;

        if (matched.Count > 0
            && recommended.VramGb < matched.Max(m => m.RequiredVramGb))
        {
            recommended = minimum;
        }

        return new GpuRecommendationResult
        {
            RecommendedGpuCode = recommended.Code,
            MinimumGpuCode = minimum.Code,
            RequiredVramGb = requiredVram,
            EstimatedPerformance = $"{Math.Max(10, 120 - requiredVram)}–{Math.Max(20, 180 - requiredVram)} tok/s (estimate)",
            Warnings = warnings,
        };
    }

    private static IReadOnlyList<string> ParseStringList(string? json)
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

    private static string InferArea(string name, string regionId)
    {
        var haystack = $"{name} {regionId}".ToUpperInvariant();
        if (haystack.Contains("EU", StringComparison.Ordinal) || haystack.Contains("EUROPE", StringComparison.Ordinal))
        {
            return "Europe";
        }

        if (haystack.Contains("ASIA", StringComparison.Ordinal)
            || haystack.Contains("AP", StringComparison.Ordinal)
            || haystack.Contains("JP", StringComparison.Ordinal)
            || haystack.Contains("SG", StringComparison.Ordinal))
        {
            return "Asia";
        }

        if (haystack.Contains("AU", StringComparison.Ordinal) || haystack.Contains("OCEANIA", StringComparison.Ordinal))
        {
            return "Oceania";
        }

        return "Americas";
    }

    private static int InferLatencyMs(string area) =>
        area switch
        {
            "Europe" => 90,
            "Asia" => 180,
            "Oceania" => 220,
            _ => 40,
        };
}
