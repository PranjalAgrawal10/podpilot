using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Selects the best model from scored candidates.
/// </summary>
public sealed class ModelRouter : IModelRouter
{
    private readonly IModelScorer modelScorer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelRouter"/> class.
    /// </summary>
    public ModelRouter(IModelScorer modelScorer)
    {
        this.modelScorer = modelScorer;
    }

    /// <inheritdoc />
    public Task<RoutingCandidate?> SelectModelAsync(
        IReadOnlyList<RoutingCandidate> candidates,
        RoutingRequestAnalysis analysis,
        RoutingScoreWeights weights,
        CancellationToken cancellationToken = default)
    {
        if (candidates.Count == 0)
        {
            return Task.FromResult<RoutingCandidate?>(null);
        }

        var scored = modelScorer.Score(candidates, analysis, weights);
        return Task.FromResult<RoutingCandidate?>(scored.FirstOrDefault());
    }
}
