using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Application.Common;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="PodPool"/>.
/// </summary>
public sealed class PodPoolConfiguration : IEntityTypeConfiguration<PodPool>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PodPool> builder)
    {
        builder.ToTable("PodPools");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(ApplicationConstants.ProviderNameMaxLength).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(ApplicationConstants.ProviderDescriptionMaxLength);
        builder.Property(p => p.PoolType).HasConversion<string>().HasMaxLength(32);
        builder.Property(p => p.GpuId).HasMaxLength(200);
        builder.Property(p => p.GpuType).HasConversion<string>().HasMaxLength(32);
        builder.Property(p => p.Region).HasMaxLength(ApplicationConstants.PodRegionMaxLength);
        builder.Property(p => p.TemplateId).HasMaxLength(100);
        builder.Property(p => p.ImageName).HasMaxLength(ApplicationConstants.PodImageNameMaxLength);
        builder.Property(p => p.CreatedBy).HasMaxLength(128);
        builder.Property(p => p.UpdatedBy).HasMaxLength(128);

        builder.HasIndex(p => new { p.OrganizationId, p.Name }).IsUnique();
        builder.HasIndex(p => new { p.OrganizationId, p.IsDefault });
        builder.HasIndex(p => new { p.OrganizationId, p.IsActive });

        builder.HasOne(p => p.Organization)
            .WithMany()
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.ScalingPolicy)
            .WithMany()
            .HasForeignKey(p => p.ScalingPolicyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
