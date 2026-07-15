using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="LatencyHistory"/>.
/// </summary>
public class LatencyHistoryConfiguration : IEntityTypeConfiguration<LatencyHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LatencyHistory> builder)
    {
        builder.ToTable("LatencyHistory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ModelName).HasMaxLength(200);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);

        builder.HasIndex(x => new { x.OrganizationId, x.AiProviderId, x.RecordedAt });

        builder.HasOne(x => x.AiProvider)
            .WithMany()
            .HasForeignKey(x => x.AiProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
