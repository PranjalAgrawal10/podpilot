using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="RequestQueueEntry"/>.
/// </summary>
public sealed class RequestQueueEntryConfiguration : IEntityTypeConfiguration<RequestQueueEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RequestQueueEntry> builder)
    {
        builder.ToTable("RequestQueue");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.QueueName).HasMaxLength(64).IsRequired();
        builder.Property(e => e.Priority).HasConversion<string>().HasMaxLength(16);
        builder.Property(e => e.ClientRequestId).HasMaxLength(64);
        builder.HasIndex(e => new { e.OrganizationId, e.IsActive, e.EnqueuedAt });
        builder.HasIndex(e => e.GatewayRequestId).IsUnique();
    }
}
