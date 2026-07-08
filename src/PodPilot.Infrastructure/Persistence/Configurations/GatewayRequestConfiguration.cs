using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="GatewayRequest"/>.
/// </summary>
public sealed class GatewayRequestConfiguration : IEntityTypeConfiguration<GatewayRequest>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GatewayRequest> builder)
    {
        builder.ToTable("GatewayRequests");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.HttpMethod).HasMaxLength(16).IsRequired();
        builder.Property(r => r.Path).HasMaxLength(500).IsRequired();
        builder.Property(r => r.Model).HasMaxLength(200);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(r => r.CorrelationId).HasMaxLength(64);
        builder.Property(r => r.UpstreamBaseUrl).HasMaxLength(500);
        builder.HasIndex(r => new { r.OrganizationId, r.StartedAt });
        builder.HasIndex(r => r.GpuPodId);

        builder.HasOne<GatewayApiKey>()
            .WithMany()
            .HasForeignKey(r => r.ApiKeyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<GpuPod>()
            .WithMany()
            .HasForeignKey(r => r.GpuPodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Latency)
            .WithOne(l => l.Request)
            .HasForeignKey<GatewayLatency>(l => l.GatewayRequestId);

        builder.HasOne(r => r.Error)
            .WithOne(e => e.Request)
            .HasForeignKey<GatewayError>(e => e.GatewayRequestId);
    }
}
