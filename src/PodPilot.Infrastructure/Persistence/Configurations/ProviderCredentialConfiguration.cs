using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="ProviderCredential"/>.
/// </summary>
public class ProviderCredentialConfiguration : IEntityTypeConfiguration<ProviderCredential>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProviderCredential> builder)
    {
        builder.ToTable("ProviderCredentials");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.EncryptedApiKey)
            .IsRequired()
            .HasMaxLength(4096);

        builder.HasIndex(c => c.ComputeProviderId)
            .IsUnique();

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(128);

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(128);

        builder.HasOne(c => c.ComputeProvider)
            .WithOne(p => p.Credential)
            .HasForeignKey<ProviderCredential>(c => c.ComputeProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
