using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="ComputeProvider"/>.
/// </summary>
public class ComputeProviderConfiguration : IEntityTypeConfiguration<ComputeProvider>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ComputeProvider> builder)
    {
        builder.ToTable("ComputeProviders");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.DefaultRegion)
            .HasMaxLength(100);

        builder.Property(p => p.ProviderType)
            .IsRequired();

        builder.HasIndex(p => new { p.OrganizationId, p.Name })
            .IsUnique();

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(128);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(128);

        builder.HasOne(p => p.Organization)
            .WithMany(o => o.ComputeProviders)
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
