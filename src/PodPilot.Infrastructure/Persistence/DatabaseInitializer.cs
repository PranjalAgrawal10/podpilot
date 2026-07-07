using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Infrastructure.Persistence;

/// <summary>
/// Initializes the database on application startup.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Applies pending migrations and seeds default data.
    /// </summary>
    /// <param name="services">The application service provider.</param>
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        try
        {
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");

            await identityService.EnsureRolesExistAsync();
            logger.LogInformation("Default roles ensured.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
