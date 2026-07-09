using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Application.Common;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="ScalingPolicy"/>.
/// </summary>
public sealed class ScalingPolicyConfiguration : IEntityTypeConfiguration<ScalingPolicy>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ScalingPolicy> builder)
    {
        builder.ToTable("ScalingPolicies");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(ApplicationConstants.ProviderNameMaxLength).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(128);
        builder.Property(p => p.UpdatedBy).HasMaxLength(128);

        builder.HasIndex(p => new { p.OrganizationId, p.Name }).IsUnique();

        builder.HasOne(p => p.Organization)
            .WithMany()
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
