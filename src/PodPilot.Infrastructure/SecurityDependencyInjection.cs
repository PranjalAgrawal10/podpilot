using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Security;
using PodPilot.Infrastructure.Security.Secrets;

namespace PodPilot.Infrastructure;

/// <summary>
/// Enterprise security dependency injection.
/// </summary>
public static class SecurityDependencyInjection
{
    /// <summary>
    /// Registers enterprise security services.
    /// </summary>
    public static IServiceCollection AddEnterpriseSecurity(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        services.AddHttpClient(nameof(SsoService));

        services.AddSingleton<IMfaChallengeStore, MfaChallengeStore>();

        services.AddScoped<LocalEncryptedSecretProvider>();
        services.AddScoped<ISecretProvider>(sp => sp.GetRequiredService<LocalEncryptedSecretProvider>());
        services.AddScoped<ISecretProvider, AzureKeyVaultSecretProvider>();
        services.AddScoped<ISecretProvider, AwsSecretsManagerSecretProvider>();
        services.AddScoped<ISecretProvider, HashiCorpVaultSecretProvider>();
        services.AddScoped<ISecretProviderFactory, SecretProviderFactory>();
        services.AddScoped<ISecretManager, SecretManager>();

        services.AddScoped<IMfaService, MfaService>();
        services.AddScoped<IEnterpriseAuditService, EnterpriseAuditService>();
        services.AddScoped<ISsoService, SsoService>();
        services.AddScoped<IScimProvisioningService, ScimProvisioningService>();
        services.AddScoped<IPolicyEngine, PolicyEngine>();
        services.AddScoped<IComplianceService, ComplianceService>();
        services.AddScoped<ISessionTracker, SessionTracker>();
        services.AddScoped<ISecurityAlertService, SecurityAlertService>();
        services.AddScoped<ISecurityDashboardService, SecurityDashboardService>();

        if (environment.IsEnvironment("Testing"))
        {
            services.AddScoped<ISecurityNotificationService, NoOpSecurityNotificationService>();
        }
        else
        {
            services.AddScoped<ISecurityNotificationService, SecurityNotificationService>();
        }

        return services;
    }
}
