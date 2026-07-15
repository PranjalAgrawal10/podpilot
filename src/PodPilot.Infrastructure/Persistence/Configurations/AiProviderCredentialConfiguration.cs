using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="AiProviderCredential"/>.
/// </summary>
public class AiProviderCredentialConfiguration : IEntityTypeConfiguration<AiProviderCredential>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AiProviderCredential> builder)
    {
        builder.ToTable("AiProviderCredentials");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.EncryptedApiKey)
            .IsRequired()
            .HasMaxLength(4096);

        builder.HasIndex(c => c.AiProviderId)
            .IsUnique();

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(128);

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(128);

        builder.HasOne(c => c.AiProvider)
            .WithOne(p => p.Credential)
            .HasForeignKey<AiProviderCredential>(c => c.AiProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
