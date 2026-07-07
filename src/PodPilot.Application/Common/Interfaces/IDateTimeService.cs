namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Abstraction for accessing the current UTC time.
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}
