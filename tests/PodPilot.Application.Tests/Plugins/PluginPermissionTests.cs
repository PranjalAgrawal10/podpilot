using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Plugins;
using PodPilot.Infrastructure.Plugins.BuiltIn;

namespace PodPilot.Application.Tests.Plugins;

public class PluginPermissionTests
{
    [Fact]
    public async Task InstallAsync_Rejects_Missing_Required_Permissions()
    {
        await using var db = CreateDb();
        var registry = new PluginRegistry();
        registry.Register(new UtilityHealthPlugin());

        var loader = new Mock<IPluginLoader>();
        loader.Setup(l => l.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new UtilityHealthPlugin()]);

        var installer = new PluginInstaller(
            db,
            loader.Object,
            registry,
            Mock.Of<IDateTimeService>(d => d.UtcNow == DateTime.UtcNow),
            Mock.Of<IPluginNotificationService>(),
            Mock.Of<IPolicyEngine>(),
            NullLogger<PluginInstaller>.Instance);

        await installer.SyncCatalogAsync();

        await Assert.ThrowsAsync<ValidationException>(() =>
            installer.InstallAsync(Guid.NewGuid(), "com.podpilot.utility.health", grantedPermissions: []));
    }

    [Fact]
    public async Task InstallAsync_Succeeds_When_Permissions_Granted()
    {
        await using var db = CreateDb();
        var registry = new PluginRegistry();
        var plugin = new UtilityHealthPlugin();
        registry.Register(plugin);

        var loader = new Mock<IPluginLoader>();
        loader.Setup(l => l.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([plugin]);

        var installer = new PluginInstaller(
            db,
            loader.Object,
            registry,
            Mock.Of<IDateTimeService>(d => d.UtcNow == DateTime.UtcNow),
            Mock.Of<IPluginNotificationService>(),
            Mock.Of<IPolicyEngine>(),
            NullLogger<PluginInstaller>.Instance);

        var orgId = Guid.NewGuid();
        var installationId = await installer.InstallAsync(
            orgId,
            plugin.PackageId,
            plugin.RequiredPermissions);

        var installation = await db.PluginInstallations.SingleAsync(i => i.Id == installationId);
        var granted = JsonSerializer.Deserialize<List<string>>(installation.GrantedPermissionsJson);
        Assert.Contains("Plugin.Read", granted!);
        Assert.Equal(PluginStatus.Installed, installation.Status);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"plugin-perm-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
