using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>EF configuration for plugin entities.</summary>
public class PluginDefinitionConfiguration : IEntityTypeConfiguration<PluginDefinition>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PluginDefinition> builder)
    {
        builder.ToTable("Plugins");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PackageId).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Version).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Publisher).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.EntryAssembly).HasMaxLength(300);
        builder.Property(x => x.EntryType).HasMaxLength(500);
        builder.Property(x => x.RequiredPermissionsJson).IsRequired().HasMaxLength(4000);
        builder.Property(x => x.SettingsSchemaJson).HasMaxLength(8000);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => x.PackageId).IsUnique();
    }
}

/// <summary>EF configuration for plugin installations.</summary>
public class PluginInstallationConfiguration : IEntityTypeConfiguration<PluginInstallation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PluginInstallation> builder)
    {
        builder.ToTable("PluginInstallations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.GrantedPermissionsJson).IsRequired().HasMaxLength(4000);
        builder.Property(x => x.HealthMessage).HasMaxLength(1000);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.OrganizationId, x.PluginDefinitionId }).IsUnique();
        builder.HasOne(x => x.PluginDefinition)
            .WithMany(d => d.Installations)
            .HasForeignKey(x => x.PluginDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>EF configuration for plugin settings.</summary>
public class PluginSettingConfiguration : IEntityTypeConfiguration<PluginSetting>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PluginSetting> builder)
    {
        builder.ToTable("PluginSettings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Key).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Value).IsRequired().HasMaxLength(8000);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.PluginInstallationId, x.Key }).IsUnique();
        builder.HasOne(x => x.PluginInstallation)
            .WithMany(i => i.Settings)
            .HasForeignKey(x => x.PluginInstallationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF configuration for plugin logs.</summary>
public class PluginLogConfiguration : IEntityTypeConfiguration<PluginLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PluginLog> builder)
    {
        builder.ToTable("PluginLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Level).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Category).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Message).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.DetailsJson).HasMaxLength(8000);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.OrganizationId, x.OccurredAt });
        builder.HasOne(x => x.PluginInstallation)
            .WithMany(i => i.Logs)
            .HasForeignKey(x => x.PluginInstallationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
