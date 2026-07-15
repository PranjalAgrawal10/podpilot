using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="RoutingEvent"/>.
/// </summary>
public class RoutingEventConfiguration : IEntityTypeConfiguration<RoutingEvent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RoutingEvent> builder)
    {
        builder.ToTable("RoutingEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SelectedModelName).HasMaxLength(200);
        builder.Property(x => x.DecisionReason).HasMaxLength(1000);
        builder.Property(x => x.EstimatedCostUsd).HasPrecision(18, 8);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);

        builder.HasIndex(x => new { x.OrganizationId, x.DecidedAt });

        builder.HasOne(x => x.RoutingPolicy)
            .WithMany()
            .HasForeignKey(x => x.RoutingPolicyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.SelectedProvider)
            .WithMany()
            .HasForeignKey(x => x.SelectedProviderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
