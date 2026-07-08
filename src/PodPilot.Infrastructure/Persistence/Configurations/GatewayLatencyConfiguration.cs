using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="GatewayLatency"/>.
/// </summary>
public sealed class GatewayLatencyConfiguration : IEntityTypeConfiguration<GatewayLatency>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GatewayLatency> builder)
    {
        builder.ToTable("GatewayLatencies");
        builder.HasKey(l => l.GatewayRequestId);
    }
}

/// <summary>
/// EF configuration for <see cref="GatewayError"/>.
/// </summary>
public sealed class GatewayErrorConfiguration : IEntityTypeConfiguration<GatewayError>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GatewayError> builder)
    {
        builder.ToTable("GatewayErrors");
        builder.HasKey(e => e.GatewayRequestId);
        builder.Property(e => e.ErrorCode).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Message).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.InternalDetails).HasMaxLength(4000);
        builder.Property(e => e.ErrorFormat).HasConversion<string>().HasMaxLength(32);
    }
}
