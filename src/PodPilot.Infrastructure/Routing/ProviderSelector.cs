using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Loads provider/model candidates filtered by required capabilities.
/// </summary>
public sealed class ProviderSelector : IProviderSelector
{
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderSelector"/> class.
    /// </summary>
    public ProviderSelector(IApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RoutingCandidate>> SelectProvidersAsync(
        Guid organizationId,
        RoutingRequestAnalysis analysis,
        CancellationToken cancellationToken = default)
    {
        var models = await dbContext.AiProviderModels
            .AsNoTracking()
            .Include(m => m.AiProvider)
            .Where(m =>
                m.OrganizationId == organizationId &&
                m.IsEnabled &&
                m.AiProvider.IsEnabled &&
                m.AiProvider.IsValidated)
            .ToListAsync(cancellationToken);

        IEnumerable<Domain.Entities.AiProviderModel> filtered = models;

        if (!string.IsNullOrWhiteSpace(analysis.RequestedModel) &&
            !string.Equals(analysis.RequestedModel, "auto", StringComparison.OrdinalIgnoreCase))
        {
            var exact = models.Where(m =>
                string.Equals(m.ModelName, analysis.RequestedModel, StringComparison.OrdinalIgnoreCase)).ToList();
            if (exact.Count > 0)
            {
                filtered = exact;
            }
        }

        if (analysis.RequiresVision)
        {
            filtered = filtered.Where(m => m.SupportsVision);
        }

        if (analysis.RequiresEmbeddings)
        {
            filtered = filtered.Where(m => m.SupportsEmbeddings);
        }

        if (analysis.RequiresTools)
        {
            filtered = filtered.Where(m => m.SupportsTools);
        }

        var candidates = filtered.Select(m => new RoutingCandidate
        {
            ProviderId = m.AiProviderId,
            ProviderName = m.AiProvider.DisplayName,
            ProviderKind = m.AiProvider.ProviderKind,
            ModelId = m.Id,
            ModelName = m.ModelName,
            ContextLength = m.ContextLength,
            SupportsStreaming = m.SupportsStreaming,
            SupportsVision = m.SupportsVision,
            SupportsTools = m.SupportsTools,
            SupportsEmbeddings = m.SupportsEmbeddings,
            SupportsReasoning = m.SupportsReasoning,
            InputCostPerMillionTokens = m.InputCostPerMillionTokens,
            OutputCostPerMillionTokens = m.OutputCostPerMillionTokens,
            SpeedScore = m.SpeedScore,
            QualityScore = m.QualityScore,
            ReliabilityScore = m.ReliabilityScore,
        }).ToList();

        if (candidates.Count == 0 &&
            !string.IsNullOrWhiteSpace(analysis.RequestedModel) &&
            !string.Equals(analysis.RequestedModel, "auto", StringComparison.OrdinalIgnoreCase) &&
            !analysis.RequiresEmbeddings)
        {
            var providers = await dbContext.AiInferenceProviders
                .AsNoTracking()
                .Where(p => p.OrganizationId == organizationId && p.IsEnabled && p.IsValidated)
                .OrderBy(p => p.Priority)
                .ThenBy(p => p.Name)
                .ToListAsync(cancellationToken);

            candidates = providers.Select(p => new RoutingCandidate
            {
                ProviderId = p.Id,
                ProviderName = p.DisplayName,
                ProviderKind = p.ProviderKind,
                ModelId = Guid.Empty,
                ModelName = analysis.RequestedModel!,
                SupportsStreaming = true,
                SpeedScore = 50,
                QualityScore = 50,
                ReliabilityScore = 50,
            }).ToList();
        }

        return candidates;
    }
}
