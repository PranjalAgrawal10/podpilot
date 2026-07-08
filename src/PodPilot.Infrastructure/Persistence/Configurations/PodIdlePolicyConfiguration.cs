using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="PodIdlePolicy"/>.
/// </summary>
public sealed class PodIdlePolicyConfiguration : IEntityTypeConfiguration<PodIdlePolicy>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PodIdlePolicy> builder)
    {
        builder.ToTable("PodIdlePolicies");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.PodId).IsUnique();

        builder.HasOne(p => p.Pod)
            .WithOne(pod => pod.IdlePolicy)
            .HasForeignKey<PodIdlePolicy>(p => p.PodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
