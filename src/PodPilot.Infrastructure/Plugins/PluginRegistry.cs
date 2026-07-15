using System.Collections.Concurrent;
using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.Plugins;

/// <summary>
/// Thread-safe in-memory registry of loaded plugin instances.
/// </summary>
public sealed class PluginRegistry : IPluginRegistry
{
    private readonly ConcurrentDictionary<string, IPlugin> plugins = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void Register(IPlugin plugin) =>
        plugins[plugin.PackageId] = plugin;

    /// <inheritdoc />
    public bool Unregister(string packageId) =>
        plugins.TryRemove(packageId, out _);

    /// <inheritdoc />
    public IPlugin? Get(string packageId) =>
        plugins.TryGetValue(packageId, out var plugin) ? plugin : null;

    /// <inheritdoc />
    public IReadOnlyList<IPlugin> List() =>
        plugins.Values.OrderBy(p => p.Name).ToList();
}
