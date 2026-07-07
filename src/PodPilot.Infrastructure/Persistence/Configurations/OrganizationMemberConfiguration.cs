using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;
using PodPilot.Infrastructure.Identity;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="OrganizationMember"/>.
/// </summary>
public class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.ToTable("OrganizationMembers");

        builder.HasKey(m => m.Id);

        builder.HasIndex(m => new { m.OrganizationId, m.UserId })
            .IsUnique();

        builder.Property(m => m.CreatedBy)
            .HasMaxLength(128);

        builder.Property(m => m.UpdatedBy)
            .HasMaxLength(128);

        builder.HasOne(m => m.Organization)
            .WithMany(o => o.Members)
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(m => m.User);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
