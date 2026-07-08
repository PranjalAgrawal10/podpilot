using MediatR;
using PodPilot.Contracts.Models;

namespace PodPilot.Application.Models.Queries.GetHealth;

/// <summary>
/// Gets model health records for the current organization.
/// </summary>
public sealed class GetHealthQuery : IRequest<IReadOnlyList<ModelHealthResponse>>
{
    /// <summary>
    /// Gets or sets an optional model filter.
    /// </summary>
    public Guid? ModelId { get; set; }

    /// <summary>
    /// Gets or sets the maximum records to return.
    /// </summary>
    public int Limit { get; set; } = 50;
}
