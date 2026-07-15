using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Commercial;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Commercial;

/// <summary>
/// Opt-in telemetry preference and structured event logging.
/// </summary>
public sealed class TelemetryService : ITelemetryService
{
    private readonly IApplicationDbContext db;
    private readonly ILogger<TelemetryService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryService"/> class.
    /// </summary>
    public TelemetryService(IApplicationDbContext db, ILogger<TelemetryService> logger)
    {
        this.db = db;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<TelemetryPreferenceInfo> GetPreferenceAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var preference = await GetOrCreateAsync(organizationId, cancellationToken);
        return Map(preference);
    }

    /// <inheritdoc />
    public async Task UpdatePreferenceAsync(
        Guid organizationId,
        TelemetryPreferenceInfo preference,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetOrCreateAsync(organizationId, cancellationToken);
        entity.OptIn = preference.OptIn;
        entity.CrashReports = preference.CrashReports;
        entity.PerformanceMetrics = preference.PerformanceMetrics;
        entity.FeatureUsage = preference.FeatureUsage;
        entity.HealthReports = preference.HealthReports;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task TrackAsync(
        Guid organizationId,
        string eventName,
        IReadOnlyDictionary<string, string>? properties = null,
        CancellationToken cancellationToken = default)
    {
        var preference = await GetOrCreateAsync(organizationId, cancellationToken);
        if (!preference.OptIn)
        {
            return;
        }

        logger.LogInformation(
            "Telemetry event {EventName} for organization {OrganizationId} Props={@Properties}",
            eventName,
            organizationId,
            properties ?? new Dictionary<string, string>());
        await Task.CompletedTask;
    }

    private async Task<TelemetryPreference> GetOrCreateAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var preference = await db.TelemetryPreferences
            .FirstOrDefaultAsync(p => p.OrganizationId == organizationId, cancellationToken);

        if (preference is not null)
        {
            return preference;
        }

        preference = new TelemetryPreference
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            OptIn = false,
            CreatedAt = DateTime.UtcNow,
        };

        await db.AddTelemetryPreferenceAsync(preference, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return preference;
    }

    private static TelemetryPreferenceInfo Map(TelemetryPreference preference) =>
        new()
        {
            OptIn = preference.OptIn,
            CrashReports = preference.CrashReports,
            PerformanceMetrics = preference.PerformanceMetrics,
            FeatureUsage = preference.FeatureUsage,
            HealthReports = preference.HealthReports,
        };
}
