using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="ModelHealthHistory"/>.
/// </summary>
public sealed class ModelHealthHistoryConfiguration : IEntityTypeConfiguration<ModelHealthHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ModelHealthHistory> builder)
    {
        builder.ToTable("ModelHealthHistory");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.ErrorMessage).HasMaxLength(2000);
        builder.HasIndex(h => new { h.ModelId, h.LastChecked });

        builder.HasOne(h => h.Model)
            .WithMany(m => m.HealthHistory)
            .HasForeignKey(h => h.ModelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
