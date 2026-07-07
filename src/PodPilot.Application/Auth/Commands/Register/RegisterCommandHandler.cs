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
    private readonly IJwtTokenService jwtTokenService;
    private readonly IRefreshTokenService refreshTokenService;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterCommandHandler"/> class.
    /// </summary>
    public RegisterCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext dbContext,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.identityService = identityService;
        this.dbContext = dbContext;
        this.jwtTokenService = jwtTokenService;
        this.refreshTokenService = refreshTokenService;
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

        var slug = ApplicationConstants.CreateSlug(request.OrganizationName);
        var organization = new Organization
        {
            Name = request.OrganizationName.Trim(),
            Slug = slug,
            CreatedAt = dateTimeService.UtcNow,
            CreatedBy = user.Id.ToString(),
        };

        await dbContext.AddOrganizationAsync(organization, cancellationToken);

        var membership = new OrganizationMember
        {
            OrganizationId = organization.Id,
            UserId = user.Id,
            Role = UserRole.Admin,
            CreatedAt = dateTimeService.UtcNow,
            CreatedBy = user.Id.ToString(),
        };

        await dbContext.AddOrganizationMemberAsync(membership, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var roles = await identityService.GetUserRolesAsync(user.Id, cancellationToken);
        var (accessToken, expiresIn) = jwtTokenService.GenerateAccessToken(user, roles);
        var (_, refreshToken) = await refreshTokenService.GenerateRefreshTokenAsync(
            user.Id,
            httpContextService.IpAddress,
            cancellationToken);

        await auditService.LogAsync(
            AuditAction.Register,
            nameof(User),
            user.Id.ToString(),
            $"User registered with organization '{organization.Name}'",
            user.Id,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn,
            User = new UserSummary
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles,
            },
        };
    }
}
