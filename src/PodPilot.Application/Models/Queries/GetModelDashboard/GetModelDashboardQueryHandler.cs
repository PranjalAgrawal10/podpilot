using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models;
using PodPilot.Contracts.Models;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Queries.GetModelDashboard;

/// <summary>
/// Handles model dashboard queries.
/// </summary>
public sealed class GetModelDashboardQueryHandler : IRequestHandler<GetModelDashboardQuery, ModelDashboardResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IModelRepository modelRepository;
    private readonly IApplicationDbContext dbContext;
    private readonly IModelService modelService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetModelDashboardQueryHandler"/> class.
    /// </summary>
    public GetModelDashboardQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IModelRepository modelRepository,
        IApplicationDbContext dbContext,
        IModelService modelService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.modelRepository = modelRepository;
        this.dbContext = dbContext;
        this.modelService = modelService;
    }

    /// <inheritdoc />
    public async Task<ModelDashboardResponse> Handle(GetModelDashboardQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ModelAccess.RequireOrganizationContext(currentUserService);
        await ModelAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ModelRead,
            cancellationToken);

        var models = await modelRepository.ListAsync(organizationId, request.PodId, cancellationToken);
        var activeDownloads = await modelRepository.ListActiveDownloadsAsync(organizationId, cancellationToken);
        var filteredDownloads = request.PodId.HasValue
            ? activeDownloads.Where(d => d.Model.PodId == request.PodId.Value).ToList()
            : activeDownloads;

        var latestHealth = await dbContext.ModelHealthHistory
            .Include(h => h.Model)
            .Where(h => h.Model.OrganizationId == organizationId
                && (!request.PodId.HasValue || h.Model.PodId == request.PodId.Value))
            .GroupBy(h => h.ModelId)
            .Select(g => g.OrderByDescending(x => x.LastChecked).First())
            .ToListAsync(cancellationToken);

        var defaultModel = models.FirstOrDefault(m => m.IsDefault)?.FullName;
        var (ollamaDetected, ollamaVersion) = request.PodId.HasValue
            ? await modelService.TryDetectOllamaAsync(organizationId, request.PodId.Value, cancellationToken)
            : (false, null);

        return new ModelDashboardResponse
        {
            InstalledModels = models.Count(m => m.Status == ModelStatus.Available),
            DownloadingModels = filteredDownloads.Count,
            DefaultModel = defaultModel,
            StorageUsedBytes = models.Where(m => m.Status == ModelStatus.Available).Sum(m => m.Size),
            OllamaVersion = ollamaVersion,
            OllamaDetected = ollamaDetected,
            HealthyModels = latestHealth.Count(h => h.Status == ModelHealthStatus.Healthy),
            UnhealthyModels = latestHealth.Count(h => h.Status != ModelHealthStatus.Healthy),
        };
    }
}
