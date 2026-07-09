using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="AlertHistory"/>.
/// </summary>
public sealed class AlertHistoryConfiguration : IEntityTypeConfiguration<AlertHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AlertHistory> builder)
    {
        builder.ToTable("AlertHistory");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AlertType).HasConversion<string>().HasMaxLength(64);
        builder.Property(a => a.Severity).HasConversion<string>().HasMaxLength(32);
        builder.Property(a => a.Title).HasMaxLength(256);
        builder.Property(a => a.Message).HasMaxLength(2000);
        builder.Property(a => a.ModelName).HasMaxLength(256);

        builder.HasIndex(a => new { a.OrganizationId, a.RaisedAt });
        builder.HasIndex(a => new { a.OrganizationId, a.IsActive, a.RaisedAt });
    }
}
