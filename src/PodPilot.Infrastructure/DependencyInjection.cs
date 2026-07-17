using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MySql.EntityFrameworkCore.Extensions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.BackgroundServices;
using PodPilot.Infrastructure.Compute;
using PodPilot.Infrastructure.Configuration;
using PodPilot.Infrastructure.Gateway;
using PodPilot.Infrastructure.Hubs;
using PodPilot.Infrastructure.Identity;
using PodPilot.Infrastructure.Ollama;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.RunPod;
using PodPilot.Infrastructure.Services;
namespace PodPilot.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection extensions.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure layer services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        if (configuration.GetValue<string>("DatabaseProvider") == "InMemory")
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(configuration.GetValue<string>("InMemoryDatabaseName") ?? "PodPilotTest"));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is not configured.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySQL(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        }

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are not configured.");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.Zero,
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                };
            })
            .AddScheme<AuthenticationSchemeOptions, GatewayApiKeyAuthenticationHandler>(
                GatewayAuthConstants.SchemeName,
                _ => { });

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
            .AddPolicy("MemberOrAdmin", policy => policy.RequireRole("Admin", "Member"));

        services.AddHttpContextAccessor();
        services.AddDataProtection();
        services.AddHttpClient(nameof(RunPodProvider));
        services.AddHttpClient(nameof(RunPodPodProvider));
        services.AddHttpClient(nameof(OllamaInferenceClient));
        services.AddHttpClient(nameof(OllamaClient));
        services.AddHttpClient(nameof(StreamingProxy));

        services.AddSingleton<IComputeProvider, RunPodProvider>();
        services.AddSingleton<IComputeProviderFactory, ComputeProviderFactory>();
        services.AddSingleton<IPodProvider, RunPodPodProvider>();
        services.AddSingleton<IPodProviderFactory, PodProviderFactory>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IProviderService, ProviderService>();
        services.AddScoped<IPodService, PodService>();
        services.AddScoped<IPodLifecycleService, PodLifecycleService>();
        services.AddScoped<IPodRecoveryService, PodRecoveryService>();
        services.AddScoped<IPodNotificationService, PodNotificationService>();

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IHttpContextService, HttpContextService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IOrganizationAuthorizationService, OrganizationAuthorizationService>();
        services.AddScoped<IAuthTokenIssuer, AuthTokenIssuer>();

        services.AddScoped<IAiGateway, AiGateway>();
        services.AddScoped<IGatewayRouter, GatewayRouter>();
        services.AddScoped<IStreamingProxy, StreamingProxy>();
        services.AddScoped<IInferenceClient, OllamaInferenceClient>();
        services.AddScoped<IGatewayApiKeyService, GatewayApiKeyService>();
        services.AddScoped<IGatewayNotificationService, GatewayNotificationService>();
        services.AddSingleton<IGatewayRateLimitService, GatewayRateLimitService>();

        services.AddScoped<IOllamaClient, OllamaClient>();
        services.AddScoped<IModelRepository, Persistence.Repositories.ModelRepository>();
        services.AddScoped<IModelService, ModelService>();
        services.AddScoped<IModelHealthService, ModelHealthService>();
        services.AddScoped<IModelNotificationService, ModelNotificationService>();

        return services;
    }

    /// <summary>
    /// Registers scheduler layer services. Call after <see cref="AddInfrastructure"/>.
    /// </summary>
    public static IServiceCollection AddScheduler(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment) =>
        services
            .AddSchedulerServices(configuration, environment)
            .AddSchedulerHostedServices(environment);

    /// <summary>
    /// Registers orchestrator layer services. Call after <see cref="AddScheduler"/>.
    /// </summary>
    public static IServiceCollection AddOrchestratorLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment) =>
        services.AddOrchestrator(configuration, environment);

    /// <summary>
    /// Registers observability layer services. Call after <see cref="AddOrchestratorLayer"/>.
    /// </summary>
    public static IServiceCollection AddObservabilityLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment) =>
        services.AddObservability(configuration, environment);

    /// <summary>
    /// Registers AI providers layer services. Call after <see cref="AddObservabilityLayer"/>.
    /// </summary>
    public static IServiceCollection AddAiProvidersLayer(
        this IServiceCollection services,
        IHostEnvironment environment) =>
        services.AddAiProviders(environment).AddIntelligentRouting(environment);

    /// <summary>
    /// Registers plugin system and MCP ecosystem services.
    /// </summary>
    public static IServiceCollection AddPluginSystemLayer(
        this IServiceCollection services,
        IHostEnvironment environment) =>
        services.AddPluginSystem(environment);

    /// <summary>
    /// Registers enterprise security services.
    /// </summary>
    public static IServiceCollection AddEnterpriseSecurityLayer(
        this IServiceCollection services,
        IHostEnvironment environment) =>
        services.AddEnterpriseSecurity(environment);

    /// <summary>
    /// Registers commercial platform services.
    /// </summary>
    public static IServiceCollection AddCommercialPlatformLayer(
        this IServiceCollection services,
        IHostEnvironment environment) =>
        services.AddCommercialPlatform(environment);

    /// <summary>
    /// Registers one-click AI pod deployment services.
    /// </summary>
    public static IServiceCollection AddDeploymentsLayer(
        this IServiceCollection services,
        IHostEnvironment environment) =>
        services.AddDeployments(environment);

    /// <summary>
    /// Registers background services for non-testing environments.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The host environment.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddInfrastructureHostedServices(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        if (!environment.IsEnvironment("Testing"))
        {
            services.AddHostedService<ProviderHealthWorker>();
            services.AddHostedService<PodStatusSyncWorker>();
            services.AddHostedService<IdleDetectionWorker>();
            services.AddHostedService<PodWakeWorker>();
            services.AddHostedService<ModelHealthWorker>();
        }

        return services;
    }
}
