using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="Invitation"/>.
/// </summary>
public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("Invitations");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.Token)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(i => i.Token)
            .IsUnique();

        builder.HasIndex(i => new { i.OrganizationId, i.Email, i.Status });

        builder.Property(i => i.CreatedBy)
            .HasMaxLength(128);

        builder.Property(i => i.UpdatedBy)
            .HasMaxLength(128);

        builder.HasOne(i => i.Organization)
            .WithMany(o => o.Invitations)
            .HasForeignKey(i => i.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
