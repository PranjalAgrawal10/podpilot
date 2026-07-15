using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Commercial;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Commercial;

/// <summary>
/// Platform release channel and version status.
/// </summary>
public sealed class ReleaseService : IReleaseService
{
    private readonly IApplicationDbContext db;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReleaseService"/> class.
    /// </summary>
    public ReleaseService(IApplicationDbContext db) => this.db = db;

    /// <inheritdoc />
    public async Task EnsureDefaultReleaseAsync(CancellationToken cancellationToken = default)
    {
        var exists = await db.PlatformReleases.AnyAsync(
            r => r.Channel == "stable" && r.Version == "1.0.0",
            cancellationToken);

        if (exists)
        {
            return;
        }

        await db.AddPlatformReleaseAsync(
            new PlatformRelease
            {
                Id = Guid.NewGuid(),
                Version = "1.0.0",
                Channel = "stable",
                ReleaseNotes = "Initial PodPilot commercial platform release.",
                PublishedAt = DateTime.UtcNow,
                DownloadUrl = "https://downloads.podpilot.local/releases/1.0.0",
                IsLatest = true,
                CreatedAt = DateTime.UtcNow,
            },
            cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ReleaseStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDefaultReleaseAsync(cancellationToken);

        var latest = await db.PlatformReleases.AsNoTracking()
            .Where(r => r.Channel == "stable")
            .OrderByDescending(r => r.IsLatest)
            .ThenByDescending(r => r.PublishedAt)
            .FirstAsync(cancellationToken);

        var current = GetCurrentVersion();
        var updateAvailable = !string.Equals(current, latest.Version, StringComparison.OrdinalIgnoreCase)
                              && Version.TryParse(Normalize(current), out var currentVer)
                              && Version.TryParse(Normalize(latest.Version), out var latestVer)
                              && latestVer > currentVer;

        return new ReleaseStatus
        {
            CurrentVersion = current,
            LatestVersion = latest.Version,
            UpdateAvailable = updateAvailable,
            ReleaseNotes = latest.ReleaseNotes,
            DownloadUrl = latest.DownloadUrl,
            Channel = latest.Channel,
        };
    }

    private static string GetCurrentVersion()
    {
        var informational = Assembly.GetEntryAssembly()
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informational))
        {
            var plus = informational.IndexOf('+', StringComparison.Ordinal);
            return plus > 0 ? informational[..plus] : informational;
        }

        return Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "1.0.0";
    }

    private static string Normalize(string version)
    {
        var dash = version.IndexOf('-', StringComparison.Ordinal);
        return dash > 0 ? version[..dash] : version;
    }
}
