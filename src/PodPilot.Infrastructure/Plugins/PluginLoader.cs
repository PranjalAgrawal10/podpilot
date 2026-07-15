using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Plugins.BuiltIn;

namespace PodPilot.Infrastructure.Plugins;

/// <summary>
/// Loads first-party plugins from DI registrations and optional disk assemblies.
/// </summary>
public sealed class PluginLoader : IPluginLoader
{
    private readonly IEnumerable<IPlugin> builtInPlugins;
    private readonly IHostEnvironment environment;
    private readonly ILogger<PluginLoader> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginLoader"/> class.
    /// </summary>
    public PluginLoader(
        IEnumerable<IPlugin> builtInPlugins,
        IHostEnvironment environment,
        ILogger<PluginLoader> logger)
    {
        this.builtInPlugins = builtInPlugins;
        this.environment = environment;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IPlugin>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var loaded = new Dictionary<string, IPlugin>(StringComparer.OrdinalIgnoreCase);
        foreach (var plugin in builtInPlugins)
        {
            loaded[plugin.PackageId] = plugin;
        }

        var pluginsRoot = Path.Combine(environment.ContentRootPath, "plugins");
        if (Directory.Exists(pluginsRoot))
        {
            foreach (var assemblyPath in Directory.EnumerateFiles(pluginsRoot, "*.dll", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var alc = new PluginLoadContext(assemblyPath);
                    var assembly = alc.LoadFromAssemblyPath(assemblyPath);
                    foreach (var type in assembly.GetTypes().Where(t =>
                                 typeof(IPlugin).IsAssignableFrom(t) &&
                                 t is { IsAbstract: false, IsInterface: false }))
                    {
                        if (Activator.CreateInstance(type) is not IPlugin plugin)
                        {
                            continue;
                        }

                        loaded[plugin.PackageId] = plugin;
                        logger.LogInformation("Hot-loaded plugin {PackageId} from {Path}", plugin.PackageId, assemblyPath);
                    }
                }
                catch (Exception ex) when (ex is ReflectionTypeLoadException or BadImageFormatException or FileLoadException)
                {
                    logger.LogWarning(ex, "Skipped invalid plugin assembly {Path}", assemblyPath);
                }
            }
        }

        await Task.CompletedTask;
        return loaded.Values.OrderBy(p => p.Name).ToList();
    }

    private sealed class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver resolver;

        public PluginLoadContext(string pluginPath)
            : base(isCollectible: true)
        {
            resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var path = resolver.ResolveAssemblyToPath(assemblyName);
            return path is null ? null : LoadFromAssemblyPath(path);
        }
    }
}
