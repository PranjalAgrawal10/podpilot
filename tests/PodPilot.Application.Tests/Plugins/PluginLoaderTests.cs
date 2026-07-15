using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Plugins;
using PodPilot.Infrastructure.Plugins.BuiltIn;

namespace PodPilot.Application.Tests.Plugins;

public class PluginLoaderTests
{
    [Fact]
    public async Task LoadAsync_Returns_BuiltIn_Plugins()
    {
        IEnumerable<IPlugin> builtins =
        [
            new UtilityHealthPlugin(),
            new NotificationAuditPlugin(),
            new McpBridgePlugin(),
            new MonitoringTelemetryPlugin(),
        ];

        var loader = new PluginLoader(
            builtins,
            new FakeHostEnvironment(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))),
            NullLogger<PluginLoader>.Instance);

        var loaded = await loader.LoadAsync();

        Assert.Equal(4, loaded.Count);
        Assert.Contains(loaded, p => p.PackageId == "com.podpilot.utility.health");
        Assert.Contains(loaded, p => p.PackageId == "com.podpilot.mcp.bridge");
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public FakeHostEnvironment(string contentRootPath) => ContentRootPath = contentRootPath;

        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "PodPilot.Tests";

        public string ContentRootPath { get; set; }

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
