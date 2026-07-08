using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="ProviderGpu"/>.
/// </summary>
public class ProviderGpuConfiguration : IEntityTypeConfiguration<ProviderGpu>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProviderGpu> builder)
    {
        builder.ToTable("ProviderGpus");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.GpuId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(g => new { g.ComputeProviderId, g.GpuId })
            .IsUnique();

        builder.HasOne(g => g.ComputeProvider)
            .WithMany(p => p.Gpus)
            .HasForeignKey(g => g.ComputeProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
