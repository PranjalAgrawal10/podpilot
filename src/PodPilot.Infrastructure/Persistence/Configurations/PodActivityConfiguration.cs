using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="PodActivity"/>.
/// </summary>
public sealed class PodActivityConfiguration : IEntityTypeConfiguration<PodActivity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PodActivity> builder)
    {
        builder.ToTable("PodActivities");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Source).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Metadata).HasMaxLength(2000);
        builder.Property(a => a.ActivityType).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(a => new { a.PodId, a.Timestamp });

        builder.HasOne(a => a.Pod)
            .WithMany(p => p.Activities)
            .HasForeignKey(a => a.PodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
