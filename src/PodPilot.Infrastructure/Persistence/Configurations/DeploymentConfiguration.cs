using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for one-click AI deployment entities.
/// </summary>
public sealed class AiDeploymentConfiguration : IEntityTypeConfiguration<AiDeployment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AiDeployment> builder)
    {
        builder.ToTable("AiDeployments");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name).HasMaxLength(100).IsRequired();
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(d => d.CloudProvider).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(d => d.Region).HasMaxLength(64).IsRequired();
        builder.Property(d => d.GpuCode).HasMaxLength(64).IsRequired();
        builder.Property(d => d.ProviderGpuId).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Runtime).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(d => d.CudaVersion).HasMaxLength(16).IsRequired();
        builder.Property(d => d.ImageName).HasMaxLength(500);
        builder.Property(d => d.StatusMessage).HasMaxLength(1000);
        builder.Property(d => d.ErrorMessage).HasMaxLength(2000);
        builder.Property(d => d.EstimatedHourlyCostUsd).HasPrecision(18, 4);
        builder.Property(d => d.EnvironmentVariablesJson).HasColumnType("longtext");

        builder.HasIndex(d => d.OrganizationId);
        builder.HasIndex(d => new { d.OrganizationId, d.Status });
        builder.HasIndex(d => d.ProviderId);
        builder.HasIndex(d => d.GpuPodId);

        builder.HasOne(d => d.Organization)
            .WithMany()
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Provider)
            .WithMany()
            .HasForeignKey(d => d.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Template)
            .WithMany()
            .HasForeignKey(d => d.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.GpuPod)
            .WithMany()
            .HasForeignKey(d => d.GpuPodId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(d => d.Models)
            .WithOne(m => m.Deployment)
            .HasForeignKey(m => m.DeploymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Logs)
            .WithOne(l => l.Deployment)
            .HasForeignKey(l => l.DeploymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Health)
            .WithOne(h => h.Deployment)
            .HasForeignKey<DeploymentHealth>(h => h.DeploymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.History)
            .WithOne(h => h.Deployment)
            .HasForeignKey(h => h.DeploymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF configuration for <see cref="DeploymentTemplate"/>.
/// </summary>
public sealed class DeploymentTemplateConfiguration : IEntityTypeConfiguration<DeploymentTemplate>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DeploymentTemplate> builder)
    {
        builder.ToTable("DeploymentTemplates");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Code).HasMaxLength(64).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Kind).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(1000);
        builder.Property(t => t.Runtime).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(t => t.ContainerImage).HasMaxLength(500).IsRequired();
        builder.Property(t => t.StartupCommand).HasMaxLength(1000);
        builder.Property(t => t.EnvironmentVariablesJson).HasColumnType("longtext");
        builder.Property(t => t.HealthCheckPath).HasMaxLength(200).IsRequired();
        builder.Property(t => t.RecommendedGpuCode).HasMaxLength(64).IsRequired();
        builder.Property(t => t.DefaultModelCodesJson).HasMaxLength(2000).IsRequired();

        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasIndex(t => t.Kind);
    }
}

/// <summary>
/// EF configuration for <see cref="DeploymentModel"/>.
/// </summary>
public sealed class DeploymentModelConfiguration : IEntityTypeConfiguration<DeploymentModel>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DeploymentModel> builder)
    {
        builder.ToTable("DeploymentModels");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ModelReference).HasMaxLength(200).IsRequired();
        builder.Property(m => m.DownloadStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(m => m.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(m => m.DeploymentId);
        builder.HasIndex(m => m.ModelCatalogId);

        builder.HasOne(m => m.ModelCatalog)
            .WithMany()
            .HasForeignKey(m => m.ModelCatalogId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>
/// EF configuration for <see cref="DeploymentLog"/>.
/// </summary>
public sealed class DeploymentLogConfiguration : IEntityTypeConfiguration<DeploymentLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DeploymentLog> builder)
    {
        builder.ToTable("DeploymentLogs");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Level).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(l => l.Stage).HasMaxLength(64).IsRequired();
        builder.Property(l => l.Message).HasMaxLength(4000).IsRequired();

        builder.HasIndex(l => new { l.DeploymentId, l.TimestampUtc });
    }
}

/// <summary>
/// EF configuration for <see cref="RuntimeVersion"/>.
/// </summary>
public sealed class RuntimeVersionConfiguration : IEntityTypeConfiguration<RuntimeVersion>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RuntimeVersion> builder)
    {
        builder.ToTable("RuntimeVersions");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Runtime).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(r => r.Version).HasMaxLength(64).IsRequired();
        builder.Property(r => r.CudaVersion).HasMaxLength(16).IsRequired();
        builder.Property(r => r.ContainerImage).HasMaxLength(500).IsRequired();
        builder.Property(r => r.HealthPath).HasMaxLength(200).IsRequired();

        builder.HasIndex(r => new { r.Runtime, r.Version }).IsUnique();
    }
}

/// <summary>
/// EF configuration for <see cref="GpuCatalogEntry"/>.
/// </summary>
public sealed class GpuCatalogEntryConfiguration : IEntityTypeConfiguration<GpuCatalogEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GpuCatalogEntry> builder)
    {
        builder.ToTable("GpuCatalogEntries");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Code).HasMaxLength(64).IsRequired();
        builder.Property(g => g.Name).HasMaxLength(200).IsRequired();
        builder.Property(g => g.GpuType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(g => g.CudaCapability).HasMaxLength(32).IsRequired();
        builder.Property(g => g.EstimatedHourlyCostUsd).HasPrecision(18, 4);
        builder.Property(g => g.ProviderAvailabilityJson).HasMaxLength(1000).IsRequired();

        builder.HasIndex(g => g.Code).IsUnique();
        builder.HasIndex(g => g.IsActive);
    }
}

/// <summary>
/// EF configuration for <see cref="ModelCatalogEntry"/>.
/// </summary>
public sealed class ModelCatalogEntryConfiguration : IEntityTypeConfiguration<ModelCatalogEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ModelCatalogEntry> builder)
    {
        builder.ToTable("ModelCatalogEntries");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Code).HasMaxLength(64).IsRequired();
        builder.Property(m => m.ModelReference).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Name).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Provider).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Version).HasMaxLength(64).IsRequired();
        builder.Property(m => m.Family).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Parameters).HasMaxLength(64).IsRequired();
        builder.Property(m => m.Quantization).HasMaxLength(50);
        builder.Property(m => m.RecommendedGpuCode).HasMaxLength(64).IsRequired();
        builder.Property(m => m.MinimumGpuCode).HasMaxLength(64).IsRequired();
        builder.Property(m => m.License).HasMaxLength(200);
        builder.Property(m => m.DownloadSizeGb).HasPrecision(18, 4);
        builder.Property(m => m.PreferredRuntime).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.HasIndex(m => m.Code).IsUnique();
        builder.HasIndex(m => m.ModelReference);
        builder.HasIndex(m => m.IsActive);
    }
}

/// <summary>
/// EF configuration for <see cref="DeploymentHealth"/>.
/// </summary>
public sealed class DeploymentHealthConfiguration : IEntityTypeConfiguration<DeploymentHealth>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DeploymentHealth> builder)
    {
        builder.ToTable("DeploymentHealthSnapshots");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.State).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(h => h.DetailsJson).HasColumnType("longtext");

        builder.HasIndex(h => h.DeploymentId).IsUnique();
    }
}

/// <summary>
/// EF configuration for <see cref="DeploymentHistory"/>.
/// </summary>
public sealed class DeploymentHistoryConfiguration : IEntityTypeConfiguration<DeploymentHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DeploymentHistory> builder)
    {
        builder.ToTable("DeploymentHistoryEntries");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(32);
        builder.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(h => h.Message).HasMaxLength(1000);

        builder.HasIndex(h => new { h.DeploymentId, h.TimestampUtc });
    }
}
