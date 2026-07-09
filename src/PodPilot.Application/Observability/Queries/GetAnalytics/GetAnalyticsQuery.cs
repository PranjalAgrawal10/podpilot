using MediatR;
using PodPilot.Contracts.Observability;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Observability.Queries.GetAnalytics;

/// <summary>
/// Gets usage analytics for the current organization.
/// </summary>
public sealed class GetAnalyticsQuery : IRequest<AnalyticsResponse>
{
    /// <summary>Gets or sets the aggregation period.</summary>
    public MetricsPeriod Period { get; init; } = MetricsPeriod.Daily;

    /// <summary>Gets or sets the optional start time.</summary>
    public DateTime? From { get; init; }

    /// <summary>Gets or sets the optional end time.</summary>
    public DateTime? To { get; init; }

    /// <summary>Gets or sets the optional provider identifier.</summary>
    public Guid? ProviderId { get; init; }

    /// <summary>Gets or sets the optional pod identifier.</summary>
    public Guid? PodId { get; init; }

    /// <summary>Gets or sets the optional model name.</summary>
    public string? ModelName { get; init; }
}
