using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Commercial;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Commercial;

/// <summary>
/// Configuration / database backup jobs under ContentRoot/backups.
/// </summary>
public sealed class BackupService : IBackupService
{
    private readonly IApplicationDbContext db;
    private readonly IHostEnvironment environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupService"/> class.
    /// </summary>
    public BackupService(IApplicationDbContext db, IHostEnvironment environment)
    {
        this.db = db;
        this.environment = environment;
    }

    /// <inheritdoc />
    public async Task<BackupJobInfo> StartAsync(
        Guid? organizationId,
        string backupType,
        bool scheduled,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = string.IsNullOrWhiteSpace(backupType) ? "Database" : backupType.Trim();
        var now = DateTime.UtcNow;
        var job = new BackupJob
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            BackupType = normalizedType,
            Status = "Running",
            IsScheduled = scheduled,
            StartedAt = now,
            CreatedAt = now,
        };

        await db.AddBackupJobAsync(job, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            var orgFolder = organizationId?.ToString("N") ?? "platform";
            var stamp = now.ToString("yyyyMMddHHmmss");
            var directory = Path.Combine(environment.ContentRootPath, "backups", orgFolder, stamp);
            Directory.CreateDirectory(directory);

            var configPath = Path.Combine(directory, "config.json");
            var dumpPath = Path.Combine(directory, "database.sql.placeholder");

            object configPayload;
            if (organizationId.HasValue)
            {
                var org = await db.Organizations.AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == organizationId.Value, cancellationToken);

                var providers = await db.ComputeProviders.AsNoTracking()
                    .Where(p => p.OrganizationId == organizationId.Value)
                    .Select(p => new { p.Id, p.Name, p.ProviderType, p.IsEnabled })
                    .ToListAsync(cancellationToken);

                var pods = await db.GpuPods.AsNoTracking()
                    .Where(p => p.OrganizationId == organizationId.Value)
                    .Select(p => new { p.Id, p.Name, p.Status, p.ProviderId })
                    .ToListAsync(cancellationToken);

                configPayload = new
                {
                    organization = org is null
                        ? null
                        : new { org.Id, org.Name, org.Slug, org.Description, org.IsActive },
                    providers,
                    pods,
                    exportedAt = now,
                    note = "Sensitive credentials intentionally omitted.",
                };
            }
            else
            {
                configPayload = new
                {
                    exportedAt = now,
                    note = "Platform-level backup placeholder.",
                };
            }

            var configJson = JsonSerializer.Serialize(configPayload, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(configPath, configJson, Encoding.UTF8, cancellationToken);

            var dumpContent = $"""
                -- PodPilot SQL dump placeholder
                -- Organization: {organizationId?.ToString() ?? "platform"}
                -- Generated: {now:O}
                -- Replace with mysqldump output in production operators.
                SELECT 1;
                """;
            await File.WriteAllTextAsync(dumpPath, dumpContent, Encoding.UTF8, cancellationToken);

            var size = new FileInfo(configPath).Length + new FileInfo(dumpPath).Length;
            job.StorageLocator = directory;
            job.SizeBytes = size;
            job.Status = "Completed";
            job.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            job.Status = "Failed";
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;
        }

        job.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Map(job);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BackupJobInfo>> ListAsync(
        Guid? organizationId,
        CancellationToken cancellationToken = default)
    {
        var query = db.BackupJobs.AsNoTracking().AsQueryable();
        if (organizationId.HasValue)
        {
            query = query.Where(b => b.OrganizationId == organizationId.Value);
        }

        var jobs = await query.OrderByDescending(b => b.CreatedAt).Take(100).ToListAsync(cancellationToken);
        return jobs.Select(Map).ToList();
    }

    /// <inheritdoc />
    public async Task RestoreAsync(Guid backupJobId, CancellationToken cancellationToken = default)
    {
        var job = await db.BackupJobs.FirstOrDefaultAsync(b => b.Id == backupJobId, cancellationToken)
            ?? throw new NotFoundException("Backup job", backupJobId);

        if (!string.Equals(job.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(
                    nameof(backupJobId),
                    "Only completed backups can be restored."),
            ]);
        }

        // Restoration applies configuration metadata marker; full DB restore is operator-driven.
        job.Status = "Restored";
        job.UpdatedAt = DateTime.UtcNow;
        job.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static BackupJobInfo Map(BackupJob job) =>
        new()
        {
            Id = job.Id,
            OrganizationId = job.OrganizationId,
            BackupType = job.BackupType,
            Status = job.Status,
            StorageLocator = job.StorageLocator,
            SizeBytes = job.SizeBytes,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            ErrorMessage = job.ErrorMessage,
            IsScheduled = job.IsScheduled,
        };
}
