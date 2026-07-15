using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Plugins;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Plugins;
using PodPilot.Infrastructure.Plugins.BuiltIn;

namespace PodPilot.Application.Tests.Plugins;

public class PluginRegistryTests
{
    [Fact]
    public void Register_Get_And_Unregister_Work()
    {
        var registry = new PluginRegistry();
        IPlugin plugin = new UtilityHealthPlugin();

        registry.Register(plugin);

        Assert.Same(plugin, registry.Get(plugin.PackageId));
        Assert.Contains(registry.List(), p => p.PackageId == plugin.PackageId);
        Assert.True(registry.Unregister(plugin.PackageId));
        Assert.Null(registry.Get(plugin.PackageId));
    }

    [Fact]
    public void Register_Overwrites_Same_PackageId()
    {
        var registry = new PluginRegistry();
        registry.Register(new UtilityHealthPlugin());
        registry.Register(new DuplicateUtilityPlugin());

        var resolved = registry.Get("com.podpilot.utility.health");
        Assert.NotNull(resolved);
        Assert.Equal("Duplicate", resolved.Name);
    }

    private sealed class DuplicateUtilityPlugin : BuiltInPluginBase
    {
        public override string PackageId => "com.podpilot.utility.health";

        public override string Name => "Duplicate";

        public override PluginType PluginType => PluginType.Utility;
    }
}
