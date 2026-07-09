using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="MetricsSnapshot"/>.
/// </summary>
public sealed class MetricsSnapshotConfiguration : IEntityTypeConfiguration<MetricsSnapshot>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MetricsSnapshot> builder)
    {
        builder.ToTable("MetricsSnapshots");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ModelName).HasMaxLength(256);

        builder.HasIndex(m => new { m.OrganizationId, m.RecordedAt });
        builder.HasIndex(m => new { m.OrganizationId, m.GpuPodId, m.RecordedAt });
        builder.HasIndex(m => new { m.OrganizationId, m.ProviderId, m.RecordedAt });
    }
}
