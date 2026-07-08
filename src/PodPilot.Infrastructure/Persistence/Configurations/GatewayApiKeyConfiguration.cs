using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="GatewayApiKey"/>.
/// </summary>
public sealed class GatewayApiKeyConfiguration : IEntityTypeConfiguration<GatewayApiKey>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GatewayApiKey> builder)
    {
        builder.ToTable("GatewayApiKeys");
        builder.HasKey(k => k.Id);
        builder.Property(k => k.Name).HasMaxLength(200).IsRequired();
        builder.Property(k => k.KeyPrefix).HasMaxLength(32).IsRequired();
        builder.Property(k => k.KeyHash).HasMaxLength(128).IsRequired();
        builder.Property(k => k.KeyType).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(k => new { k.OrganizationId, k.KeyPrefix });
        builder.HasIndex(k => k.KeyHash).IsUnique();

        builder.HasOne(k => k.Organization)
            .WithMany()
            .HasForeignKey(k => k.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
