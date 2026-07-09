using MediatR;
using PodPilot.Contracts.Observability;

namespace PodPilot.Application.Observability.Queries.ListAlerts;

/// <summary>
/// Lists alerts for the current organization.
/// </summary>
public sealed class ListAlertsQuery : IRequest<IReadOnlyList<AlertResponse>>
{
    /// <summary>Gets or sets a value indicating whether to include only active alerts.</summary>
    public bool ActiveOnly { get; init; }

    /// <summary>Gets or sets the result limit.</summary>
    public int Limit { get; init; } = 50;
}
