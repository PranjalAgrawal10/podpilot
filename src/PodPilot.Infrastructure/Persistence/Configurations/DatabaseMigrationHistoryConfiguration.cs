using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="DatabaseMigrationHistory"/>.
/// </summary>
public sealed class DatabaseMigrationHistoryConfiguration : IEntityTypeConfiguration<DatabaseMigrationHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DatabaseMigrationHistory> builder)
    {
        builder.ToTable("DatabaseMigrationHistory");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.MigrationId).HasMaxLength(200).IsRequired();
        builder.Property(h => h.ProductVersion).HasMaxLength(50).IsRequired();
        builder.HasIndex(h => h.MigrationId).IsUnique();
        builder.HasIndex(h => h.AppliedAt);
    }
}
