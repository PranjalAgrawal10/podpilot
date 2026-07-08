using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="ModelDownload"/>.
/// </summary>
public sealed class ModelDownloadConfiguration : IEntityTypeConfiguration<ModelDownload>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ModelDownload> builder)
    {
        builder.ToTable("ModelDownloads");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.ErrorMessage).HasMaxLength(2000);
        builder.HasIndex(d => new { d.ModelId, d.Status });

        builder.HasOne(d => d.Model)
            .WithMany(m => m.Downloads)
            .HasForeignKey(d => d.ModelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
