using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="CapacitySnapshot"/>.
/// </summary>
public sealed class CapacitySnapshotConfiguration : IEntityTypeConfiguration<CapacitySnapshot>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CapacitySnapshot> builder)
    {
        builder.ToTable("CapacitySnapshots");
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => new { s.OrganizationId, s.RecordedAt });
        builder.HasIndex(s => new { s.OrganizationId, s.PodPoolId, s.RecordedAt });
    }
}
