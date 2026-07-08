using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="PodLifecycleEvent"/>.
/// </summary>
public sealed class PodLifecycleEventConfiguration : IEntityTypeConfiguration<PodLifecycleEvent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PodLifecycleEvent> builder)
    {
        builder.ToTable("PodLifecycleEvents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Source).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Message).HasMaxLength(1000);
        builder.Property(e => e.Metadata).HasMaxLength(2000);
        builder.Property(e => e.EventType).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(e => new { e.PodId, e.Timestamp });

        builder.HasOne(e => e.Pod)
            .WithMany(p => p.LifecycleEvents)
            .HasForeignKey(e => e.PodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
