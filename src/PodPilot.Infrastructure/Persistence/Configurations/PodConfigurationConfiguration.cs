using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Application.Common;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="PodConfiguration"/>.
/// </summary>
public sealed class PodConfigurationConfiguration : IEntityTypeConfiguration<PodConfiguration>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PodConfiguration> builder)
    {
        builder.ToTable("PodConfigurations");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.TemplateId).HasMaxLength(100);
        builder.Property(c => c.TemplateName).HasMaxLength(200);
        builder.Property(c => c.ImageName).HasMaxLength(ApplicationConstants.PodImageNameMaxLength).IsRequired();
        builder.Property(c => c.VolumeMountPath).HasMaxLength(200).IsRequired();
        builder.Property(c => c.EnvironmentVariablesJson).HasMaxLength(8000);
        builder.Property(c => c.PortsJson).HasMaxLength(2000);

        builder.HasIndex(c => c.GpuPodId).IsUnique();

        builder.HasOne(c => c.GpuPod)
            .WithOne(p => p.Configuration)
            .HasForeignKey<PodConfiguration>(c => c.GpuPodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
