using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.AiProviders.Commands.UpdateRoutingPolicy;

/// <summary>
/// Handles routing policy updates.
/// </summary>
public sealed class UpdateRoutingPolicyCommandHandler : IRequestHandler<UpdateRoutingPolicyCommand, AiRoutingPolicyResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoutingPolicyCommandHandler"/> class.
    /// </summary>
    public UpdateRoutingPolicyCommandHandler(
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
    public async Task<AiRoutingPolicyResponse> Handle(UpdateRoutingPolicyCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = AiProviderAccess.RequireOrganizationContext(currentUserService);
        await AiProviderAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.AiProviderUpdate,
            cancellationToken);

        var policy = await AiProviderAccess.GetRoutingPolicyAsync(dbContext, request.PolicyId, organizationId, cancellationToken);
        var normalizedName = request.Name.Trim();
        if (await dbContext.AiRoutingPolicies.AnyAsync(
                p => p.OrganizationId == organizationId && p.Name == normalizedName && p.Id != policy.Id,
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
                .Where(p => p.OrganizationId == organizationId && p.IsDefault && p.Id != policy.Id)
                .ToListAsync(cancellationToken);
            foreach (var existing in defaults)
            {
                existing.IsDefault = false;
            }
        }

        policy.Name = normalizedName;
        policy.ModelName = string.IsNullOrWhiteSpace(request.ModelName) ? null : request.ModelName.Trim();
        policy.PrimaryProviderId = request.PrimaryProviderId;
        policy.FallbackProviderIdsJson = AiProviderMapper.ToFallbackJson(request.FallbackProviderIds);
        policy.FailoverStrategy = request.FailoverStrategy;
        policy.Strategy = RoutingStrategy.ProviderPriority;
        policy.MaxRetries = request.MaxRetries;
        policy.IsEnabled = request.IsEnabled;
        policy.IsDefault = request.IsDefault;
        policy.UpdatedAt = dateTimeService.UtcNow;
        policy.UpdatedBy = userId.ToString();

        await dbContext.SaveChangesAsync(cancellationToken);
        policy = await AiProviderAccess.GetRoutingPolicyAsync(dbContext, policy.Id, organizationId, cancellationToken);

        await auditService.LogAsync(
            AuditAction.Updated,
            nameof(AiRoutingPolicy),
            policy.Id.ToString(),
            $"Routing policy '{policy.Name}' updated",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return AiProviderMapper.ToRoutingPolicyResponse(policy);
    }
}
