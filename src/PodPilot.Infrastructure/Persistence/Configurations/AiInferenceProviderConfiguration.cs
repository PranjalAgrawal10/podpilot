using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="AiInferenceProvider"/>.
/// </summary>
public class AiInferenceProviderConfiguration : IEntityTypeConfiguration<AiInferenceProvider>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AiInferenceProvider> builder)
    {
        builder.ToTable("AiProviders");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.BaseUrl)
            .HasMaxLength(500);

        builder.Property(p => p.DeploymentName)
            .HasMaxLength(200);

        builder.Property(p => p.ApiVersion)
            .HasMaxLength(50);

        builder.Property(p => p.ProviderKind)
            .IsRequired();

        builder.HasIndex(p => new { p.OrganizationId, p.Name })
            .IsUnique();

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(128);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(128);

        builder.HasOne(p => p.Organization)
            .WithMany(o => o.AiInferenceProviders)
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
