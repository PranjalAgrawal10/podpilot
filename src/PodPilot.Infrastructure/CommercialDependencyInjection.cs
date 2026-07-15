using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Commercial;

namespace PodPilot.Infrastructure;

/// <summary>
/// Commercial platform dependency injection.
/// </summary>
public static class CommercialDependencyInjection
{
    /// <summary>
    /// Registers commercial platform services.
    /// </summary>
    public static IServiceCollection AddCommercialPlatform(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        services.AddHttpClient(nameof(StripePaymentGateway));
        services.AddHttpClient(nameof(RazorpayPaymentGateway));

        services.AddScoped<PlanCatalogSeeder>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        services.AddScoped<IPaymentGateway, RazorpayPaymentGateway>();
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
        services.AddScoped<IUsageMeteringService, UsageMeteringService>();
        services.AddScoped<IQuotaService, QuotaService>();
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<IOnboardingService, OnboardingService>();
        services.AddScoped<ITelemetryService, TelemetryService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<IReleaseService, ReleaseService>();
        services.AddScoped<ICommercialDashboardService, CommercialDashboardService>();

        if (environment.IsEnvironment("Testing"))
        {
            services.AddScoped<ICommercialNotificationService, NoOpCommercialNotificationService>();
        }
        else
        {
            services.AddScoped<ICommercialNotificationService, CommercialNotificationService>();
        }

        return services;
    }
}
