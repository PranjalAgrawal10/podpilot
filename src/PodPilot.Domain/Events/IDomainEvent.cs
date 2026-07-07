namespace PodPilot.Domain.Events;

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the UTC timestamp when the event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}
