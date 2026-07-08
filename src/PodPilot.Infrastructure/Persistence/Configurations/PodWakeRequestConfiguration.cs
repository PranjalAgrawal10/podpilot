using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="PodWakeRequest"/>.
/// </summary>
public sealed class PodWakeRequestConfiguration : IEntityTypeConfiguration<PodWakeRequest>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PodWakeRequest> builder)
    {
        builder.ToTable("PodWakeRequests");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Source).HasMaxLength(100).IsRequired();
        builder.Property(r => r.ErrorMessage).HasMaxLength(1000);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(r => new { r.PodId, r.Status });
        builder.HasIndex(r => r.RequestedAt);

        builder.HasOne(r => r.Pod)
            .WithMany()
            .HasForeignKey(r => r.PodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
