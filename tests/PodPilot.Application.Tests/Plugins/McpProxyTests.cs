using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Plugins;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Mcp;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Application.Tests.Plugins;

public class McpProxyTests
{
    [Fact]
    public async Task ExecuteToolAsync_Calls_Connection_And_Audits()
    {
        await using var db = CreateDb();
        var orgId = Guid.NewGuid();
        var server = new McpServer
        {
            OrganizationId = orgId,
            Name = "shell",
            Version = "1.0.0",
            ServerKind = McpServerKind.Shell,
            Endpoint = "http://127.0.0.1:7106/mcp",
            IsEnabled = true,
            Status = McpConnectionStatus.Connected,
            MaxRetries = 0,
            CreatedAt = DateTime.UtcNow,
        };
        await db.AddMcpServerAsync(server);
        await db.AddMcpToolAsync(new McpTool
        {
            OrganizationId = orgId,
            McpServerId = server.Id,
            Name = "run",
            IsEnabled = true,
            DiscoveredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var connection = new Mock<IMcpConnection>();
        connection.Setup(c => c.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        connection
            .Setup(c => c.CallToolAsync("run", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new McpToolCallResult
            {
                Succeeded = true,
                ContentJson = "{\"ok\":true}",
                DurationMs = 12,
            });
        connection.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var factory = new Mock<IMcpConnectionFactory>();
        factory
            .Setup(f => f.CreateAsync(It.IsAny<McpServer>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(connection.Object);

        var notifications = new Mock<IPluginNotificationService>();
        var proxy = new McpProxy(
            db,
            new McpRegistry(
                db,
                factory.Object,
                Mock.Of<IEncryptionService>(),
                new McpServerKindCatalog(),
                Mock.Of<IDateTimeService>(d => d.UtcNow == DateTime.UtcNow),
                notifications.Object,
                NullLogger<McpRegistry>.Instance),
            factory.Object,
            Mock.Of<IEncryptionService>(),
            Mock.Of<IDateTimeService>(d => d.UtcNow == DateTime.UtcNow),
            notifications.Object,
            NullLogger<McpProxy>.Instance);

        var result = await proxy.ExecuteToolAsync(new McpToolCallRequest
        {
            OrganizationId = orgId,
            ToolName = "run",
            ArgumentsJson = "{\"cmd\":\"echo\"}",
        });

        Assert.True(result.Succeeded);
        Assert.Equal("{\"ok\":true}", result.ContentJson);
        Assert.Equal(1, await db.McpToolExecutions.CountAsync());
        notifications.Verify(
            n => n.NotifyToolExecutedAsync(orgId, server.Id, "run", true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteToolAsync_Throws_When_Tool_Missing()
    {
        await using var db = CreateDb();
        var proxy = new McpProxy(
            db,
            new McpRegistry(
                db,
                Mock.Of<IMcpConnectionFactory>(),
                Mock.Of<IEncryptionService>(),
                new McpServerKindCatalog(),
                Mock.Of<IDateTimeService>(d => d.UtcNow == DateTime.UtcNow),
                Mock.Of<IPluginNotificationService>(),
                NullLogger<McpRegistry>.Instance),
            Mock.Of<IMcpConnectionFactory>(),
            Mock.Of<IEncryptionService>(),
            Mock.Of<IDateTimeService>(d => d.UtcNow == DateTime.UtcNow),
            Mock.Of<IPluginNotificationService>(),
            NullLogger<McpProxy>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            proxy.ExecuteToolAsync(new McpToolCallRequest
            {
                OrganizationId = Guid.NewGuid(),
                ToolName = "missing",
            }));
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"mcp-proxy-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
