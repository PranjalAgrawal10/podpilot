using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="AiModel"/>.
/// </summary>
public sealed class AiModelConfiguration : IEntityTypeConfiguration<AiModel>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AiModel> builder)
    {
        builder.ToTable("AiModels");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Tag).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Family).HasMaxLength(100);
        builder.Property(m => m.Quantization).HasMaxLength(50);
        builder.Property(m => m.Parameters).HasMaxLength(50);
        builder.Property(m => m.License).HasMaxLength(200);
        builder.HasIndex(m => new { m.OrganizationId, m.PodId, m.Name, m.Tag }).IsUnique();
        builder.HasIndex(m => new { m.OrganizationId, m.PodId, m.IsDefault });
        builder.HasIndex(m => m.Status);

        builder.HasOne(m => m.Organization)
            .WithMany()
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Pod)
            .WithMany()
            .HasForeignKey(m => m.PodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
