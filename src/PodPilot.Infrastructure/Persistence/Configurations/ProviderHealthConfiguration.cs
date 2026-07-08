using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="ProviderHealth"/>.
/// </summary>
public class ProviderHealthConfiguration : IEntityTypeConfiguration<ProviderHealth>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProviderHealth> builder)
    {
        builder.ToTable("ProviderHealth");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(h => h.ComputeProviderId)
            .IsUnique();

        builder.HasOne(h => h.ComputeProvider)
            .WithOne(p => p.Health)
            .HasForeignKey<ProviderHealth>(h => h.ComputeProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
