using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="CostSnapshot"/>.
/// </summary>
public sealed class CostSnapshotConfiguration : IEntityTypeConfiguration<CostSnapshot>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CostSnapshot> builder)
    {
        builder.ToTable("CostSnapshots");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Period).HasConversion<string>().HasMaxLength(32);
        builder.Property(c => c.ModelName).HasMaxLength(256);
        builder.Property(c => c.HourlyCost).HasPrecision(18, 4);
        builder.Property(c => c.DailyCost).HasPrecision(18, 4);
        builder.Property(c => c.WeeklyCost).HasPrecision(18, 4);
        builder.Property(c => c.MonthlyCost).HasPrecision(18, 4);
        builder.Property(c => c.ProjectedMonthlyCost).HasPrecision(18, 4);
        builder.Property(c => c.AutoShutdownSavings).HasPrecision(18, 4);

        builder.HasIndex(c => new { c.OrganizationId, c.RecordedAt });
        builder.HasIndex(c => new { c.OrganizationId, c.Period, c.RecordedAt });
    }
}
