using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="PodHealthMetric"/>.
/// </summary>
public sealed class PodHealthMetricConfiguration : IEntityTypeConfiguration<PodHealthMetric>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PodHealthMetric> builder)
    {
        builder.ToTable("PodHealthMetrics");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.State).HasConversion<string>().HasMaxLength(32);
        builder.Property(m => m.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(m => new { m.OrganizationId, m.GpuPodId, m.RecordedAt });
        builder.HasIndex(m => new { m.GpuPodId, m.RecordedAt });
    }
}
