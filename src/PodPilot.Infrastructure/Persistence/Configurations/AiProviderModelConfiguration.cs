using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="AiProviderModel"/>.
/// </summary>
public class AiProviderModelConfiguration : IEntityTypeConfiguration<AiProviderModel>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AiProviderModel> builder)
    {
        builder.ToTable("ProviderModels");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ModelName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.DisplayName)
            .HasMaxLength(200);

        builder.Property(m => m.Parameters)
            .HasMaxLength(100);

        builder.Property(m => m.InputCostPerMillionTokens)
            .HasPrecision(18, 6);

        builder.Property(m => m.OutputCostPerMillionTokens)
            .HasPrecision(18, 6);

        builder.HasIndex(m => new { m.AiProviderId, m.ModelName })
            .IsUnique();

        builder.HasIndex(m => new { m.OrganizationId, m.ModelName });

        builder.Property(m => m.CreatedBy)
            .HasMaxLength(128);

        builder.Property(m => m.UpdatedBy)
            .HasMaxLength(128);

        builder.HasOne(m => m.AiProvider)
            .WithMany(p => p.Models)
            .HasForeignKey(m => m.AiProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
