using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// System clock implementation.
/// </summary>
public sealed class DateTimeService : IDateTimeService
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
