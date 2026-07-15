using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="AiFailoverEvent"/>.
/// </summary>
public class AiFailoverEventConfiguration : IEntityTypeConfiguration<AiFailoverEvent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AiFailoverEvent> builder)
    {
        builder.ToTable("AiFailoverEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ModelName)
            .HasMaxLength(200);

        builder.Property(e => e.Reason)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasIndex(e => new { e.OrganizationId, e.OccurredAt });
    }
}
