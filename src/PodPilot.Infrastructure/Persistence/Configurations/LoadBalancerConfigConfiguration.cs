using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="LoadBalancerConfig"/>.
/// </summary>
public sealed class LoadBalancerConfigConfiguration : IEntityTypeConfiguration<LoadBalancerConfig>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LoadBalancerConfig> builder)
    {
        builder.ToTable("LoadBalancerConfigs");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Strategy).HasConversion<string>().HasMaxLength(32);
        builder.Property(c => c.CreatedBy).HasMaxLength(128);
        builder.Property(c => c.UpdatedBy).HasMaxLength(128);

        builder.HasIndex(c => c.OrganizationId).IsUnique();

        builder.HasOne(c => c.Organization)
            .WithMany()
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
