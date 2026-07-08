using MediatR;
using PodPilot.Contracts.Models;

namespace PodPilot.Application.Models.Queries.GetModels;

/// <summary>
/// Lists AI models for the current organization.
/// </summary>
public sealed class GetModelsQuery : IRequest<IReadOnlyList<ModelResponse>>
{
    /// <summary>
    /// Gets or sets an optional pod filter.
    /// </summary>
    public Guid? PodId { get; set; }
}
