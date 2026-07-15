using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.AiProviders.Commands.CreateRoutingPolicy;

/// <summary>
/// Handles routing policy creation.
/// </summary>
public sealed class CreateRoutingPolicyCommandHandler : IRequestHandler<CreateRoutingPolicyCommand, AiRoutingPolicyResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateRoutingPolicyCommandHandler"/> class.
    /// </summary>
    public CreateRoutingPolicyCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<AiRoutingPolicyResponse> Handle(CreateRoutingPolicyCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = AiProviderAccess.RequireOrganizationContext(currentUserService);
        await AiProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.AiProviderCreate,
            cancellationToken);

        var normalizedName = request.Name.Trim();
        if (await dbContext.AiRoutingPolicies.AnyAsync(
                p => p.OrganizationId == organizationId && p.Name == normalizedName,
                cancellationToken))
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.Name), "A routing policy with this name already exists."),
            ]);
        }

        _ = await AiProviderAccess.GetProviderAsync(dbContext, request.PrimaryProviderId, organizationId, cancellationToken);

        foreach (var fallbackId in request.FallbackProviderIds)
        {
            _ = await AiProviderAccess.GetProviderAsync(dbContext, fallbackId, organizationId, cancellationToken);
        }

        if (request.IsDefault)
        {
            var defaults = await dbContext.AiRoutingPolicies
                .Where(p => p.OrganizationId == organizationId && p.IsDefault)
                .ToListAsync(cancellationToken);
            foreach (var existing in defaults)
            {
                existing.IsDefault = false;
            }
        }

        var now = dateTimeService.UtcNow;
        var policy = new AiRoutingPolicy
        {
            OrganizationId = organizationId,
            Name = normalizedName,
            ModelName = string.IsNullOrWhiteSpace(request.ModelName) ? null : request.ModelName.Trim(),
            PrimaryProviderId = request.PrimaryProviderId,
            FallbackProviderIdsJson = AiProviderMapper.ToFallbackJson(request.FallbackProviderIds),
            FailoverStrategy = request.FailoverStrategy,
            Strategy = RoutingStrategy.ProviderPriority,
            MaxRetries = request.MaxRetries,
            IsEnabled = request.IsEnabled,
            IsDefault = request.IsDefault,
            CreatedAt = now,
            CreatedBy = userId.ToString(),
        };

        await dbContext.AddAiRoutingPolicyAsync(policy, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        policy = await AiProviderAccess.GetRoutingPolicyAsync(dbContext, policy.Id, organizationId, cancellationToken);

        await auditService.LogAsync(
            AuditAction.Created,
            nameof(AiRoutingPolicy),
            policy.Id.ToString(),
            $"Routing policy '{policy.Name}' created",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return AiProviderMapper.ToRoutingPolicyResponse(policy);
    }
}
