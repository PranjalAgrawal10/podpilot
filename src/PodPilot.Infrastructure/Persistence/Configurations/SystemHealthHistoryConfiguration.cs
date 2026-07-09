using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="SystemHealthHistory"/>.
/// </summary>
public sealed class SystemHealthHistoryConfiguration : IEntityTypeConfiguration<SystemHealthHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SystemHealthHistory> builder)
    {
        builder.ToTable("SystemHealthHistory");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Component).HasConversion<string>().HasMaxLength(32);
        builder.Property(h => h.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(h => h.Message).HasMaxLength(2000);
        builder.Property(h => h.Metadata).HasMaxLength(4000);

        builder.HasIndex(h => new { h.OrganizationId, h.RecordedAt });
        builder.HasIndex(h => new { h.OrganizationId, h.Component, h.RecordedAt });
    }
}
