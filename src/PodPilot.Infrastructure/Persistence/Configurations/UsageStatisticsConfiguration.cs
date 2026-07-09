using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="UsageStatistics"/>.
/// </summary>
public sealed class UsageStatisticsConfiguration : IEntityTypeConfiguration<UsageStatistics>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UsageStatistics> builder)
    {
        builder.ToTable("UsageStatistics");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Period).HasConversion<string>().HasMaxLength(32);
        builder.Property(u => u.ModelName).HasMaxLength(256);

        builder.HasIndex(u => new { u.OrganizationId, u.RecordedAt });
        builder.HasIndex(u => new { u.OrganizationId, u.Period, u.RecordedAt });
    }
}
