using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="GatewayRoute"/>.
/// </summary>
public sealed class GatewayRouteConfiguration : IEntityTypeConfiguration<GatewayRoute>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GatewayRoute> builder)
    {
        builder.ToTable("GatewayRoutes");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.ModelName).HasMaxLength(200).IsRequired();
        builder.HasIndex(r => new { r.OrganizationId, r.ModelName }).IsUnique();
        builder.HasIndex(r => new { r.OrganizationId, r.IsDefault });

        builder.HasOne(r => r.Organization)
            .WithMany()
            .HasForeignKey(r => r.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Pod)
            .WithMany()
            .HasForeignKey(r => r.GpuPodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
