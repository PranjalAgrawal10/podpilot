using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Application.Common;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="GpuPod"/>.
/// </summary>
public sealed class GpuPodConfiguration : IEntityTypeConfiguration<GpuPod>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GpuPod> builder)
    {
        builder.ToTable("GpuPods");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(ApplicationConstants.PodNameMaxLength).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(ApplicationConstants.PodDescriptionMaxLength);
        builder.Property(p => p.ProviderPodId).HasMaxLength(ApplicationConstants.ProviderPodIdMaxLength);
        builder.Property(p => p.GpuId).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Region).HasMaxLength(ApplicationConstants.PodRegionMaxLength).IsRequired();
        builder.Property(p => p.TemplateId).HasMaxLength(100);
        builder.Property(p => p.ImageName).HasMaxLength(ApplicationConstants.PodImageNameMaxLength).IsRequired();
        builder.Property(p => p.PublicIp).HasMaxLength(64);
        builder.Property(p => p.Endpoint).HasMaxLength(500);
        builder.Property(p => p.HourlyCost).HasPrecision(18, 4);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(p => p.LastActivityAt);

        builder.HasIndex(p => new { p.OrganizationId, p.Name });
        builder.HasIndex(p => p.ProviderId);
        builder.HasIndex(p => p.Status);

        builder.HasOne(p => p.Organization)
            .WithMany()
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Provider)
            .WithMany()
            .HasForeignKey(p => p.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
