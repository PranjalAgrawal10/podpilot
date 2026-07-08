using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="ProviderRegion"/>.
/// </summary>
public class ProviderRegionConfiguration : IEntityTypeConfiguration<ProviderRegion>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProviderRegion> builder)
    {
        builder.ToTable("ProviderRegions");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RegionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(r => new { r.ComputeProviderId, r.RegionId })
            .IsUnique();

        builder.HasOne(r => r.ComputeProvider)
            .WithMany(p => p.Regions)
            .HasForeignKey(r => r.ComputeProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
