using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="ScalingEvent"/>.
/// </summary>
public sealed class ScalingEventConfiguration : IEntityTypeConfiguration<ScalingEvent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ScalingEvent> builder)
    {
        builder.ToTable("ScalingEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Direction).HasConversion<string>().HasMaxLength(16);
        builder.Property(e => e.TriggerType).HasConversion<string>().HasMaxLength(16);
        builder.Property(e => e.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(e => new { e.OrganizationId, e.OccurredAt });
        builder.HasIndex(e => new { e.PodPoolId, e.OccurredAt });
    }
}
