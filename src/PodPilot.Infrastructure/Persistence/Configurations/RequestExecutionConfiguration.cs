using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for <see cref="RequestExecution"/>.
/// </summary>
public sealed class RequestExecutionConfiguration : IEntityTypeConfiguration<RequestExecution>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RequestExecution> builder)
    {
        builder.ToTable("RequestExecutions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);
        builder.HasIndex(e => new { e.GatewayRequestId, e.AttemptNumber });
        builder.HasIndex(e => e.GpuPodId);
    }
}
