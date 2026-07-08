using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Persistence;

/// <summary>
/// Initializes the database on application startup and records migration/seed history.
/// </summary>
public static class DatabaseInitializer
{
    private const string PermissionsSeeder = "PermissionsSeeder";
    private const string OrganizationRolesSeeder = "OrganizationRolesSeeder";
    private const string MissingPermissionsSeeder = "MissingPermissionsSeeder";
    private const string IdentityRolesSeeder = "IdentityRolesSeeder";
    private const string SeederVersion = "1.0";

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

            await SyncMigrationHistoryAsync(context, logger);

            await RunSeederAsync(
                context,
                logger,
                IdentityRolesSeeder,
                async () =>
                {
                    await identityService.EnsureRolesExistAsync();
                    logger.LogInformation("Default roles ensured.");
                    return "rolesEnsured=true";
                });

            await RunSeederAsync(
                context,
                logger,
                PermissionsSeeder,
                async () => await SeedPermissionsAndRolesAsync(context, logger));

            await RunSeederAsync(
                context,
                logger,
                MissingPermissionsSeeder,
                async () => await EnsureMissingPermissionsAsync(context, logger));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    private static async Task SyncMigrationHistoryAsync(ApplicationDbContext context, ILogger logger)
    {
        var efHistory = await ReadEfMigrationHistoryAsync(context);
        if (efHistory.Count == 0)
        {
            return;
        }

        var existing = await context.DatabaseMigrationHistoryEntries
            .Select(h => h.MigrationId)
            .ToListAsync();

        var existingSet = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;
        var added = 0;

        foreach (var row in efHistory)
        {
            if (existingSet.Contains(row.MigrationId))
            {
                continue;
            }

            context.DatabaseMigrationHistoryEntries.Add(new DatabaseMigrationHistory
            {
                MigrationId = row.MigrationId,
                ProductVersion = row.ProductVersion,
                AppliedAt = now,
            });
            added++;
        }

        if (added > 0)
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Recorded {Count} migration(s) in DatabaseMigrationHistory.", added);
        }
    }

    private static async Task<List<EfMigrationRow>> ReadEfMigrationHistoryAsync(ApplicationDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory";
        await using var reader = await command.ExecuteReaderAsync();

        var efHistory = new List<EfMigrationRow>();
        while (await reader.ReadAsync())
        {
            efHistory.Add(new EfMigrationRow
            {
                MigrationId = reader.GetString(0),
                ProductVersion = reader.GetString(1),
            });
        }

        return efHistory;
    }

    private static async Task RunSeederAsync(
        ApplicationDbContext context,
        ILogger logger,
        string seederName,
        Func<Task<string?>> seeder)
    {
        try
        {
            var details = await seeder();
            context.DatabaseSeedHistoryEntries.Add(new DatabaseSeedHistory
            {
                SeederName = seederName,
                Version = SeederVersion,
                AppliedAt = DateTime.UtcNow,
                Success = true,
                Details = details,
            });
            await context.SaveChangesAsync();
            logger.LogInformation("Seeder {SeederName} completed.", seederName);
        }
        catch (Exception ex)
        {
            context.DatabaseSeedHistoryEntries.Add(new DatabaseSeedHistory
            {
                SeederName = seederName,
                Version = SeederVersion,
                AppliedAt = DateTime.UtcNow,
                Success = false,
                Details = ex.Message,
            });
            await context.SaveChangesAsync();
            throw;
        }
    }

    private static async Task<string?> SeedPermissionsAndRolesAsync(ApplicationDbContext context, ILogger logger)
    {
        var permissionsAdded = 0;
        var rolesAdded = 0;

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
            permissionsAdded = permissions.Count;
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
            rolesAdded = roles.Count;
            logger.LogInformation("Seeded {Count} organization roles.", roles.Count);
        }

        return $"permissionsAdded={permissionsAdded};rolesAdded={rolesAdded}";
    }

    private static async Task<string?> EnsureMissingPermissionsAsync(ApplicationDbContext context, ILogger logger)
    {
        var existing = await context.Permissions.Select(p => p.Name).ToListAsync();
        var missing = PermissionNames.All.Except(existing).ToList();
        if (missing.Count == 0)
        {
            return "permissionsAdded=0;rolePermissionsAdded=0";
        }

        var permissions = missing
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
        logger.LogInformation("Added {Count} missing permissions.", permissions.Count);

        var permissionsByName = await context.Permissions.ToDictionaryAsync(p => p.Name);
        var roles = await context.OrgRoles.ToListAsync();
        var existingRolePermissions = await context.Set<RolePermission>()
            .Select(rp => new { rp.RoleId, rp.PermissionId })
            .ToListAsync();
        var existingSet = existingRolePermissions
            .Select(rp => (rp.RoleId, rp.PermissionId))
            .ToHashSet();

        var additions = new List<RolePermission>();
        foreach (var role in roles)
        {
            foreach (var permissionName in RolePermissionMatrix.GetPermissions(role.OrganizationRole))
            {
                if (permissionsByName.TryGetValue(permissionName, out var permission)
                    && !existingSet.Contains((role.Id, permission.Id)))
                {
                    additions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
                }
            }
        }

        if (additions.Count > 0)
        {
            context.Set<RolePermission>().AddRange(additions);
            await context.SaveChangesAsync();
            logger.LogInformation("Linked {Count} missing role permissions.", additions.Count);
        }

        return $"permissionsAdded={permissions.Count};rolePermissionsAdded={additions.Count}";
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

    private sealed class EfMigrationRow
    {
        public string MigrationId { get; set; } = string.Empty;

        public string ProductVersion { get; set; } = string.Empty;
    }
}
