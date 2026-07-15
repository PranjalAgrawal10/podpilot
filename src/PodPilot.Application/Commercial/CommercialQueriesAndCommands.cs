using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Commercial;
using PodPilot.Contracts.Commercial;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using AppIssueLicense = PodPilot.Application.Models.Commercial.IssueLicenseRequest;
using AppStartCheckout = PodPilot.Application.Models.Commercial.StartCheckoutRequest;
using ContractsIssueLicense = PodPilot.Contracts.Commercial.IssueLicenseRequest;
using ContractsStartCheckout = PodPilot.Contracts.Commercial.StartCheckoutRequest;

namespace PodPilot.Application.Commercial;

/// <summary>Lists public subscription plans.</summary>
public sealed class ListPlansQuery : IRequest<IReadOnlyList<PlanResponse>>
{
}

/// <summary>Handles <see cref="ListPlansQuery"/>.</summary>
public sealed class ListPlansQueryHandler : IRequestHandler<ListPlansQuery, IReadOnlyList<PlanResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;
    private readonly ISubscriptionService subscriptionService;

    /// <summary>Initializes a new instance of the <see cref="ListPlansQueryHandler"/> class.</summary>
    public ListPlansQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db,
        ISubscriptionService subscriptionService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
        this.subscriptionService = subscriptionService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlanResponse>> Handle(ListPlansQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.BillingRead, cancellationToken);
        await subscriptionService.EnsureCatalogAsync(cancellationToken);

        var plans = await db.SubscriptionPlans.AsNoTracking()
            .Include(p => p.Quota)
            .Where(p => p.IsPublic)
            .OrderBy(p => p.Tier)
            .ToListAsync(cancellationToken);

        return plans.Select(p => CommercialMapper.ToPlanResponse(new PlanCatalogItem
        {
            Code = p.Code,
            Name = p.Name,
            Tier = p.Tier.ToString(),
            PricingModel = p.PricingModel.ToString(),
            MonthlyPriceUsd = p.MonthlyPriceUsd,
            YearlyPriceUsd = p.YearlyPriceUsd,
            SeatPriceUsd = p.SeatPriceUsd,
            IncludedSeats = p.IncludedSeats,
            Description = p.Description,
            Quotas = MapQuota(p.Quota),
        })).ToList();
    }

    private static QuotaLimits MapQuota(PlanQuota? quota) =>
        quota is null
            ? new QuotaLimits()
            : new QuotaLimits
            {
                MaxPods = quota.MaxPods,
                MaxProviders = quota.MaxProviders,
                MaxModels = quota.MaxModels,
                MaxOrganizations = quota.MaxOrganizations,
                MaxTeamMembers = quota.MaxTeamMembers,
                MaxApiRequestsPerMonth = quota.MaxApiRequestsPerMonth,
                MaxConcurrentStreams = quota.MaxConcurrentStreams,
                MaxStorageGb = quota.MaxStorageGb,
            };
}

/// <summary>Gets the organization subscription.</summary>
public sealed class GetSubscriptionQuery : IRequest<SubscriptionResponse>
{
}

/// <summary>Handles <see cref="GetSubscriptionQuery"/>.</summary>
public sealed class GetSubscriptionQueryHandler : IRequestHandler<GetSubscriptionQuery, SubscriptionResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly ISubscriptionService subscriptionService;

    /// <summary>Initializes a new instance of the <see cref="GetSubscriptionQueryHandler"/> class.</summary>
    public GetSubscriptionQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        ISubscriptionService subscriptionService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.subscriptionService = subscriptionService;
    }

    /// <inheritdoc />
    public async Task<SubscriptionResponse> Handle(GetSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.BillingRead, cancellationToken);
        var info = await subscriptionService.GetOrCreateAsync(orgId, cancellationToken);
        return CommercialMapper.ToSubscriptionResponse(info);
    }
}

/// <summary>Starts checkout for a plan upgrade.</summary>
public sealed class StartCheckoutCommand : IRequest<CheckoutSessionResponse>
{
    public ContractsStartCheckout Request { get; init; } = new();
}

/// <summary>Handles <see cref="StartCheckoutCommand"/>.</summary>
public sealed class StartCheckoutCommandHandler : IRequestHandler<StartCheckoutCommand, CheckoutSessionResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly ISubscriptionService subscriptionService;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>Initializes a new instance of the <see cref="StartCheckoutCommandHandler"/> class.</summary>
    public StartCheckoutCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        ISubscriptionService subscriptionService,
        IAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.subscriptionService = subscriptionService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<CheckoutSessionResponse> Handle(StartCheckoutCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.BillingManage, cancellationToken);

        if (!Enum.TryParse<BillingInterval>(request.Request.Interval, ignoreCase: true, out var interval))
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.Request.Interval), "Invalid billing interval."),
            ]);
        }

        if (!Enum.TryParse<PaymentProviderKind>(request.Request.Provider, ignoreCase: true, out var provider))
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.Request.Provider), "Invalid payment provider."),
            ]);
        }

        var appRequest = new AppStartCheckout
        {
            PlanCode = request.Request.PlanCode,
            Interval = interval,
            SeatCount = request.Request.SeatCount,
            Provider = provider,
            SuccessUrl = request.Request.SuccessUrl,
            CancelUrl = request.Request.CancelUrl,
        };

        var result = await subscriptionService.StartCheckoutAsync(orgId, appRequest, cancellationToken);
        await auditService.LogAsync(
            AuditAction.Created,
            nameof(OrganizationSubscription),
            orgId.ToString(),
            $"Checkout started for plan {appRequest.PlanCode}",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return CommercialMapper.ToCheckoutResponse(result);
    }
}

/// <summary>Cancels the organization subscription.</summary>
public sealed class CancelSubscriptionCommand : IRequest
{
    public bool AtPeriodEnd { get; init; } = true;
}

/// <summary>Handles <see cref="CancelSubscriptionCommand"/>.</summary>
public sealed class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly ISubscriptionService subscriptionService;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>Initializes a new instance of the <see cref="CancelSubscriptionCommandHandler"/> class.</summary>
    public CancelSubscriptionCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        ISubscriptionService subscriptionService,
        IAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.subscriptionService = subscriptionService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.BillingManage, cancellationToken);
        await subscriptionService.CancelAsync(orgId, request.AtPeriodEnd, cancellationToken);
        await auditService.LogAsync(
            AuditAction.Updated,
            nameof(OrganizationSubscription),
            orgId.ToString(),
            request.AtPeriodEnd ? "Subscription set to cancel at period end" : "Subscription canceled immediately",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);
    }
}

/// <summary>Gets usage dashboard.</summary>
public sealed class GetUsageQuery : IRequest<UsageResponse>
{
}

/// <summary>Handles <see cref="GetUsageQuery"/>.</summary>
public sealed class GetUsageQueryHandler : IRequestHandler<GetUsageQuery, UsageResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IUsageMeteringService usageMetering;

    /// <summary>Initializes a new instance of the <see cref="GetUsageQueryHandler"/> class.</summary>
    public GetUsageQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IUsageMeteringService usageMetering)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.usageMetering = usageMetering;
    }

    /// <inheritdoc />
    public async Task<UsageResponse> Handle(GetUsageQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.BillingRead, cancellationToken);
        var usage = await usageMetering.GetUsageAsync(orgId, cancellationToken);
        return CommercialMapper.ToUsageResponse(usage);
    }
}

/// <summary>Generates an invoice for the current period.</summary>
public sealed class GenerateInvoiceCommand : IRequest<InvoiceResponse>
{
}

/// <summary>Handles <see cref="GenerateInvoiceCommand"/>.</summary>
public sealed class GenerateInvoiceCommandHandler : IRequestHandler<GenerateInvoiceCommand, InvoiceResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IUsageMeteringService usageMetering;
    private readonly ICommercialNotificationService notifications;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>Initializes a new instance of the <see cref="GenerateInvoiceCommandHandler"/> class.</summary>
    public GenerateInvoiceCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IUsageMeteringService usageMetering,
        ICommercialNotificationService notifications,
        IAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.usageMetering = usageMetering;
        this.notifications = notifications;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<InvoiceResponse> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.BillingManage, cancellationToken);
        var invoice = await usageMetering.GenerateInvoiceAsync(orgId, cancellationToken: cancellationToken);
        await notifications.NotifyInvoiceGeneratedAsync(orgId, invoice.InvoiceNumber, cancellationToken);
        await auditService.LogAsync(
            AuditAction.Created,
            nameof(Invoice),
            invoice.Id.ToString(),
            $"Invoice {invoice.InvoiceNumber} generated",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);
        return CommercialMapper.ToInvoiceResponse(invoice);
    }
}

/// <summary>Lists invoices for the organization.</summary>
public sealed class ListInvoicesQuery : IRequest<IReadOnlyList<InvoiceResponse>>
{
}

/// <summary>Handles <see cref="ListInvoicesQuery"/>.</summary>
public sealed class ListInvoicesQueryHandler : IRequestHandler<ListInvoicesQuery, IReadOnlyList<InvoiceResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;

    /// <summary>Initializes a new instance of the <see cref="ListInvoicesQueryHandler"/> class.</summary>
    public ListInvoicesQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<InvoiceResponse>> Handle(ListInvoicesQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.BillingRead, cancellationToken);

        var invoices = await db.Invoices.AsNoTracking()
            .Where(i => i.OrganizationId == orgId)
            .OrderByDescending(i => i.PeriodEnd)
            .ToListAsync(cancellationToken);

        return invoices.Select(i => CommercialMapper.ToInvoiceResponse(new InvoiceInfo
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            Status = i.Status,
            SubtotalUsd = i.SubtotalUsd,
            TaxUsd = i.TaxUsd,
            TotalUsd = i.TotalUsd,
            PeriodStart = i.PeriodStart,
            PeriodEnd = i.PeriodEnd,
            LineItemsJson = i.LineItemsJson,
        })).ToList();
    }
}

/// <summary>Gets the organization license.</summary>
public sealed class GetLicenseQuery : IRequest<LicenseResponse>
{
}

/// <summary>Handles <see cref="GetLicenseQuery"/>.</summary>
public sealed class GetLicenseQueryHandler : IRequestHandler<GetLicenseQuery, LicenseResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly ILicenseService licenseService;

    /// <summary>Initializes a new instance of the <see cref="GetLicenseQueryHandler"/> class.</summary>
    public GetLicenseQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        ILicenseService licenseService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.licenseService = licenseService;
    }

    /// <inheritdoc />
    public async Task<LicenseResponse> Handle(GetLicenseQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.LicenseRead, cancellationToken);
        var license = await licenseService.ValidateAsync(orgId, cancellationToken);
        return CommercialMapper.ToLicenseResponse(license);
    }
}

/// <summary>Activates a license key.</summary>
public sealed class ActivateLicenseCommand : IRequest<LicenseResponse>
{
    public string LicenseKey { get; init; } = string.Empty;
}

/// <summary>Handles <see cref="ActivateLicenseCommand"/>.</summary>
public sealed class ActivateLicenseCommandHandler : IRequestHandler<ActivateLicenseCommand, LicenseResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly ILicenseService licenseService;
    private readonly ICommercialNotificationService notifications;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>Initializes a new instance of the <see cref="ActivateLicenseCommandHandler"/> class.</summary>
    public ActivateLicenseCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        ILicenseService licenseService,
        ICommercialNotificationService notifications,
        IAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.licenseService = licenseService;
        this.notifications = notifications;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<LicenseResponse> Handle(ActivateLicenseCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.LicenseManage, cancellationToken);
        var license = await licenseService.ActivateAsync(orgId, request.LicenseKey, cancellationToken);
        await notifications.NotifyLicenseUpdatedAsync(orgId, license.Edition.ToString(), cancellationToken);
        await auditService.LogAsync(
            AuditAction.Updated,
            nameof(ProductLicense),
            license.Id.ToString(),
            $"License activated ({license.Edition})",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);
        return CommercialMapper.ToLicenseResponse(license);
    }
}

/// <summary>Issues a new license key.</summary>
public sealed class IssueLicenseCommand : IRequest<IssuedLicenseResponse>
{
    public ContractsIssueLicense Request { get; init; } = new();
}

/// <summary>Handles <see cref="IssueLicenseCommand"/>.</summary>
public sealed class IssueLicenseCommandHandler : IRequestHandler<IssueLicenseCommand, IssuedLicenseResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly ILicenseService licenseService;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>Initializes a new instance of the <see cref="IssueLicenseCommandHandler"/> class.</summary>
    public IssueLicenseCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        ILicenseService licenseService,
        IAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.licenseService = licenseService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<IssuedLicenseResponse> Handle(IssueLicenseCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.LicenseManage, cancellationToken);

        if (!Enum.TryParse<LicenseEdition>(request.Request.Edition, ignoreCase: true, out var edition))
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.Request.Edition), "Invalid license edition."),
            ]);
        }

        if (!Enum.TryParse<LicenseDeploymentMode>(request.Request.DeploymentMode, ignoreCase: true, out var mode))
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.Request.DeploymentMode), "Invalid deployment mode."),
            ]);
        }

        var issued = await licenseService.IssueAsync(
            new AppIssueLicense
            {
                OrganizationId = request.Request.OrganizationId ?? orgId,
                Edition = edition,
                DeploymentMode = mode,
                MaxSeats = request.Request.MaxSeats,
                ExpiresAt = request.Request.ExpiresAt,
            },
            cancellationToken);

        await auditService.LogAsync(
            AuditAction.Created,
            nameof(ProductLicense),
            issued.Info.Id.ToString(),
            $"License issued ({edition})",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return CommercialMapper.ToIssuedLicenseResponse(issued);
    }
}

/// <summary>Gets onboarding status.</summary>
public sealed class GetOnboardingQuery : IRequest<OnboardingResponse>
{
}

/// <summary>Handles <see cref="GetOnboardingQuery"/>.</summary>
public sealed class GetOnboardingQueryHandler : IRequestHandler<GetOnboardingQuery, OnboardingResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IOnboardingService onboardingService;

    /// <summary>Initializes a new instance of the <see cref="GetOnboardingQueryHandler"/> class.</summary>
    public GetOnboardingQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IOnboardingService onboardingService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.onboardingService = onboardingService;
    }

    /// <inheritdoc />
    public async Task<OnboardingResponse> Handle(GetOnboardingQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.OrganizationRead, cancellationToken);
        var status = await onboardingService.GetAsync(orgId, cancellationToken);
        return CommercialMapper.ToOnboardingResponse(status);
    }
}

/// <summary>Completes an onboarding step.</summary>
public sealed class CompleteOnboardingStepCommand : IRequest<OnboardingResponse>
{
    public string Step { get; init; } = string.Empty;
}

/// <summary>Handles <see cref="CompleteOnboardingStepCommand"/>.</summary>
public sealed class CompleteOnboardingStepCommandHandler : IRequestHandler<CompleteOnboardingStepCommand, OnboardingResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IOnboardingService onboardingService;

    /// <summary>Initializes a new instance of the <see cref="CompleteOnboardingStepCommandHandler"/> class.</summary>
    public CompleteOnboardingStepCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IOnboardingService onboardingService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.onboardingService = onboardingService;
    }

    /// <inheritdoc />
    public async Task<OnboardingResponse> Handle(CompleteOnboardingStepCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.OrganizationUpdate, cancellationToken);

        if (!Enum.TryParse<OnboardingStep>(request.Step, ignoreCase: true, out var step))
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.Step), "Invalid onboarding step."),
            ]);
        }

        var status = await onboardingService.CompleteStepAsync(orgId, step, cancellationToken);
        return CommercialMapper.ToOnboardingResponse(status);
    }
}

/// <summary>Dismisses onboarding.</summary>
public sealed class DismissOnboardingCommand : IRequest
{
}

/// <summary>Handles <see cref="DismissOnboardingCommand"/>.</summary>
public sealed class DismissOnboardingCommandHandler : IRequestHandler<DismissOnboardingCommand>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IOnboardingService onboardingService;

    /// <summary>Initializes a new instance of the <see cref="DismissOnboardingCommandHandler"/> class.</summary>
    public DismissOnboardingCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IOnboardingService onboardingService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.onboardingService = onboardingService;
    }

    /// <inheritdoc />
    public async Task Handle(DismissOnboardingCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.OrganizationUpdate, cancellationToken);
        await onboardingService.DismissAsync(orgId, cancellationToken);
    }
}

/// <summary>Gets telemetry preference.</summary>
public sealed class GetTelemetryQuery : IRequest<TelemetryPreferenceResponse>
{
}

/// <summary>Handles <see cref="GetTelemetryQuery"/>.</summary>
public sealed class GetTelemetryQueryHandler : IRequestHandler<GetTelemetryQuery, TelemetryPreferenceResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly ITelemetryService telemetryService;

    /// <summary>Initializes a new instance of the <see cref="GetTelemetryQueryHandler"/> class.</summary>
    public GetTelemetryQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        ITelemetryService telemetryService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.telemetryService = telemetryService;
    }

    /// <inheritdoc />
    public async Task<TelemetryPreferenceResponse> Handle(GetTelemetryQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.OrganizationRead, cancellationToken);
        var preference = await telemetryService.GetPreferenceAsync(orgId, cancellationToken);
        return CommercialMapper.ToTelemetryResponse(preference);
    }
}

/// <summary>Updates telemetry preference.</summary>
public sealed class UpdateTelemetryCommand : IRequest<TelemetryPreferenceResponse>
{
    public TelemetryPreferenceResponse Preference { get; init; } = new();
}

/// <summary>Handles <see cref="UpdateTelemetryCommand"/>.</summary>
public sealed class UpdateTelemetryCommandHandler : IRequestHandler<UpdateTelemetryCommand, TelemetryPreferenceResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly ITelemetryService telemetryService;

    /// <summary>Initializes a new instance of the <see cref="UpdateTelemetryCommandHandler"/> class.</summary>
    public UpdateTelemetryCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        ITelemetryService telemetryService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.telemetryService = telemetryService;
    }

    /// <inheritdoc />
    public async Task<TelemetryPreferenceResponse> Handle(UpdateTelemetryCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.OrganizationUpdate, cancellationToken);
        var info = new TelemetryPreferenceInfo
        {
            OptIn = request.Preference.OptIn,
            CrashReports = request.Preference.CrashReports,
            PerformanceMetrics = request.Preference.PerformanceMetrics,
            FeatureUsage = request.Preference.FeatureUsage,
            HealthReports = request.Preference.HealthReports,
        };
        await telemetryService.UpdatePreferenceAsync(orgId, info, cancellationToken);
        return CommercialMapper.ToTelemetryResponse(info);
    }
}

/// <summary>Lists backup jobs.</summary>
public sealed class ListBackupsQuery : IRequest<IReadOnlyList<BackupJobResponse>>
{
}

/// <summary>Handles <see cref="ListBackupsQuery"/>.</summary>
public sealed class ListBackupsQueryHandler : IRequestHandler<ListBackupsQuery, IReadOnlyList<BackupJobResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IBackupService backupService;

    /// <summary>Initializes a new instance of the <see cref="ListBackupsQueryHandler"/> class.</summary>
    public ListBackupsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IBackupService backupService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.backupService = backupService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BackupJobResponse>> Handle(ListBackupsQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.BackupRead, cancellationToken);
        var jobs = await backupService.ListAsync(orgId, cancellationToken);
        return jobs.Select(CommercialMapper.ToBackupResponse).ToList();
    }
}

/// <summary>Starts a backup job.</summary>
public sealed class StartBackupCommand : IRequest<BackupJobResponse>
{
    public string BackupType { get; init; } = "Database";
    public bool Scheduled { get; init; }
}

/// <summary>Handles <see cref="StartBackupCommand"/>.</summary>
public sealed class StartBackupCommandHandler : IRequestHandler<StartBackupCommand, BackupJobResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IBackupService backupService;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>Initializes a new instance of the <see cref="StartBackupCommandHandler"/> class.</summary>
    public StartBackupCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IBackupService backupService,
        IAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.backupService = backupService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<BackupJobResponse> Handle(StartBackupCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.BackupManage, cancellationToken);
        var job = await backupService.StartAsync(orgId, request.BackupType, request.Scheduled, cancellationToken);
        await auditService.LogAsync(
            AuditAction.Created,
            nameof(BackupJob),
            job.Id.ToString(),
            $"Backup started ({request.BackupType})",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);
        return CommercialMapper.ToBackupResponse(job);
    }
}

/// <summary>Restores from a backup.</summary>
public sealed class RestoreBackupCommand : IRequest
{
    public Guid BackupJobId { get; init; }
}

/// <summary>Handles <see cref="RestoreBackupCommand"/>.</summary>
public sealed class RestoreBackupCommandHandler : IRequestHandler<RestoreBackupCommand>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;
    private readonly IBackupService backupService;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>Initializes a new instance of the <see cref="RestoreBackupCommandHandler"/> class.</summary>
    public RestoreBackupCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db,
        IBackupService backupService,
        IAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
        this.backupService = backupService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task Handle(RestoreBackupCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.BackupManage, cancellationToken);

        var job = await db.BackupJobs.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BackupJobId && b.OrganizationId == orgId, cancellationToken)
            ?? throw new NotFoundException("Backup job", request.BackupJobId);

        await backupService.RestoreAsync(job.Id, cancellationToken);
        await auditService.LogAsync(
            AuditAction.Updated,
            nameof(BackupJob),
            job.Id.ToString(),
            "Backup restore completed",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);
    }
}

/// <summary>Gets platform release status.</summary>
public sealed class GetReleaseStatusQuery : IRequest<ReleaseStatusResponse>
{
}

/// <summary>Handles <see cref="GetReleaseStatusQuery"/>.</summary>
public sealed class GetReleaseStatusQueryHandler : IRequestHandler<GetReleaseStatusQuery, ReleaseStatusResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IReleaseService releaseService;

    /// <summary>Initializes a new instance of the <see cref="GetReleaseStatusQueryHandler"/> class.</summary>
    public GetReleaseStatusQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IReleaseService releaseService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.releaseService = releaseService;
    }

    /// <inheritdoc />
    public async Task<ReleaseStatusResponse> Handle(GetReleaseStatusQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.OrganizationRead, cancellationToken);
        var status = await releaseService.GetStatusAsync(cancellationToken);
        return CommercialMapper.ToReleaseResponse(status);
    }
}

/// <summary>Gets commercial dashboard.</summary>
public sealed class GetCommercialDashboardQuery : IRequest<CommercialDashboardResponse>
{
}

/// <summary>Handles <see cref="GetCommercialDashboardQuery"/>.</summary>
public sealed class GetCommercialDashboardQueryHandler : IRequestHandler<GetCommercialDashboardQuery, CommercialDashboardResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly ICommercialDashboardService dashboardService;

    /// <summary>Initializes a new instance of the <see cref="GetCommercialDashboardQueryHandler"/> class.</summary>
    public GetCommercialDashboardQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        ICommercialDashboardService dashboardService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.dashboardService = dashboardService;
    }

    /// <inheritdoc />
    public async Task<CommercialDashboardResponse> Handle(GetCommercialDashboardQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = CommercialAccess.RequireOrganizationContext(currentUserService);
        await CommercialAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.BillingRead, cancellationToken);
        var dashboard = await dashboardService.GetAsync(orgId, cancellationToken);
        return CommercialMapper.ToDashboardResponse(dashboard);
    }
}

/// <summary>Gets anonymous system status.</summary>
public sealed class GetSystemStatusQuery : IRequest<SystemStatusResponse>
{
}

/// <summary>Handles <see cref="GetSystemStatusQuery"/>.</summary>
public sealed class GetSystemStatusQueryHandler : IRequestHandler<GetSystemStatusQuery, SystemStatusResponse>
{
    private readonly IReleaseService releaseService;
    private readonly IApplicationDbContext db;

    /// <summary>Initializes a new instance of the <see cref="GetSystemStatusQueryHandler"/> class.</summary>
    public GetSystemStatusQueryHandler(IReleaseService releaseService, IApplicationDbContext db)
    {
        this.releaseService = releaseService;
        this.db = db;
    }

    /// <inheritdoc />
    public async Task<SystemStatusResponse> Handle(GetSystemStatusQuery request, CancellationToken cancellationToken)
    {
        var release = await releaseService.GetStatusAsync(cancellationToken);
        var canQuery = true;
        try
        {
            _ = await db.SubscriptionPlans.AsNoTracking().Take(1).CountAsync(cancellationToken);
        }
        catch
        {
            canQuery = false;
        }

        return new SystemStatusResponse
        {
            Status = canQuery ? "Operational" : "Degraded",
            Version = release.CurrentVersion,
            UpdateAvailable = release.UpdateAvailable,
            Components =
            [
                new SystemComponentStatus { Name = "API", Status = "Operational" },
                new SystemComponentStatus { Name = "Database", Status = canQuery ? "Operational" : "Unavailable" },
                new SystemComponentStatus { Name = "Billing", Status = canQuery ? "Operational" : "Unavailable" },
            ],
        };
    }
}

/// <summary>Handles Stripe webhook payload.</summary>
public sealed class HandleStripeWebhookCommand : IRequest
{
    public string Payload { get; init; } = string.Empty;
    public string? SignatureHeader { get; init; }
}

/// <summary>Handles <see cref="HandleStripeWebhookCommand"/>.</summary>
public sealed class HandleStripeWebhookCommandHandler : IRequestHandler<HandleStripeWebhookCommand>
{
    private readonly IPaymentGatewayFactory gatewayFactory;

    /// <summary>Initializes a new instance of the <see cref="HandleStripeWebhookCommandHandler"/> class.</summary>
    public HandleStripeWebhookCommandHandler(IPaymentGatewayFactory gatewayFactory) =>
        this.gatewayFactory = gatewayFactory;

    /// <inheritdoc />
    public Task Handle(HandleStripeWebhookCommand request, CancellationToken cancellationToken) =>
        gatewayFactory.Get(PaymentProviderKind.Stripe)
            .HandleWebhookAsync(request.Payload, request.SignatureHeader, cancellationToken);
}

/// <summary>Handles Razorpay webhook payload.</summary>
public sealed class HandleRazorpayWebhookCommand : IRequest
{
    public string Payload { get; init; } = string.Empty;
    public string? SignatureHeader { get; init; }
}

/// <summary>Handles <see cref="HandleRazorpayWebhookCommand"/>.</summary>
public sealed class HandleRazorpayWebhookCommandHandler : IRequestHandler<HandleRazorpayWebhookCommand>
{
    private readonly IPaymentGatewayFactory gatewayFactory;

    /// <summary>Initializes a new instance of the <see cref="HandleRazorpayWebhookCommandHandler"/> class.</summary>
    public HandleRazorpayWebhookCommandHandler(IPaymentGatewayFactory gatewayFactory) =>
        this.gatewayFactory = gatewayFactory;

    /// <inheritdoc />
    public Task Handle(HandleRazorpayWebhookCommand request, CancellationToken cancellationToken) =>
        gatewayFactory.Get(PaymentProviderKind.Razorpay)
            .HandleWebhookAsync(request.Payload, request.SignatureHeader, cancellationToken);
}
