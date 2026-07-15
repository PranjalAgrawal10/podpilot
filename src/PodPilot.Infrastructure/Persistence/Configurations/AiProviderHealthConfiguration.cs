using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="AiProviderHealth"/>.
/// </summary>
public class AiProviderHealthConfiguration : IEntityTypeConfiguration<AiProviderHealth>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AiProviderHealth> builder)
    {
        builder.ToTable("AiProviderHealth");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(h => h.AiProviderId)
            .IsUnique();

        builder.HasOne(h => h.AiProvider)
            .WithOne(p => p.Health)
            .HasForeignKey<AiProviderHealth>(h => h.AiProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
