using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Organizations;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Organizations.Commands.CreateOrganization;

/// <summary>
/// Handles organization creation.
/// </summary>
public sealed class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, OrganizationResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateOrganizationCommandHandler"/> class.
    /// </summary>
    public CreateOrganizationCommandHandler(
        ICurrentUserService currentUserService,
        IApplicationDbContext dbContext,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.dbContext = dbContext;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<OrganizationResponse> Handle(
        CreateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var userId = currentUserService.UserId.Value;
        var now = dateTimeService.UtcNow;
        var slug = await CreateUniqueSlugAsync(request.Name, cancellationToken);

        var organization = new Organization
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            Logo = request.Logo?.Trim(),
            OwnerUserId = userId,
            IsDefault = false,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = userId.ToString(),
        };

        await dbContext.AddOrganizationAsync(organization, cancellationToken);

        var membership = new OrganizationMember
        {
            OrganizationId = organization.Id,
            UserId = userId,
            Role = OrganizationRole.Owner,
            JoinedAt = now,
            Status = MemberStatus.Active,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = userId.ToString(),
        };

        await dbContext.AddOrganizationMemberAsync(membership, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Created,
            nameof(Organization),
            organization.Id.ToString(),
            $"Organization '{organization.Name}' created",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return new OrganizationResponse
        {
            Id = organization.Id,
            Name = organization.Name,
            Slug = organization.Slug,
            Description = organization.Description,
            Logo = organization.Logo,
            OwnerUserId = organization.OwnerUserId,
            IsDefault = organization.IsDefault,
            IsActive = organization.IsActive,
            CreatedAt = organization.CreatedAt,
            CurrentUserRole = ApplicationConstants.ToRoleName(OrganizationRole.Owner),
        };
    }

    private async Task<string> CreateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = ApplicationConstants.CreateSlug(name);
        var slug = baseSlug;
        var counter = 1;

        while (await dbContext.Organizations.AnyAsync(o => o.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{++counter}";
        }

        return slug;
    }
}
