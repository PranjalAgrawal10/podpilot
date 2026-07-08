using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="ProviderHealthHistory"/>.
/// </summary>
public class ProviderHealthHistoryConfiguration : IEntityTypeConfiguration<ProviderHealthHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProviderHealthHistory> builder)
    {
        builder.ToTable("ProviderHealthHistory");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(h => new { h.ComputeProviderId, h.CheckedAt });

        builder.HasOne(h => h.ComputeProvider)
            .WithMany(p => p.HealthHistory)
            .HasForeignKey(h => h.ComputeProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
