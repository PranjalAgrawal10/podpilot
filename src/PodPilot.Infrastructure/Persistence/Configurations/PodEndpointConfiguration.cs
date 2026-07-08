using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="PodEndpoint"/>.
/// </summary>
public sealed class PodEndpointConfiguration : IEntityTypeConfiguration<PodEndpoint>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PodEndpoint> builder)
    {
        builder.ToTable("PodEndpoints");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Protocol).HasMaxLength(16).IsRequired();
        builder.Property(e => e.Url).HasMaxLength(500);

        builder.HasIndex(e => new { e.GpuPodId, e.Port, e.Protocol }).IsUnique();

        builder.HasOne(e => e.GpuPod)
            .WithMany(p => p.Endpoints)
            .HasForeignKey(e => e.GpuPodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
