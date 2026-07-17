using FluentValidation;
using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Deployments;
using PodPilot.Contracts.Deployments;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Deployments;

/// <summary>Creates a one-click deployment.</summary>
public sealed class CreateDeploymentCommand : IRequest<DeploymentResponse>
{
    /// <summary>Gets or sets the request payload.</summary>
    public CreateDeploymentRequest Request { get; init; } = new();
}

/// <summary>Validates <see cref="CreateDeploymentCommand"/>.</summary>
public sealed class CreateDeploymentCommandValidator : AbstractValidator<CreateDeploymentCommand>
{
    /// <summary>Initializes a new instance of the <see cref="CreateDeploymentCommandValidator"/> class.</summary>
    public CreateDeploymentCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.ProviderId).NotEmpty();
        RuleFor(x => x.Request.Region).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Request.GpuCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Request.Models).NotEmpty().WithMessage("At least one model is required.");
        RuleForEach(x => x.Request.Models).NotEmpty().MaximumLength(200);
    }
}

/// <summary>Handles <see cref="CreateDeploymentCommand"/>.</summary>
public sealed class CreateDeploymentCommandHandler : IRequestHandler<CreateDeploymentCommand, DeploymentResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService authorizationService;
    private readonly IDeploymentService deploymentService;

    /// <summary>Initializes a new instance of the <see cref="CreateDeploymentCommandHandler"/> class.</summary>
    public CreateDeploymentCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService authorizationService,
        IDeploymentService deploymentService)
    {
        this.currentUserService = currentUserService;
        this.authorizationService = authorizationService;
        this.deploymentService = deploymentService;
    }

    /// <inheritdoc />
    public async Task<DeploymentResponse> Handle(CreateDeploymentCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = DeploymentAccess.RequireOrganizationContext(currentUserService);
        await DeploymentAccess.EnsurePermissionAsync(
            authorizationService,
            organizationId,
            userId,
            PermissionNames.DeploymentManage,
            cancellationToken);

        if (!Enum.TryParse<InferenceRuntimeKind>(request.Request.Runtime, true, out var runtime))
        {
            runtime = InferenceRuntimeKind.Ollama;
        }

        var detail = await deploymentService.CreateAsync(
            new CreateDeploymentOptions
            {
                OrganizationId = organizationId,
                UserId = userId,
                Name = request.Request.Name.Trim(),
                ProviderId = request.Request.ProviderId,
                Region = request.Request.Region.Trim(),
                GpuCode = request.Request.GpuCode.Trim(),
                ProviderGpuId = request.Request.ProviderGpuId,
                Runtime = runtime,
                Models = request.Request.Models,
                TemplateCode = request.Request.TemplateCode,
                EnvironmentVariables = request.Request.EnvironmentVariables,
            },
            cancellationToken);

        return DeploymentMapper.ToResponse(detail);
    }
}

/// <summary>Deletes a deployment.</summary>
public sealed class DeleteDeploymentCommand : IRequest
{
    /// <summary>Gets or sets deployment id.</summary>
    public Guid DeploymentId { get; init; }
}

/// <summary>Handles <see cref="DeleteDeploymentCommand"/>.</summary>
public sealed class DeleteDeploymentCommandHandler : IRequestHandler<DeleteDeploymentCommand>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService authorizationService;
    private readonly IDeploymentService deploymentService;

    /// <summary>Initializes a new instance of the <see cref="DeleteDeploymentCommandHandler"/> class.</summary>
    public DeleteDeploymentCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService authorizationService,
        IDeploymentService deploymentService)
    {
        this.currentUserService = currentUserService;
        this.authorizationService = authorizationService;
        this.deploymentService = deploymentService;
    }

    /// <inheritdoc />
    public async Task Handle(DeleteDeploymentCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = DeploymentAccess.RequireOrganizationContext(currentUserService);
        await DeploymentAccess.EnsurePermissionAsync(
            authorizationService,
            organizationId,
            userId,
            PermissionNames.DeploymentManage,
            cancellationToken);
        await deploymentService.DeleteAsync(organizationId, request.DeploymentId, userId, cancellationToken);
    }
}

/// <summary>Restarts a deployment.</summary>
public sealed class RestartDeploymentCommand : IRequest<DeploymentResponse>
{
    /// <summary>Gets or sets deployment id.</summary>
    public Guid DeploymentId { get; init; }
}

/// <summary>Handles <see cref="RestartDeploymentCommand"/>.</summary>
public sealed class RestartDeploymentCommandHandler : IRequestHandler<RestartDeploymentCommand, DeploymentResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService authorizationService;
    private readonly IDeploymentService deploymentService;

    /// <summary>Initializes a new instance of the <see cref="RestartDeploymentCommandHandler"/> class.</summary>
    public RestartDeploymentCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService authorizationService,
        IDeploymentService deploymentService)
    {
        this.currentUserService = currentUserService;
        this.authorizationService = authorizationService;
        this.deploymentService = deploymentService;
    }

    /// <inheritdoc />
    public async Task<DeploymentResponse> Handle(RestartDeploymentCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = DeploymentAccess.RequireOrganizationContext(currentUserService);
        await DeploymentAccess.EnsurePermissionAsync(
            authorizationService,
            organizationId,
            userId,
            PermissionNames.DeploymentManage,
            cancellationToken);
        var detail = await deploymentService.RestartAsync(organizationId, request.DeploymentId, userId, cancellationToken);
        return DeploymentMapper.ToResponse(detail);
    }
}

/// <summary>Runs health check.</summary>
public sealed class RunDeploymentHealthCommand : IRequest<DeploymentHealthResponse>
{
    /// <summary>Gets or sets deployment id.</summary>
    public Guid DeploymentId { get; init; }
}

/// <summary>Handles <see cref="RunDeploymentHealthCommand"/>.</summary>
public sealed class RunDeploymentHealthCommandHandler : IRequestHandler<RunDeploymentHealthCommand, DeploymentHealthResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService authorizationService;
    private readonly IDeploymentService deploymentService;

    /// <summary>Initializes a new instance of the <see cref="RunDeploymentHealthCommandHandler"/> class.</summary>
    public RunDeploymentHealthCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService authorizationService,
        IDeploymentService deploymentService)
    {
        this.currentUserService = currentUserService;
        this.authorizationService = authorizationService;
        this.deploymentService = deploymentService;
    }

    /// <inheritdoc />
    public async Task<DeploymentHealthResponse> Handle(RunDeploymentHealthCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = DeploymentAccess.RequireOrganizationContext(currentUserService);
        await DeploymentAccess.EnsurePermissionAsync(
            authorizationService,
            organizationId,
            userId,
            PermissionNames.DeploymentRead,
            cancellationToken);
        var health = await deploymentService.RunHealthCheckAsync(organizationId, request.DeploymentId, cancellationToken);
        return DeploymentMapper.ToHealthResponse(health);
    }
}

/// <summary>Lists deployments.</summary>
public sealed class ListDeploymentsQuery : IRequest<IReadOnlyList<DeploymentResponse>>
{
}

/// <summary>Handles <see cref="ListDeploymentsQuery"/>.</summary>
public sealed class ListDeploymentsQueryHandler : IRequestHandler<ListDeploymentsQuery, IReadOnlyList<DeploymentResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService authorizationService;
    private readonly IDeploymentService deploymentService;

    /// <summary>Initializes a new instance of the <see cref="ListDeploymentsQueryHandler"/> class.</summary>
    public ListDeploymentsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService authorizationService,
        IDeploymentService deploymentService)
    {
        this.currentUserService = currentUserService;
        this.authorizationService = authorizationService;
        this.deploymentService = deploymentService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DeploymentResponse>> Handle(ListDeploymentsQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = DeploymentAccess.RequireOrganizationContext(currentUserService);
        await DeploymentAccess.EnsurePermissionAsync(
            authorizationService,
            organizationId,
            userId,
            PermissionNames.DeploymentRead,
            cancellationToken);
        var items = await deploymentService.ListAsync(organizationId, cancellationToken);
        return items.Select(DeploymentMapper.ToResponse).ToList();
    }
}

/// <summary>Gets a deployment.</summary>
public sealed class GetDeploymentQuery : IRequest<DeploymentResponse>
{
    /// <summary>Gets or sets deployment id.</summary>
    public Guid DeploymentId { get; init; }
}

/// <summary>Handles <see cref="GetDeploymentQuery"/>.</summary>
public sealed class GetDeploymentQueryHandler : IRequestHandler<GetDeploymentQuery, DeploymentResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService authorizationService;
    private readonly IDeploymentService deploymentService;

    /// <summary>Initializes a new instance of the <see cref="GetDeploymentQueryHandler"/> class.</summary>
    public GetDeploymentQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService authorizationService,
        IDeploymentService deploymentService)
    {
        this.currentUserService = currentUserService;
        this.authorizationService = authorizationService;
        this.deploymentService = deploymentService;
    }

    /// <inheritdoc />
    public async Task<DeploymentResponse> Handle(GetDeploymentQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = DeploymentAccess.RequireOrganizationContext(currentUserService);
        await DeploymentAccess.EnsurePermissionAsync(
            authorizationService,
            organizationId,
            userId,
            PermissionNames.DeploymentRead,
            cancellationToken);
        var detail = await deploymentService.GetAsync(organizationId, request.DeploymentId, cancellationToken);
        return DeploymentMapper.ToResponse(detail);
    }
}

/// <summary>Lists GPU catalog.</summary>
public sealed class ListGpuCatalogQuery : IRequest<IReadOnlyList<GpuCatalogResponse>>
{
}

/// <summary>Handles <see cref="ListGpuCatalogQuery"/>.</summary>
public sealed class ListGpuCatalogQueryHandler : IRequestHandler<ListGpuCatalogQuery, IReadOnlyList<GpuCatalogResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService authorizationService;
    private readonly IDeploymentCatalogService catalogService;

    /// <summary>Initializes a new instance of the <see cref="ListGpuCatalogQueryHandler"/> class.</summary>
    public ListGpuCatalogQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService authorizationService,
        IDeploymentCatalogService catalogService)
    {
        this.currentUserService = currentUserService;
        this.authorizationService = authorizationService;
        this.catalogService = catalogService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GpuCatalogResponse>> Handle(ListGpuCatalogQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = DeploymentAccess.RequireOrganizationContext(currentUserService);
        await DeploymentAccess.EnsurePermissionAsync(
            authorizationService,
            organizationId,
            userId,
            PermissionNames.DeploymentRead,
            cancellationToken);
        await catalogService.EnsureSeededAsync(cancellationToken);
        var items = await catalogService.ListGpusAsync(cancellationToken);
        return items.Select(DeploymentMapper.ToGpuResponse).ToList();
    }
}

/// <summary>Lists model catalog.</summary>
public sealed class ListModelCatalogQuery : IRequest<IReadOnlyList<ModelCatalogResponse>>
{
}

/// <summary>Handles <see cref="ListModelCatalogQuery"/>.</summary>
public sealed class ListModelCatalogQueryHandler : IRequestHandler<ListModelCatalogQuery, IReadOnlyList<ModelCatalogResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService authorizationService;
    private readonly IDeploymentCatalogService catalogService;

    /// <summary>Initializes a new instance of the <see cref="ListModelCatalogQueryHandler"/> class.</summary>
    public ListModelCatalogQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService authorizationService,
        IDeploymentCatalogService catalogService)
    {
        this.currentUserService = currentUserService;
        this.authorizationService = authorizationService;
        this.catalogService = catalogService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ModelCatalogResponse>> Handle(ListModelCatalogQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = DeploymentAccess.RequireOrganizationContext(currentUserService);
        await DeploymentAccess.EnsurePermissionAsync(
            authorizationService,
            organizationId,
            userId,
            PermissionNames.DeploymentRead,
            cancellationToken);
        await catalogService.EnsureSeededAsync(cancellationToken);
        var items = await catalogService.ListModelsAsync(cancellationToken);
        return items.Select(DeploymentMapper.ToModelResponse).ToList();
    }
}

/// <summary>Lists templates.</summary>
public sealed class ListDeploymentTemplatesQuery : IRequest<IReadOnlyList<DeploymentTemplateResponse>>
{
}

/// <summary>Handles <see cref="ListDeploymentTemplatesQuery"/>.</summary>
public sealed class ListDeploymentTemplatesQueryHandler
    : IRequestHandler<ListDeploymentTemplatesQuery, IReadOnlyList<DeploymentTemplateResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService authorizationService;
    private readonly IDeploymentCatalogService catalogService;

    /// <summary>Initializes a new instance of the <see cref="ListDeploymentTemplatesQueryHandler"/> class.</summary>
    public ListDeploymentTemplatesQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService authorizationService,
        IDeploymentCatalogService catalogService)
    {
        this.currentUserService = currentUserService;
        this.authorizationService = authorizationService;
        this.catalogService = catalogService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DeploymentTemplateResponse>> Handle(
        ListDeploymentTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = DeploymentAccess.RequireOrganizationContext(currentUserService);
        await DeploymentAccess.EnsurePermissionAsync(
            authorizationService,
            organizationId,
            userId,
            PermissionNames.DeploymentRead,
            cancellationToken);
        await catalogService.EnsureSeededAsync(cancellationToken);
        var items = await catalogService.ListTemplatesAsync(cancellationToken);
        return items.Select(DeploymentMapper.ToTemplateResponse).ToList();
    }
}

/// <summary>Lists regions.</summary>
public sealed class ListDeploymentRegionsQuery : IRequest<IReadOnlyList<DeploymentRegionResponse>>
{
    /// <summary>Gets or sets provider id.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets sort by (latency|price|availability).</summary>
    public string? SortBy { get; init; }
}

/// <summary>Handles <see cref="ListDeploymentRegionsQuery"/>.</summary>
public sealed class ListDeploymentRegionsQueryHandler
    : IRequestHandler<ListDeploymentRegionsQuery, IReadOnlyList<DeploymentRegionResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService authorizationService;
    private readonly IDeploymentCatalogService catalogService;

    /// <summary>Initializes a new instance of the <see cref="ListDeploymentRegionsQueryHandler"/> class.</summary>
    public ListDeploymentRegionsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService authorizationService,
        IDeploymentCatalogService catalogService)
    {
        this.currentUserService = currentUserService;
        this.authorizationService = authorizationService;
        this.catalogService = catalogService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DeploymentRegionResponse>> Handle(
        ListDeploymentRegionsQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = DeploymentAccess.RequireOrganizationContext(currentUserService);
        await DeploymentAccess.EnsurePermissionAsync(
            authorizationService,
            organizationId,
            userId,
            PermissionNames.DeploymentRead,
            cancellationToken);
        var items = await catalogService.ListRegionsAsync(
            organizationId,
            request.ProviderId,
            request.SortBy,
            cancellationToken);
        return items.Select(DeploymentMapper.ToRegionResponse).ToList();
    }
}

/// <summary>Recommends GPUs.</summary>
public sealed class RecommendGpuQuery : IRequest<GpuRecommendationResponse>
{
    /// <summary>Gets or sets model codes or references.</summary>
    public IReadOnlyList<string> Models { get; init; } = [];
}

/// <summary>Handles <see cref="RecommendGpuQuery"/>.</summary>
public sealed class RecommendGpuQueryHandler : IRequestHandler<RecommendGpuQuery, GpuRecommendationResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService authorizationService;
    private readonly IDeploymentCatalogService catalogService;

    /// <summary>Initializes a new instance of the <see cref="RecommendGpuQueryHandler"/> class.</summary>
    public RecommendGpuQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService authorizationService,
        IDeploymentCatalogService catalogService)
    {
        this.currentUserService = currentUserService;
        this.authorizationService = authorizationService;
        this.catalogService = catalogService;
    }

    /// <inheritdoc />
    public async Task<GpuRecommendationResponse> Handle(RecommendGpuQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = DeploymentAccess.RequireOrganizationContext(currentUserService);
        await DeploymentAccess.EnsurePermissionAsync(
            authorizationService,
            organizationId,
            userId,
            PermissionNames.DeploymentRead,
            cancellationToken);
        await catalogService.EnsureSeededAsync(cancellationToken);
        var result = await catalogService.RecommendGpuAsync(request.Models, cancellationToken);
        return DeploymentMapper.ToRecommendationResponse(result);
    }
}

/// <summary>Gets deployment dashboard.</summary>
public sealed class GetDeploymentDashboardQuery : IRequest<DeploymentDashboardResponse>
{
}

/// <summary>Handles <see cref="GetDeploymentDashboardQuery"/>.</summary>
public sealed class GetDeploymentDashboardQueryHandler
    : IRequestHandler<GetDeploymentDashboardQuery, DeploymentDashboardResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService authorizationService;
    private readonly IDeploymentService deploymentService;

    /// <summary>Initializes a new instance of the <see cref="GetDeploymentDashboardQueryHandler"/> class.</summary>
    public GetDeploymentDashboardQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService authorizationService,
        IDeploymentService deploymentService)
    {
        this.currentUserService = currentUserService;
        this.authorizationService = authorizationService;
        this.deploymentService = deploymentService;
    }

    /// <inheritdoc />
    public async Task<DeploymentDashboardResponse> Handle(
        GetDeploymentDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = DeploymentAccess.RequireOrganizationContext(currentUserService);
        await DeploymentAccess.EnsurePermissionAsync(
            authorizationService,
            organizationId,
            userId,
            PermissionNames.DeploymentRead,
            cancellationToken);
        var info = await deploymentService.GetDashboardAsync(organizationId, cancellationToken);
        return DeploymentMapper.ToDashboardResponse(info);
    }
}
