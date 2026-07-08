using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="SchedulerEvent"/>.
/// </summary>
public sealed class SchedulerEventConfiguration : IEntityTypeConfiguration<SchedulerEvent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SchedulerEvent> builder)
    {
        builder.ToTable("SchedulerEvents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EventType).HasConversion<string>().HasMaxLength(32);
        builder.Property(e => e.Message).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.Metadata).HasMaxLength(4000);
        builder.HasIndex(e => new { e.OrganizationId, e.Timestamp });
        builder.HasIndex(e => e.GatewayRequestId);
    }
}
