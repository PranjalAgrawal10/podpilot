using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Commercial;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Commercial;

/// <summary>
/// Aggregates and records usage for billing.
/// </summary>
public sealed class UsageMeteringService : IUsageMeteringService
{
    private readonly IApplicationDbContext db;
    private readonly IQuotaService quotaService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsageMeteringService"/> class.
    /// </summary>
    public UsageMeteringService(IApplicationDbContext db, IQuotaService quotaService)
    {
        this.db = db;
        this.quotaService = quotaService;
    }

    /// <inheritdoc />
    public async Task RecordAsync(
        Guid organizationId,
        UsageMetricKind metric,
        decimal quantity,
        decimal estimatedCostUsd = 0,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1);

        var existing = await db.UsageRecords
            .FirstOrDefaultAsync(
                r => r.OrganizationId == organizationId
                     && r.MetricKind == metric
                     && r.PeriodStart == periodStart,
                cancellationToken);

        if (existing is null)
        {
            await db.AddUsageRecordAsync(
                new UsageRecord
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    MetricKind = metric,
                    Quantity = quantity,
                    EstimatedCostUsd = estimatedCostUsd,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    CreatedAt = now,
                },
                cancellationToken);
        }
        else
        {
            existing.Quantity += quantity;
            existing.EstimatedCostUsd += estimatedCostUsd;
            existing.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UsageDashboard> GetUsageAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1);
        var quotas = await quotaService.GetLimitsAsync(organizationId, cancellationToken);

        var pods = await db.GpuPods.AsNoTracking()
            .Where(p => p.OrganizationId == organizationId && p.Status != PodStatus.Deleted)
            .Select(p => new { p.Status, p.LastStartedAt, p.HourlyCost, p.CreatedAt })
            .ToListAsync(cancellationToken);

        decimal gpuHours = 0;
        decimal gpuCost = 0;
        foreach (var pod in pods)
        {
            if (pod.Status == PodStatus.Running && pod.LastStartedAt.HasValue)
            {
                var started = pod.LastStartedAt.Value < periodStart ? periodStart : pod.LastStartedAt.Value;
                var hours = (decimal)(now - started).TotalHours;
                if (hours > 0)
                {
                    gpuHours += hours;
                    gpuCost += hours * (pod.HourlyCost ?? 0.4m);
                }
            }
            else if (pod.LastStartedAt.HasValue)
            {
                // Conservative estimate: 1 hour if started this period
                if (pod.LastStartedAt.Value >= periodStart)
                {
                    gpuHours += 1m;
                    gpuCost += pod.HourlyCost ?? 0.4m;
                }
            }
        }

        var requests = await db.GatewayRequests.AsNoTracking()
            .CountAsync(
                r => r.OrganizationId == organizationId && r.CreatedAt >= periodStart && r.CreatedAt < periodEnd,
                cancellationToken);

        var tokensFromUsage = await db.UsageStatistics.AsNoTracking()
            .Where(u => u.OrganizationId == organizationId && u.RecordedAt >= periodStart && u.RecordedAt < periodEnd)
            .SumAsync(u => (long?)u.TokenCount, cancellationToken) ?? 0;

        var tokensFromCost = await db.CostHistories.AsNoTracking()
            .Where(c => c.OrganizationId == organizationId && c.RecordedAt >= periodStart && c.RecordedAt < periodEnd)
            .SumAsync(c => (long?)(c.InputTokens + c.OutputTokens), cancellationToken) ?? 0;

        var costHistoryUsd = await db.CostHistories.AsNoTracking()
            .Where(c => c.OrganizationId == organizationId && c.RecordedAt >= periodStart && c.RecordedAt < periodEnd)
            .SumAsync(c => (decimal?)c.CostUsd, cancellationToken) ?? 0m;

        var organizations = 1;

        var models = await db.AiModels.AsNoTracking()
            .CountAsync(m => m.OrganizationId == organizationId, cancellationToken);
        var providers = await db.ComputeProviders.AsNoTracking()
            .CountAsync(p => p.OrganizationId == organizationId, cancellationToken);

        var recorded = await db.UsageRecords.AsNoTracking()
            .Where(r => r.OrganizationId == organizationId && r.PeriodStart == periodStart)
            .ToListAsync(cancellationToken);

        var bandwidth = recorded.Where(r => r.MetricKind == UsageMetricKind.BandwidthGb).Sum(r => r.Quantity);
        var storage = recorded.Where(r => r.MetricKind == UsageMetricKind.StorageGb).Sum(r => r.Quantity);
        var recordedGpu = recorded.Where(r => r.MetricKind == UsageMetricKind.GpuHours).Sum(r => r.Quantity);
        var recordedRequests = recorded.Where(r => r.MetricKind == UsageMetricKind.Requests).Sum(r => r.Quantity);
        var recordedTokens = recorded.Where(r => r.MetricKind == UsageMetricKind.Tokens).Sum(r => r.Quantity);

        gpuHours = Math.Max(gpuHours, recordedGpu);
        var totalRequests = Math.Max(requests, (long)recordedRequests);
        var tokens = Math.Max(tokensFromUsage + tokensFromCost, (long)recordedTokens);
        var estimatedCost = gpuCost + costHistoryUsd + recorded.Sum(r => r.EstimatedCostUsd);

        var requestQuotaPercent = quotas.MaxApiRequestsPerMonth <= 0
            ? 0
            : Math.Min(100m, (decimal)totalRequests / quotas.MaxApiRequestsPerMonth * 100m);

        return new UsageDashboard
        {
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            GpuHours = Math.Round(gpuHours, 2),
            Requests = totalRequests,
            Tokens = tokens,
            BandwidthGb = bandwidth,
            StorageGb = storage,
            Organizations = organizations,
            Models = models,
            Providers = providers,
            EstimatedMonthlyCostUsd = Math.Round(estimatedCost, 2),
            Quotas = quotas,
            RequestsQuotaPercent = Math.Round(requestQuotaPercent, 2),
        };
    }

    /// <inheritdoc />
    public async Task<InvoiceInfo> GenerateInvoiceAsync(
        Guid organizationId,
        DateTime? periodStart = null,
        CancellationToken cancellationToken = default)
    {
        var usage = await GetUsageAsync(organizationId, cancellationToken);
        var start = periodStart ?? usage.PeriodStart;
        var end = start.AddMonths(1);

        var subscription = await db.OrganizationSubscriptions.AsNoTracking()
            .Include(s => s.SubscriptionPlan)
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId, cancellationToken);

        var planPrice = subscription?.BillingInterval == BillingInterval.Yearly
            ? (subscription.SubscriptionPlan?.YearlyPriceUsd ?? 0m) / 12m
            : subscription?.SubscriptionPlan?.MonthlyPriceUsd ?? 0m;

        var seatExtra = Math.Max(0, (subscription?.SeatCount ?? 1) - (subscription?.SubscriptionPlan?.IncludedSeats ?? 1));
        var seatCost = seatExtra * (subscription?.SubscriptionPlan?.SeatPriceUsd ?? 0m);
        var usageCost = usage.EstimatedMonthlyCostUsd;
        var subtotal = planPrice + seatCost + usageCost;
        var tax = Math.Round(subtotal * 0.0m, 2);
        var total = subtotal + tax;

        var lineItems = JsonSerializer.Serialize(new[]
        {
            new { description = "Plan", amount = planPrice },
            new { description = "Seats", amount = seatCost },
            new { description = "Usage", amount = usageCost },
        });

        var invoiceNumber = $"INV-{start:yyyyMM}-{organizationId.ToString("N")[..8].ToUpperInvariant()}";
        var existing = await db.Invoices
            .FirstOrDefaultAsync(i => i.OrganizationId == organizationId && i.InvoiceNumber == invoiceNumber, cancellationToken);

        if (existing is not null)
        {
            return new InvoiceInfo
            {
                Id = existing.Id,
                InvoiceNumber = existing.InvoiceNumber,
                Status = existing.Status,
                SubtotalUsd = existing.SubtotalUsd,
                TaxUsd = existing.TaxUsd,
                TotalUsd = existing.TotalUsd,
                PeriodStart = existing.PeriodStart,
                PeriodEnd = existing.PeriodEnd,
                LineItemsJson = existing.LineItemsJson,
            };
        }

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            InvoiceNumber = invoiceNumber,
            Status = "Open",
            Currency = "USD",
            SubtotalUsd = Math.Round(subtotal, 2),
            TaxUsd = tax,
            TotalUsd = Math.Round(total, 2),
            PeriodStart = start,
            PeriodEnd = end,
            LineItemsJson = lineItems,
            CreatedAt = DateTime.UtcNow,
        };

        await db.AddInvoiceAsync(invoice, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new InvoiceInfo
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            Status = invoice.Status,
            SubtotalUsd = invoice.SubtotalUsd,
            TaxUsd = invoice.TaxUsd,
            TotalUsd = invoice.TotalUsd,
            PeriodStart = invoice.PeriodStart,
            PeriodEnd = invoice.PeriodEnd,
            LineItemsJson = invoice.LineItemsJson,
        };
    }
}
