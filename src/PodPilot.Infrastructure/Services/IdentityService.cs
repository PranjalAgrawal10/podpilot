using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Identity;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// ASP.NET Identity backed user management service.
/// </summary>
public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<ApplicationRole> roleManager;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityService"/> class.
    /// </summary>
    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IDateTimeService dateTimeService)
    {
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<(User? User, string[] Errors)> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        var applicationUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email.Trim().ToLowerInvariant(),
            Email = email.Trim().ToLowerInvariant(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            CreatedAt = dateTimeService.UtcNow,
            IsActive = true,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(applicationUser, password);

        if (!result.Succeeded)
        {
            return (null, result.Errors.Select(e => e.Description).ToArray());
        }

        return (MapToDomainUser(applicationUser), []);
    }

    /// <inheritdoc />
    public async Task<User?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var applicationUser = await userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());
        if (applicationUser is null)
        {
            return null;
        }

        var isValid = await userManager.CheckPasswordAsync(applicationUser, password);
        return isValid ? MapToDomainUser(applicationUser) : null;
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var applicationUser = await userManager.FindByIdAsync(userId.ToString());
        return applicationUser is null ? null : MapToDomainUser(applicationUser);
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var applicationUser = await userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());
        return applicationUser is null ? null : MapToDomainUser(applicationUser);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var applicationUser = await userManager.FindByIdAsync(userId.ToString());
        if (applicationUser is null)
        {
            return [];
        }

        var roles = await userManager.GetRolesAsync(applicationUser);
        return roles.ToList();
    }

    /// <inheritdoc />
    public async Task AssignRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default)
    {
        var applicationUser = await userManager.FindByIdAsync(userId.ToString());
        if (applicationUser is null)
        {
            return;
        }

        if (!await userManager.IsInRoleAsync(applicationUser, role))
        {
            await userManager.AddToRoleAsync(applicationUser, role);
        }
    }

    /// <inheritdoc />
    public async Task EnsureRolesExistAsync(CancellationToken cancellationToken = default)
    {
        foreach (var roleName in ApplicationConstants.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }
    }

    private static User MapToDomainUser(ApplicationUser applicationUser) =>
        new()
        {
            Id = applicationUser.Id,
            Email = applicationUser.Email ?? string.Empty,
            FirstName = applicationUser.FirstName,
            LastName = applicationUser.LastName,
            IsActive = applicationUser.IsActive,
            CreatedAt = applicationUser.CreatedAt,
            UpdatedAt = applicationUser.UpdatedAt,
        };
}
