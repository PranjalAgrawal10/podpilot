using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;
using PodPilot.Infrastructure.Identity;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="RefreshToken"/>.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.HasIndex(t => t.Token)
            .IsUnique();

        builder.Property(t => t.ReplacedByToken)
            .HasMaxLength(512);

        builder.Property(t => t.ReasonRevoked)
            .HasMaxLength(256);

        builder.Ignore(t => t.User);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
