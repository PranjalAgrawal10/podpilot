using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="PodStatusHistory"/>.
/// </summary>
public sealed class PodStatusHistoryConfiguration : IEntityTypeConfiguration<PodStatusHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PodStatusHistory> builder)
    {
        builder.ToTable("PodStatusHistory");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(h => h.Message).HasMaxLength(1000);

        builder.HasIndex(h => new { h.GpuPodId, h.RecordedAt });

        builder.HasOne(h => h.GpuPod)
            .WithMany(p => p.StatusHistory)
            .HasForeignKey(h => h.GpuPodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
