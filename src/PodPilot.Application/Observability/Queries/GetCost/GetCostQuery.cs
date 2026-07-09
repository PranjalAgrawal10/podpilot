using MediatR;
using PodPilot.Contracts.Observability;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Observability.Queries.GetCost;

/// <summary>
/// Gets cost summary for the current organization.
/// </summary>
public sealed class GetCostQuery : IRequest<CostResponse>
{
    /// <summary>Gets or sets the aggregation period.</summary>
    public MetricsPeriod Period { get; init; } = MetricsPeriod.Hourly;

    /// <summary>Gets or sets the optional provider identifier.</summary>
    public Guid? ProviderId { get; init; }

    /// <summary>Gets or sets the optional pod identifier.</summary>
    public Guid? PodId { get; init; }

    /// <summary>Gets or sets the optional model name.</summary>
    public string? ModelName { get; init; }
}
