using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.BackgroundServices;
using PodPilot.Infrastructure.Deployments;
using PodPilot.Infrastructure.Deployments.Cloud;
using PodPilot.Infrastructure.Deployments.Runtimes;

namespace PodPilot.Infrastructure;

/// <summary>
/// Dependency injection for one-click AI pod deployments.
/// </summary>
public static class DeploymentDependencyInjection
{
    /// <summary>
    /// Registers deployment services and background workers.
    /// </summary>
    public static IServiceCollection AddDeployments(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddHttpClient(nameof(OllamaRuntimeProvider));
        services.AddHttpClient(nameof(VllmRuntimeProvider));
        services.AddHttpClient(nameof(LlamaCppRuntimeProvider));

        services.AddScoped<DeploymentCatalogSeeder>();
        services.AddScoped<IDeploymentCatalogService, DeploymentCatalogService>();
        services.AddScoped<IDeploymentService, DeploymentService>();

        services.AddScoped<IRuntimeProvider, OllamaRuntimeProvider>();
        services.AddScoped<IRuntimeProvider, VllmRuntimeProvider>();
        services.AddScoped<IRuntimeProvider, LlamaCppRuntimeProvider>();
        services.AddScoped<IRuntimeProviderFactory, RuntimeProviderFactory>();

        services.AddSingleton<IDeploymentCloudAdapter, RunPodDeploymentCloudAdapter>();
        services.AddSingleton<IDeploymentCloudAdapter, VastAiDeploymentCloudAdapter>();
        services.AddSingleton<IDeploymentCloudAdapter, LambdaLabsDeploymentCloudAdapter>();
        services.AddSingleton<IDeploymentCloudAdapter, AzureGpuDeploymentCloudAdapter>();
        services.AddSingleton<IDeploymentCloudAdapter, AwsGpuDeploymentCloudAdapter>();
        services.AddSingleton<IDeploymentCloudAdapter, GoogleCloudGpuDeploymentCloudAdapter>();
        services.AddSingleton<IDeploymentCloudAdapter, KubernetesDeploymentCloudAdapter>();
        services.AddSingleton<DeploymentCloudAdapterFactory>();
        services.AddSingleton<IDeploymentCloudAdapterFactory>(sp =>
            sp.GetRequiredService<DeploymentCloudAdapterFactory>());

        if (environment.IsEnvironment("Testing"))
        {
            services.AddScoped<IDeploymentNotificationService, NoOpDeploymentNotificationService>();
        }
        else
        {
            services.AddScoped<IDeploymentNotificationService, DeploymentNotificationService>();
            services.AddHostedService<DeploymentWorker>();
            services.AddHostedService<ModelDownloadWorker>();
            services.AddHostedService<DeploymentHealthWorker>();
            services.AddHostedService<DeploymentCleanupWorker>();
            services.AddHostedService<DeploymentRetryWorker>();
        }

        return services;
    }
}
