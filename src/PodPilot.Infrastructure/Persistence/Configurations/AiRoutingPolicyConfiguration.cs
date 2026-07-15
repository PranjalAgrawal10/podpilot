using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="AiRoutingPolicy"/>.
/// </summary>
public class AiRoutingPolicyConfiguration : IEntityTypeConfiguration<AiRoutingPolicy>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AiRoutingPolicy> builder)
    {
        builder.ToTable("RoutingPolicies");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.ModelName)
            .HasMaxLength(200);

        builder.Property(p => p.FallbackProviderIdsJson)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(p => p.PreferredTaskTypesJson)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.CustomRulesJson)
            .HasMaxLength(8000);

        builder.HasIndex(p => new { p.OrganizationId, p.Name })
            .IsUnique();

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(128);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(128);

        builder.HasOne(p => p.PrimaryProvider)
            .WithMany(p => p.RoutingPolicies)
            .HasForeignKey(p => p.PrimaryProviderId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
