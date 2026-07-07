using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="AuditLog"/>.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(a => a.Details)
            .HasMaxLength(2000);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(64);

        builder.Property(a => a.CorrelationId)
            .HasMaxLength(64);

        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.UserId);
    }
}
