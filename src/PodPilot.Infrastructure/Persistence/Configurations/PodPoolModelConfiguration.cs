using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="PodPoolModel"/>.
/// </summary>
public sealed class PodPoolModelConfiguration : IEntityTypeConfiguration<PodPoolModel>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PodPoolModel> builder)
    {
        builder.ToTable("PodPoolModels");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ModelName).HasMaxLength(200).IsRequired();
        builder.HasIndex(m => new { m.PodPoolId, m.ModelName }).IsUnique();

        builder.HasOne(m => m.PodPool)
            .WithMany(p => p.Models)
            .HasForeignKey(m => m.PodPoolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
