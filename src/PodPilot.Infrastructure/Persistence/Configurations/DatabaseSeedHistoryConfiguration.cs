using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="DatabaseSeedHistory"/>.
/// </summary>
public sealed class DatabaseSeedHistoryConfiguration : IEntityTypeConfiguration<DatabaseSeedHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DatabaseSeedHistory> builder)
    {
        builder.ToTable("DatabaseSeedHistory");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.SeederName).HasMaxLength(200).IsRequired();
        builder.Property(h => h.Version).HasMaxLength(100).IsRequired();
        builder.Property(h => h.Details).HasMaxLength(4000);
        builder.HasIndex(h => new { h.SeederName, h.Version, h.AppliedAt });
        builder.HasIndex(h => h.AppliedAt);
    }
}
