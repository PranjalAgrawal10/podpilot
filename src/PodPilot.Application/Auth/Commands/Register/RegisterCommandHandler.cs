using MediatR;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Auth;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Auth.Commands.Register;

/// <summary>
/// Handles user registration.
/// </summary>
public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IIdentityService identityService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuthTokenIssuer authTokenIssuer;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterCommandHandler"/> class.
    /// </summary>
    public RegisterCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext dbContext,
        IAuthTokenIssuer authTokenIssuer,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.identityService = identityService;
        this.dbContext = dbContext;
        this.authTokenIssuer = authTokenIssuer;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new ValidationException("A user with this email already exists.");
        }

        var (user, errors) = await identityService.CreateUserAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            cancellationToken);

        if (user is null)
        {
            throw new ValidationException(string.Join("; ", errors));
        }

        await identityService.AssignRoleAsync(user.Id, ApplicationConstants.AdminRole, cancellationToken);

        var now = dateTimeService.UtcNow;
        var slug = ApplicationConstants.CreateSlug(request.OrganizationName);
        var organization = new Organization
        {
            Name = request.OrganizationName.Trim(),
            Slug = slug,
            OwnerUserId = user.Id,
            IsDefault = true,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = user.Id.ToString(),
        };

        await dbContext.AddOrganizationAsync(organization, cancellationToken);

        var membership = new OrganizationMember
        {
            OrganizationId = organization.Id,
            UserId = user.Id,
            Role = OrganizationRole.Owner,
            JoinedAt = now,
            Status = MemberStatus.Active,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = user.Id.ToString(),
        };

        await dbContext.AddOrganizationMemberAsync(membership, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Register,
            nameof(User),
            user.Id.ToString(),
            $"User registered with organization '{organization.Name}'",
            user.Id,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return await authTokenIssuer.IssueTokensAsync(user.Id, organization.Id, cancellationToken);
    }
}
