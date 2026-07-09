using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="PodPoolMember"/>.
/// </summary>
public sealed class PodPoolMemberConfiguration : IEntityTypeConfiguration<PodPoolMember>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PodPoolMember> builder)
    {
        builder.ToTable("PodPoolMembers");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.State).HasConversion<string>().HasMaxLength(32);
        builder.Property(m => m.AffinityTag).HasMaxLength(64);

        builder.HasIndex(m => new { m.PodPoolId, m.GpuPodId }).IsUnique();
        builder.HasIndex(m => new { m.PodPoolId, m.State });

        builder.HasOne(m => m.PodPool)
            .WithMany(p => p.Members)
            .HasForeignKey(m => m.PodPoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.GpuPod)
            .WithMany()
            .HasForeignKey(m => m.GpuPodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
