using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="PodLifecycleLock"/>.
/// </summary>
public sealed class PodLifecycleLockConfiguration : IEntityTypeConfiguration<PodLifecycleLock>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PodLifecycleLock> builder)
    {
        builder.ToTable("PodLifecycleLocks");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.OwnerId).HasMaxLength(100).IsRequired();
        builder.Property(l => l.Operation).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(l => new { l.PodId, l.Operation }).IsUnique();

        builder.HasOne(l => l.Pod)
            .WithMany()
            .HasForeignKey(l => l.PodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
