using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Plugins;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Mcp;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Application.Tests.Plugins;

public class McpRegistryTests
{
    [Fact]
    public void ListKinds_Includes_BuiltIn_Catalog()
    {
        var registry = CreateRegistry(CreateDb(), Mock.Of<IMcpConnectionFactory>());
        var kinds = registry.ListKinds();

        Assert.Contains(kinds, k => k.ServerKind == McpServerKind.GitHub);
        Assert.Contains(kinds, k => k.ServerKind == McpServerKind.PostgreSQL);
        Assert.Contains(kinds, k => k.ServerKind == McpServerKind.Shell);
        Assert.Equal(Enum.GetValues<McpServerKind>().Length, kinds.Count);
    }

    [Fact]
    public async Task ResolveToolAsync_Returns_Enabled_Tool()
    {
        await using var db = CreateDb();
        var orgId = Guid.NewGuid();
        var server = new McpServer
        {
            OrganizationId = orgId,
            Name = "fs",
            Version = "1.0.0",
            ServerKind = McpServerKind.Filesystem,
            Endpoint = "http://127.0.0.1:7101/mcp",
            IsEnabled = true,
            Status = McpConnectionStatus.Connected,
            CreatedAt = DateTime.UtcNow,
        };
        await db.AddMcpServerAsync(server);
        await db.AddMcpToolAsync(new McpTool
        {
            OrganizationId = orgId,
            McpServerId = server.Id,
            Name = "read_file",
            IsEnabled = true,
            DiscoveredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var registry = CreateRegistry(db, Mock.Of<IMcpConnectionFactory>());
        var resolved = await registry.ResolveToolAsync(orgId, "read_file");

        Assert.NotNull(resolved);
        Assert.Equal(server.Id, resolved.Value.Server.Id);
        Assert.Equal("read_file", resolved.Value.Tool.Name);
    }

    [Fact]
    public async Task ResolveToolAsync_Ignores_Other_Organizations()
    {
        await using var db = CreateDb();
        var server = new McpServer
        {
            OrganizationId = Guid.NewGuid(),
            Name = "fs",
            Version = "1.0.0",
            ServerKind = McpServerKind.Filesystem,
            Endpoint = "http://127.0.0.1:7101/mcp",
            IsEnabled = true,
            Status = McpConnectionStatus.Connected,
            CreatedAt = DateTime.UtcNow,
        };
        await db.AddMcpServerAsync(server);
        await db.AddMcpToolAsync(new McpTool
        {
            OrganizationId = server.OrganizationId,
            McpServerId = server.Id,
            Name = "read_file",
            IsEnabled = true,
            DiscoveredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var registry = CreateRegistry(db, Mock.Of<IMcpConnectionFactory>());
        var resolved = await registry.ResolveToolAsync(Guid.NewGuid(), "read_file");

        Assert.Null(resolved);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"mcp-registry-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }

    private static McpRegistry CreateRegistry(ApplicationDbContext db, IMcpConnectionFactory factory) =>
        new(
            db,
            factory,
            Mock.Of<IEncryptionService>(),
            new McpServerKindCatalog(),
            Mock.Of<IDateTimeService>(d => d.UtcNow == DateTime.UtcNow),
            Mock.Of<IPluginNotificationService>(),
            NullLogger<McpRegistry>.Instance);
}
