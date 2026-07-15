using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Persistence.Configurations;

/// <summary>EF configuration for MCP servers.</summary>
public class McpServerConfiguration : IEntityTypeConfiguration<McpServer>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<McpServer> builder)
    {
        builder.ToTable("McpServers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Version).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Endpoint).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.AuthScheme).IsRequired().HasMaxLength(50);
        builder.Property(x => x.EncryptedCredential).HasMaxLength(8000);
        builder.Property(x => x.LastError).HasMaxLength(2000);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique();
    }
}

/// <summary>EF configuration for MCP tools.</summary>
public class McpToolConfiguration : IEntityTypeConfiguration<McpTool>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<McpTool> builder)
    {
        builder.ToTable("McpTools");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.InputSchemaJson).HasMaxLength(8000);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.McpServerId, x.Name }).IsUnique();
        builder.HasOne(x => x.McpServer)
            .WithMany(s => s.Tools)
            .HasForeignKey(x => x.McpServerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF configuration for MCP resources.</summary>
public class McpResourceConfiguration : IEntityTypeConfiguration<McpResource>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<McpResource> builder)
    {
        builder.ToTable("McpResources");
        builder.HasKey(x => x.Id);

        // Keep under MySQL utf8mb4 unique-index key limit (3072 bytes with Guid column).
        builder.Property(x => x.Uri).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.MimeType).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.McpServerId, x.Uri }).IsUnique();
        builder.HasOne(x => x.McpServer)
            .WithMany(s => s.Resources)
            .HasForeignKey(x => x.McpServerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF configuration for MCP prompts.</summary>
public class McpPromptConfiguration : IEntityTypeConfiguration<McpPrompt>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<McpPrompt> builder)
    {
        builder.ToTable("McpPrompts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.ArgumentsJson).HasMaxLength(8000);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.McpServerId, x.Name }).IsUnique();
        builder.HasOne(x => x.McpServer)
            .WithMany(s => s.Prompts)
            .HasForeignKey(x => x.McpServerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF configuration for MCP tool executions.</summary>
public class McpToolExecutionConfiguration : IEntityTypeConfiguration<McpToolExecution>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<McpToolExecution> builder)
    {
        builder.ToTable("McpToolExecutions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ToolName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);
        builder.HasIndex(x => new { x.OrganizationId, x.ExecutedAt });
        builder.HasOne(x => x.McpServer)
            .WithMany()
            .HasForeignKey(x => x.McpServerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
