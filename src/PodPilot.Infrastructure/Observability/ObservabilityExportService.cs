using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Observability;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Observability;

/// <summary>
/// Exports observability data in CSV, Excel, or JSON formats.
/// </summary>
public sealed class ObservabilityExportService : IObservabilityExportService
{
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservabilityExportService"/> class.
    /// </summary>
    public ObservabilityExportService(IApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<ExportResult> ExportAsync(
        Guid organizationId,
        ExportFormat format,
        ObservabilityExportType exportType,
        ObservabilityExportFilter filter,
        CancellationToken cancellationToken = default)
    {
        var rows = exportType switch
        {
            ObservabilityExportType.Metrics => await ExportMetricsAsync(organizationId, filter, cancellationToken),
            ObservabilityExportType.Cost => await ExportCostAsync(organizationId, filter, cancellationToken),
            ObservabilityExportType.Usage => await ExportUsageAsync(organizationId, filter, cancellationToken),
            ObservabilityExportType.Alerts => await ExportAlertsAsync(organizationId, filter, cancellationToken),
            ObservabilityExportType.Health => await ExportHealthAsync(organizationId, filter, cancellationToken),
            _ => await ExportMetricsAsync(organizationId, filter, cancellationToken),
        };

        var baseName = $"observability-{exportType.ToString().ToLowerInvariant()}-{DateTime.UtcNow:yyyyMMddHHmmss}";

        return format switch
        {
            ExportFormat.Json => new ExportResult
            {
                Content = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true })),
                ContentType = "application/json",
                FileName = $"{baseName}.json",
            },
            ExportFormat.Excel => new ExportResult
            {
                Content = Encoding.UTF8.GetBytes(ToCsv(rows)),
                ContentType = "application/vnd.ms-excel",
                FileName = $"{baseName}.csv",
            },
            _ => new ExportResult
            {
                Content = Encoding.UTF8.GetBytes(ToCsv(rows)),
                ContentType = "text/csv",
                FileName = $"{baseName}.csv",
            },
        };
    }

    private async Task<IReadOnlyList<Dictionary<string, string?>>> ExportMetricsAsync(
        Guid organizationId,
        ObservabilityExportFilter filter,
        CancellationToken cancellationToken)
    {
        var query = dbContext.MetricsSnapshots.Where(m => m.OrganizationId == organizationId);

        if (filter.From.HasValue)
        {
            query = query.Where(m => m.RecordedAt >= filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(m => m.RecordedAt <= filter.To.Value);
        }

        if (filter.ProviderId.HasValue)
        {
            query = query.Where(m => m.ProviderId == filter.ProviderId.Value);
        }

        if (filter.PodId.HasValue)
        {
            query = query.Where(m => m.GpuPodId == filter.PodId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.ModelName))
        {
            query = query.Where(m => m.ModelName == filter.ModelName);
        }

        var items = await query.OrderByDescending(m => m.RecordedAt).Take(10_000).ToListAsync(cancellationToken);

        return items.Select(m => new Dictionary<string, string?>
        {
            ["RecordedAt"] = m.RecordedAt.ToString("O", CultureInfo.InvariantCulture),
            ["GpuUtilizationPercent"] = m.GpuUtilizationPercent.ToString(CultureInfo.InvariantCulture),
            ["CpuUtilizationPercent"] = m.CpuUtilizationPercent.ToString(CultureInfo.InvariantCulture),
            ["QueueSize"] = m.QueueSize.ToString(CultureInfo.InvariantCulture),
            ["ActiveStreams"] = m.ActiveStreams.ToString(CultureInfo.InvariantCulture),
            ["InferenceCount"] = m.InferenceCount.ToString(CultureInfo.InvariantCulture),
            ["TokensGenerated"] = m.TokensGenerated.ToString(CultureInfo.InvariantCulture),
            ["AverageLatencyMs"] = m.AverageLatencyMs.ToString(CultureInfo.InvariantCulture),
            ["ErrorRate"] = m.ErrorRate.ToString(CultureInfo.InvariantCulture),
        }).ToList();
    }

    private async Task<IReadOnlyList<Dictionary<string, string?>>> ExportCostAsync(
        Guid organizationId,
        ObservabilityExportFilter filter,
        CancellationToken cancellationToken)
    {
        var query = dbContext.CostSnapshots.Where(c => c.OrganizationId == organizationId);

        if (filter.From.HasValue)
        {
            query = query.Where(c => c.RecordedAt >= filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(c => c.RecordedAt <= filter.To.Value);
        }

        var items = await query.OrderByDescending(c => c.RecordedAt).Take(10_000).ToListAsync(cancellationToken);

        return items.Select(c => new Dictionary<string, string?>
        {
            ["RecordedAt"] = c.RecordedAt.ToString("O", CultureInfo.InvariantCulture),
            ["Period"] = c.Period.ToString(),
            ["HourlyCost"] = c.HourlyCost.ToString(CultureInfo.InvariantCulture),
            ["DailyCost"] = c.DailyCost.ToString(CultureInfo.InvariantCulture),
            ["WeeklyCost"] = c.WeeklyCost.ToString(CultureInfo.InvariantCulture),
            ["MonthlyCost"] = c.MonthlyCost.ToString(CultureInfo.InvariantCulture),
            ["ProjectedMonthlyCost"] = c.ProjectedMonthlyCost.ToString(CultureInfo.InvariantCulture),
            ["AutoShutdownSavings"] = c.AutoShutdownSavings.ToString(CultureInfo.InvariantCulture),
        }).ToList();
    }

    private async Task<IReadOnlyList<Dictionary<string, string?>>> ExportUsageAsync(
        Guid organizationId,
        ObservabilityExportFilter filter,
        CancellationToken cancellationToken)
    {
        var query = dbContext.UsageStatistics.Where(u => u.OrganizationId == organizationId);

        if (filter.From.HasValue)
        {
            query = query.Where(u => u.RecordedAt >= filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(u => u.RecordedAt <= filter.To.Value);
        }

        var items = await query.OrderByDescending(u => u.RecordedAt).Take(10_000).ToListAsync(cancellationToken);

        return items.Select(u => new Dictionary<string, string?>
        {
            ["RecordedAt"] = u.RecordedAt.ToString("O", CultureInfo.InvariantCulture),
            ["Period"] = u.Period.ToString(),
            ["RequestCount"] = u.RequestCount.ToString(CultureInfo.InvariantCulture),
            ["TokenCount"] = u.TokenCount.ToString(CultureInfo.InvariantCulture),
            ["InferenceCount"] = u.InferenceCount.ToString(CultureInfo.InvariantCulture),
            ["TotalLatencyMs"] = u.TotalLatencyMs.ToString(CultureInfo.InvariantCulture),
            ["ErrorCount"] = u.ErrorCount.ToString(CultureInfo.InvariantCulture),
            ["UptimeSeconds"] = u.UptimeSeconds.ToString(CultureInfo.InvariantCulture),
        }).ToList();
    }

    private async Task<IReadOnlyList<Dictionary<string, string?>>> ExportAlertsAsync(
        Guid organizationId,
        ObservabilityExportFilter filter,
        CancellationToken cancellationToken)
    {
        var query = dbContext.AlertHistory.Where(a => a.OrganizationId == organizationId);

        if (filter.From.HasValue)
        {
            query = query.Where(a => a.RaisedAt >= filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(a => a.RaisedAt <= filter.To.Value);
        }

        var items = await query.OrderByDescending(a => a.RaisedAt).Take(10_000).ToListAsync(cancellationToken);

        return items.Select(a => new Dictionary<string, string?>
        {
            ["RaisedAt"] = a.RaisedAt.ToString("O", CultureInfo.InvariantCulture),
            ["ResolvedAt"] = a.ResolvedAt?.ToString("O", CultureInfo.InvariantCulture),
            ["AlertType"] = a.AlertType.ToString(),
            ["Severity"] = a.Severity.ToString(),
            ["Title"] = a.Title,
            ["Message"] = a.Message,
            ["IsActive"] = a.IsActive.ToString(),
        }).ToList();
    }

    private async Task<IReadOnlyList<Dictionary<string, string?>>> ExportHealthAsync(
        Guid organizationId,
        ObservabilityExportFilter filter,
        CancellationToken cancellationToken)
    {
        var query = dbContext.SystemHealthHistory
            .Where(h => h.OrganizationId == organizationId || h.OrganizationId == null);

        if (filter.From.HasValue)
        {
            query = query.Where(h => h.RecordedAt >= filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(h => h.RecordedAt <= filter.To.Value);
        }

        var items = await query.OrderByDescending(h => h.RecordedAt).Take(10_000).ToListAsync(cancellationToken);

        return items.Select(h => new Dictionary<string, string?>
        {
            ["RecordedAt"] = h.RecordedAt.ToString("O", CultureInfo.InvariantCulture),
            ["Component"] = h.Component.ToString(),
            ["Status"] = h.Status.ToString(),
            ["Message"] = h.Message,
        }).ToList();
    }

    private static string ToCsv(IReadOnlyList<Dictionary<string, string?>> rows)
    {
        if (rows.Count == 0)
        {
            return string.Empty;
        }

        var headers = rows[0].Keys.ToList();
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", headers.Select(h => EscapeCsv(row.GetValueOrDefault(h)))));
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(',', StringComparison.Ordinal) || value.Contains('"', StringComparison.Ordinal))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
