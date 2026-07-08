using System.Reflection;
using Microsoft.OpenApi;

namespace PodPilot.Api.Extensions;

/// <summary>
/// Swagger / OpenAPI configuration extensions.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Registers Swagger generation with JWT security and XML documentation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddPodPilotSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PodPilot API",
                Version = "v1",
                Description = "PodPilot AI Infrastructure Autopilot API — Part 1 Foundation",
                Contact = new OpenApiContact
                {
                    Name = "PodPilot",
                    Email = "support@podpilot.dev",
                },
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT access token. Example: eyJhbGciOiJIUzI1NiIs...",
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
            });

            options.TagActionsBy(api =>
            {
                if (!api.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller))
                {
                    return ["Default"];
                }

                var tag = controller switch
                {
                    "Auth" => "Authentication",
                    "Users" => "Users",
                    "Health" => "Health",
                    "Providers" => "Providers",
                    _ => controller,
                };

                return [tag];
            });

            options.OrderActionsBy(description =>
                $"{description.RelativePath}_{description.HttpMethod}");

            IncludeXmlComments(options, Assembly.GetExecutingAssembly().GetName().Name!);
            IncludeXmlComments(options, "PodPilot.Application");
            IncludeXmlComments(options, "PodPilot.Contracts");
        });

        return services;
    }

    /// <summary>
    /// Enables Swagger UI middleware when configured or in development.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application.</returns>
    public static WebApplication UsePodPilotSwagger(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment() && !app.Configuration.GetValue<bool>("Swagger:Enabled"))
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "PodPilot API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "PodPilot API";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
        });

        app.MapGet("/", () => Results.Redirect("/swagger"))
            .ExcludeFromDescription();

        return app;
    }

    private static void IncludeXmlComments(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options, string assemblyName)
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.xml");
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    }
}
