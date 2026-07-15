using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Commercial;

/// <summary>
/// Seeds subscription plan catalog with realistic quotas.
/// </summary>
public sealed class PlanCatalogSeeder
{
    private static readonly PlanSeed[] Definitions =
    [
        new PlanSeed(
            "free",
            "Free",
            SubscriptionPlanTier.Free,
            PricingModel.Flat,
            0m,
            0m,
            0m,
            1,
            "Hobby projects and evaluation.",
            2,
            1,
            5,
            1,
            2,
            1_000,
            1,
            10),
        new PlanSeed(
            "pro",
            "Pro",
            SubscriptionPlanTier.Pro,
            PricingModel.Hybrid,
            29m,
            290m,
            9m,
            1,
            "Solo builders shipping production pods.",
            10,
            5,
            50,
            3,
            10,
            100_000,
            5,
            100),
        new PlanSeed(
            "team",
            "Team",
            SubscriptionPlanTier.Team,
            PricingModel.SeatBased,
            99m,
            990m,
            19m,
            5,
            "Collaborative teams with shared quotas.",
            50,
            20,
            200,
            10,
            50,
            1_000_000,
            25,
            500),
        new PlanSeed(
            "enterprise",
            "Enterprise",
            SubscriptionPlanTier.Enterprise,
            PricingModel.Hybrid,
            499m,
            4_990m,
            29m,
            20,
            "Large orgs with custom limits and support.",
            500,
            100,
            1_000,
            100,
            500,
            10_000_000,
            100,
            5_000),
    ];

    private readonly IApplicationDbContext db;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanCatalogSeeder"/> class.
    /// </summary>
    public PlanCatalogSeeder(IApplicationDbContext db) => this.db = db;

    /// <summary>Ensures Free/Pro/Team/Enterprise plans exist.</summary>
    public async Task EnsureAsync(CancellationToken cancellationToken = default)
    {
        var existing = await db.SubscriptionPlans.AsNoTracking().Select(p => p.Code).ToListAsync(cancellationToken);
        var existingSet = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);

        foreach (var definition in Definitions)
        {
            if (existingSet.Contains(definition.Code))
            {
                continue;
            }

            var plan = new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Code = definition.Code,
                Name = definition.Name,
                Tier = definition.Tier,
                PricingModel = definition.PricingModel,
                MonthlyPriceUsd = definition.MonthlyPriceUsd,
                YearlyPriceUsd = definition.YearlyPriceUsd,
                SeatPriceUsd = definition.SeatPriceUsd,
                IncludedSeats = definition.IncludedSeats,
                Description = definition.Description,
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
            };

            await db.AddSubscriptionPlanAsync(plan, cancellationToken);
            await db.AddPlanQuotaAsync(
                new PlanQuota
                {
                    Id = Guid.NewGuid(),
                    SubscriptionPlanId = plan.Id,
                    MaxPods = definition.MaxPods,
                    MaxProviders = definition.MaxProviders,
                    MaxModels = definition.MaxModels,
                    MaxOrganizations = definition.MaxOrganizations,
                    MaxTeamMembers = definition.MaxTeamMembers,
                    MaxApiRequestsPerMonth = definition.MaxApiRequestsPerMonth,
                    MaxConcurrentStreams = definition.MaxConcurrentStreams,
                    MaxStorageGb = definition.MaxStorageGb,
                    CreatedAt = DateTime.UtcNow,
                },
                cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private sealed class PlanSeed
    {
        public PlanSeed(
            string code,
            string name,
            SubscriptionPlanTier tier,
            PricingModel pricingModel,
            decimal monthlyPriceUsd,
            decimal yearlyPriceUsd,
            decimal seatPriceUsd,
            int includedSeats,
            string description,
            int maxPods,
            int maxProviders,
            int maxModels,
            int maxOrganizations,
            int maxTeamMembers,
            long maxApiRequestsPerMonth,
            int maxConcurrentStreams,
            int maxStorageGb)
        {
            Code = code;
            Name = name;
            Tier = tier;
            PricingModel = pricingModel;
            MonthlyPriceUsd = monthlyPriceUsd;
            YearlyPriceUsd = yearlyPriceUsd;
            SeatPriceUsd = seatPriceUsd;
            IncludedSeats = includedSeats;
            Description = description;
            MaxPods = maxPods;
            MaxProviders = maxProviders;
            MaxModels = maxModels;
            MaxOrganizations = maxOrganizations;
            MaxTeamMembers = maxTeamMembers;
            MaxApiRequestsPerMonth = maxApiRequestsPerMonth;
            MaxConcurrentStreams = maxConcurrentStreams;
            MaxStorageGb = maxStorageGb;
        }

        public string Code { get; }

        public string Name { get; }

        public SubscriptionPlanTier Tier { get; }

        public PricingModel PricingModel { get; }

        public decimal MonthlyPriceUsd { get; }

        public decimal YearlyPriceUsd { get; }

        public decimal SeatPriceUsd { get; }

        public int IncludedSeats { get; }

        public string Description { get; }

        public int MaxPods { get; }

        public int MaxProviders { get; }

        public int MaxModels { get; }

        public int MaxOrganizations { get; }

        public int MaxTeamMembers { get; }

        public long MaxApiRequestsPerMonth { get; }

        public int MaxConcurrentStreams { get; }

        public int MaxStorageGb { get; }
    }
}
