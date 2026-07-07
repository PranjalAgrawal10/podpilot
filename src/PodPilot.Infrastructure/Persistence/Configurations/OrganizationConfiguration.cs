using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="Organization"/>.
/// </summary>
public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(o => o.Slug)
            .IsUnique();

        builder.Property(o => o.Description)
            .HasMaxLength(1000);

        builder.Property(o => o.CreatedBy)
            .HasMaxLength(128);

        builder.Property(o => o.UpdatedBy)
            .HasMaxLength(128);
    }
}
