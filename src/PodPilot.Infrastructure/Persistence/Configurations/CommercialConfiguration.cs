using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>EF configuration for subscription plans.</summary>
public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.StripeMonthlyPriceId).HasMaxLength(100);
        builder.Property(x => x.StripeYearlyPriceId).HasMaxLength(100);
        builder.Property(x => x.RazorpayMonthlyPlanId).HasMaxLength(100);
        builder.Property(x => x.RazorpayYearlyPlanId).HasMaxLength(100);
        builder.Property(x => x.MonthlyPriceUsd).HasPrecision(18, 4);
        builder.Property(x => x.YearlyPriceUsd).HasPrecision(18, 4);
        builder.Property(x => x.SeatPriceUsd).HasPrecision(18, 4);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasOne(x => x.Quota)
            .WithOne(q => q.SubscriptionPlan)
            .HasForeignKey<PlanQuota>(q => q.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF configuration for plan quotas.</summary>
public class PlanQuotaConfiguration : IEntityTypeConfiguration<PlanQuota>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PlanQuota> builder)
    {
        builder.ToTable("PlanQuotas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => x.SubscriptionPlanId).IsUnique();
    }
}

/// <summary>EF configuration for organization subscriptions.</summary>
public class OrganizationSubscriptionConfiguration : IEntityTypeConfiguration<OrganizationSubscription>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrganizationSubscription> builder)
    {
        builder.ToTable("OrganizationSubscriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalCustomerId).HasMaxLength(191);
        builder.Property(x => x.ExternalSubscriptionId).HasMaxLength(191);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => x.OrganizationId).IsUnique();
        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.SubscriptionPlan)
            .WithMany()
            .HasForeignKey(x => x.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>EF configuration for usage records.</summary>
public class UsageRecordConfiguration : IEntityTypeConfiguration<UsageRecord>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UsageRecord> builder)
    {
        builder.ToTable("UsageRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.EstimatedCostUsd).HasPrecision(18, 4);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.OrganizationId, x.MetricKind, x.PeriodStart });
    }
}

/// <summary>EF configuration for invoices.</summary>
public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(10);
        builder.Property(x => x.SubtotalUsd).HasPrecision(18, 4);
        builder.Property(x => x.TaxUsd).HasPrecision(18, 4);
        builder.Property(x => x.TotalUsd).HasPrecision(18, 4);
        builder.Property(x => x.LineItemsJson).IsRequired().HasColumnType("longtext");
        builder.Property(x => x.ExternalInvoiceId).HasMaxLength(191);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.OrganizationId, x.InvoiceNumber }).IsUnique();
    }
}

/// <summary>EF configuration for product licenses.</summary>
public class ProductLicenseConfiguration : IEntityTypeConfiguration<ProductLicense>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductLicense> builder)
    {
        builder.ToTable("ProductLicenses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LicenseKeyHash).IsRequired().HasMaxLength(128);
        builder.Property(x => x.LicenseKeyPrefix).IsRequired().HasMaxLength(64);
        builder.Property(x => x.EncryptedPayload).HasColumnType("longtext");
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => x.LicenseKeyHash).IsUnique();
        builder.HasIndex(x => x.OrganizationId);
    }
}

/// <summary>EF configuration for onboarding progress.</summary>
public class OnboardingProgressConfiguration : IEntityTypeConfiguration<OnboardingProgress>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OnboardingProgress> builder)
    {
        builder.ToTable("OnboardingProgress");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CompletedStepsJson).IsRequired().HasMaxLength(4000);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => x.OrganizationId).IsUnique();
    }
}

/// <summary>EF configuration for telemetry preferences.</summary>
public class TelemetryPreferenceConfiguration : IEntityTypeConfiguration<TelemetryPreference>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TelemetryPreference> builder)
    {
        builder.ToTable("TelemetryPreferences");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => x.OrganizationId).IsUnique();
    }
}

/// <summary>EF configuration for backup jobs.</summary>
public class BackupJobConfiguration : IEntityTypeConfiguration<BackupJob>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<BackupJob> builder)
    {
        builder.ToTable("BackupJobs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BackupType).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);
        builder.Property(x => x.StorageLocator).HasMaxLength(1000);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.OrganizationId, x.StartedAt });
    }
}

/// <summary>EF configuration for platform releases.</summary>
public class PlatformReleaseConfiguration : IEntityTypeConfiguration<PlatformRelease>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PlatformRelease> builder)
    {
        builder.ToTable("PlatformReleases");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Version).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Channel).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ReleaseNotes).HasColumnType("longtext");
        builder.Property(x => x.DownloadUrl).HasMaxLength(1000);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.Channel, x.Version }).IsUnique();
    }
}
