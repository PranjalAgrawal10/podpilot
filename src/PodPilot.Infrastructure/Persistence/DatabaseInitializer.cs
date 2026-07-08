using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
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

            await SeedPermissionsAndRolesAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    private static async Task SeedPermissionsAndRolesAsync(ApplicationDbContext context, ILogger logger)
    {
        if (!await context.Permissions.AnyAsync())
        {
            var permissions = PermissionNames.All
                .Select(name => new Permission
                {
                    Id = CreatePermissionId(name),
                    Name = name,
                    Description = name.Replace('.', ' '),
                    Category = name.Split('.')[0],
                })
                .ToList();

            context.Permissions.AddRange(permissions);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} permissions.", permissions.Count);
        }

        if (!await context.OrgRoles.AnyAsync())
        {
            var roleDefinitions = new[]
            {
                (OrganizationRole.Owner, "Owner", "Full control including ownership transfer and organization deletion."),
                (OrganizationRole.Admin, "Admin", "Can manage members, invitations, and organization settings."),
                (OrganizationRole.Developer, "Developer", "Can create and manage workloads but not users."),
                (OrganizationRole.Viewer, "Viewer", "Read-only access to organization resources."),
            };

            var permissionsByName = await context.Permissions.ToDictionaryAsync(p => p.Name);
            var roles = new List<Role>();
            var rolePermissions = new List<RolePermission>();

            foreach (var (organizationRole, name, description) in roleDefinitions)
            {
                var role = new Role
                {
                    Id = CreateRoleId(organizationRole),
                    Name = name,
                    Description = description,
                    OrganizationRole = organizationRole,
                };

                roles.Add(role);

                foreach (var permissionName in RolePermissionMatrix.GetPermissions(organizationRole))
                {
                    if (permissionsByName.TryGetValue(permissionName, out var permission))
                    {
                        rolePermissions.Add(new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = permission.Id,
                        });
                    }
                }
            }

            context.OrgRoles.AddRange(roles);
            context.Set<RolePermission>().AddRange(rolePermissions);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} organization roles.", roles.Count);
        }
    }

    private static Guid CreatePermissionId(string permissionName) =>
        Guid.Parse($"10000000-0000-4000-8000-{CreateStableSuffix(permissionName)}");

    private static Guid CreateRoleId(OrganizationRole role) =>
        Guid.Parse($"20000000-0000-4000-8000-{(int)role:D12}");

    private static string CreateStableSuffix(string value)
    {
        var hash = value.Aggregate(0, (current, character) => current + character);
        return (hash % 1_000_000_000_000L).ToString("D12");
    }
}
