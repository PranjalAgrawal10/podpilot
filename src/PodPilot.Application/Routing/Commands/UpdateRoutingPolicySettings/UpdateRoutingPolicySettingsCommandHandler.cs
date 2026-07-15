using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.AiProviders;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Routing;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Routing.Commands.UpdateRoutingPolicySettings;

/// <summary>Handles intelligent routing policy settings updates.</summary>
public sealed class UpdateRoutingPolicySettingsCommandHandler
    : IRequestHandler<UpdateRoutingPolicySettingsCommand, RoutingPolicySettingsResponse>
{
    private const string DefaultPolicyName = "Organization Default";

    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;
    private readonly IRoutingNotificationService routingNotificationService;

    /// <summary>Initializes a new instance of the <see cref="UpdateRoutingPolicySettingsCommandHandler"/> class.</summary>
    public UpdateRoutingPolicySettingsCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService,
        IRoutingNotificationService routingNotificationService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
        this.routingNotificationService = routingNotificationService;
    }

    /// <inheritdoc />
    public async Task<RoutingPolicySettingsResponse> Handle(
        UpdateRoutingPolicySettingsCommand request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = RoutingAccess.RequireOrganizationContext(currentUserService);
        await RoutingAccess.EnsurePermissionAsync(
            organizationAuthorizationService, organizationId, userId, PermissionNames.RoutingManage, cancellationToken);

        if (request.PrimaryProviderId.HasValue)
        {
            _ = await AiProviderAccess.GetProviderAsync(
                dbContext, request.PrimaryProviderId.Value, organizationId, cancellationToken);
        }

        foreach (var fallbackId in request.FallbackProviderIds)
        {
            _ = await AiProviderAccess.GetProviderAsync(dbContext, fallbackId, organizationId, cancellationToken);
        }

        var now = dateTimeService.UtcNow;
        var policy = await dbContext.AiRoutingPolicies
            .FirstOrDefaultAsync(
                p => p.OrganizationId == organizationId && p.IsDefault,
                cancellationToken);

        if (policy is null)
        {
            policy = new AiRoutingPolicy
            {
                OrganizationId = organizationId,
                Name = DefaultPolicyName,
                IsDefault = true,
                IsEnabled = true,
                CreatedAt = now,
                CreatedBy = userId.ToString(),
            };
            await dbContext.AddAiRoutingPolicyAsync(policy, cancellationToken);
        }
        else
        {
            policy.UpdatedAt = now;
            policy.UpdatedBy = userId.ToString();
        }

        policy.Strategy = request.Strategy;
        policy.CostWeight = request.CostWeight;
        policy.LatencyWeight = request.LatencyWeight;
        policy.ReliabilityWeight = request.ReliabilityWeight;
        policy.ContextWeight = request.ContextWeight;
        policy.FeaturesWeight = request.FeaturesWeight;
        policy.AvailabilityWeight = request.AvailabilityWeight;
        policy.MaxRetries = request.MaxRetries;
        policy.FailoverStrategy = request.FailoverStrategy;
        policy.PrimaryProviderId = request.PrimaryProviderId;
        policy.FallbackProviderIdsJson = RoutingMapper.ToFallbackJson(request.FallbackProviderIds);
        policy.PreferredTaskTypesJson = RoutingMapper.ToPreferredTaskTypesJson(request.PreferredTaskTypes);
        policy.CustomRulesJson = string.IsNullOrWhiteSpace(request.CustomRulesJson)
            ? null
            : request.CustomRulesJson.Trim();
        policy.IsEnabled = true;
        policy.IsDefault = true;

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Updated,
            nameof(AiRoutingPolicy),
            policy.Id.ToString(),
            $"Intelligent routing policy updated to {policy.Strategy}",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        await routingNotificationService.NotifyPolicyUpdatedAsync(organizationId, policy.Id, cancellationToken);

        return RoutingMapper.ToPolicySettingsResponse(policy);
    }
}
