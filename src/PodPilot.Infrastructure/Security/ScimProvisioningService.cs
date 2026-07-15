using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Security;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Identity;

namespace PodPilot.Infrastructure.Security;

/// <summary>
/// SCIM 2.0 user and group provisioning.
/// </summary>
public sealed class ScimProvisioningService : IScimProvisioningService
{
    private readonly IApplicationDbContext dbContext;
    private readonly IIdentityService identityService;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScimProvisioningService"/> class.
    /// </summary>
    public ScimProvisioningService(
        IApplicationDbContext dbContext,
        IIdentityService identityService,
        UserManager<ApplicationUser> userManager,
        IDateTimeService dateTimeService)
    {
        this.dbContext = dbContext;
        this.identityService = identityService;
        this.userManager = userManager;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<ScimUserResult> UpsertUserAsync(
        Guid organizationId,
        ScimUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("SCIM user email is required.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await identityService.GetUserByEmailAsync(email, cancellationToken);
        var created = false;
        Guid userId;

        if (existing is null)
        {
            var (user, errors) = await identityService.CreateUserAsync(
                email,
                $"Scim!{Guid.NewGuid():N}aA1",
                request.FirstName ?? request.UserName,
                request.LastName ?? "User",
                cancellationToken);
            if (user is null)
            {
                throw new ValidationException(string.Join("; ", errors));
            }

            await identityService.AssignRoleAsync(user.Id, ApplicationConstants.MemberRole, cancellationToken);
            userId = user.Id;
            created = true;
        }
        else
        {
            userId = existing.Id;
            var appUser = await userManager.FindByIdAsync(userId.ToString());
            if (appUser is not null)
            {
                appUser.FirstName = request.FirstName ?? appUser.FirstName;
                appUser.LastName = request.LastName ?? appUser.LastName;
                appUser.IsActive = request.Active;
                appUser.UpdatedAt = dateTimeService.UtcNow;
                await userManager.UpdateAsync(appUser);
            }
        }

        var member = await dbContext.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == userId, cancellationToken);
        if (member is null)
        {
            var role = ResolveRole(request.Groups);
            await dbContext.AddOrganizationMemberAsync(
                new OrganizationMember
                {
                    OrganizationId = organizationId,
                    UserId = userId,
                    Role = role,
                    JoinedAt = dateTimeService.UtcNow,
                    Status = request.Active ? MemberStatus.Active : MemberStatus.Suspended,
                    IsActive = request.Active,
                    CreatedAt = dateTimeService.UtcNow,
                },
                cancellationToken);
        }
        else
        {
            member.IsActive = request.Active;
            member.Status = request.Active ? MemberStatus.Active : MemberStatus.Suspended;
            member.UpdatedAt = dateTimeService.UtcNow;
        }

        var mappingKey = $"user:{request.ExternalId}";
        var mapping = await dbContext.ScimMappings
            .FirstOrDefaultAsync(
                m => m.OrganizationId == organizationId && m.ExternalGroupId == mappingKey,
                cancellationToken);
        if (mapping is null)
        {
            await dbContext.AddScimMappingAsync(
                new ScimMapping
                {
                    OrganizationId = organizationId,
                    ExternalGroupId = mappingKey,
                    ExternalGroupName = email,
                    OrganizationRole = OrganizationRole.Viewer.ToString(),
                    IsEnabled = request.Active,
                    CreatedAt = dateTimeService.UtcNow,
                },
                cancellationToken);
        }
        else
        {
            mapping.IsEnabled = request.Active;
            mapping.ExternalGroupName = email;
            mapping.UpdatedAt = dateTimeService.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new ScimUserResult
        {
            UserId = userId,
            ExternalId = request.ExternalId,
            Created = created,
        };
    }

    /// <inheritdoc />
    public async Task DisableUserAsync(
        Guid organizationId,
        string externalUserId,
        CancellationToken cancellationToken = default)
    {
        var mappingKey = $"user:{externalUserId}";
        var mapping = await dbContext.ScimMappings
            .FirstOrDefaultAsync(
                m => m.OrganizationId == organizationId && m.ExternalGroupId == mappingKey,
                cancellationToken)
            ?? throw new NotFoundException("SCIM user", externalUserId);

        User? user = null;
        if (!string.IsNullOrWhiteSpace(mapping.ExternalGroupName) &&
            mapping.ExternalGroupName.Contains('@', StringComparison.Ordinal))
        {
            user = await identityService.GetUserByEmailAsync(mapping.ExternalGroupName, cancellationToken);
        }

        if (user is null)
        {
            // ExternalGroupName may be username; search members isn't reliable — best effort by email in name.
            throw new NotFoundException("SCIM user", externalUserId);
        }

        var appUser = await userManager.FindByIdAsync(user.Id.ToString());
        if (appUser is not null)
        {
            appUser.IsActive = false;
            appUser.UpdatedAt = dateTimeService.UtcNow;
            await userManager.UpdateAsync(appUser);
        }

        var member = await dbContext.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == user.Id, cancellationToken);
        if (member is not null)
        {
            member.IsActive = false;
            member.Status = MemberStatus.Suspended;
            member.UpdatedAt = dateTimeService.UtcNow;
        }

        mapping.IsEnabled = false;
        mapping.UpdatedAt = dateTimeService.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SyncGroupAsync(
        Guid organizationId,
        ScimGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var mapping = await dbContext.ScimMappings
            .FirstOrDefaultAsync(
                m => m.OrganizationId == organizationId && m.ExternalGroupId == request.ExternalGroupId,
                cancellationToken);

        var roleName = string.IsNullOrWhiteSpace(request.OrganizationRole)
            ? OrganizationRole.Viewer.ToString()
            : request.OrganizationRole;

        if (mapping is null)
        {
            await dbContext.AddScimMappingAsync(
                new ScimMapping
                {
                    OrganizationId = organizationId,
                    ExternalGroupId = request.ExternalGroupId,
                    ExternalGroupName = request.DisplayName,
                    OrganizationRole = roleName,
                    IsEnabled = true,
                    CreatedAt = dateTimeService.UtcNow,
                },
                cancellationToken);
        }
        else
        {
            mapping.ExternalGroupName = request.DisplayName ?? mapping.ExternalGroupName;
            mapping.OrganizationRole = roleName;
            mapping.IsEnabled = true;
            mapping.UpdatedAt = dateTimeService.UtcNow;
        }

        var role = ApplicationConstants.ParseRoleName(roleName);
        foreach (var memberExternalId in request.MemberExternalIds)
        {
            var userMapping = await dbContext.ScimMappings.AsNoTracking()
                .FirstOrDefaultAsync(
                    m => m.OrganizationId == organizationId && m.ExternalGroupId == $"user:{memberExternalId}",
                    cancellationToken);
            if (userMapping?.ExternalGroupName is null)
            {
                continue;
            }

            var user = await identityService.GetUserByEmailAsync(userMapping.ExternalGroupName, cancellationToken);
            if (user is null)
            {
                continue;
            }

            var member = await dbContext.OrganizationMembers
                .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == user.Id, cancellationToken);
            if (member is not null)
            {
                member.Role = role;
                member.UpdatedAt = dateTimeService.UtcNow;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static OrganizationRole ResolveRole(IReadOnlyList<string> groups)
    {
        foreach (var group in groups)
        {
            if (Enum.TryParse<OrganizationRole>(group, true, out var role))
            {
                return role;
            }
        }

        return OrganizationRole.Viewer;
    }
}
