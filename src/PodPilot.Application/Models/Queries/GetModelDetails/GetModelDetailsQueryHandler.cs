using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models;
using PodPilot.Contracts.Models;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Models.Queries.GetModelDetails;

/// <summary>
/// Handles model detail queries.
/// </summary>
public sealed class GetModelDetailsQueryHandler : IRequestHandler<GetModelDetailsQuery, ModelDetailResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IModelRepository modelRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetModelDetailsQueryHandler"/> class.
    /// </summary>
    public GetModelDetailsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IModelRepository modelRepository)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.modelRepository = modelRepository;
    }

    /// <inheritdoc />
    public async Task<ModelDetailResponse> Handle(GetModelDetailsQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ModelAccess.RequireOrganizationContext(currentUserService);
        await ModelAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ModelRead,
            cancellationToken);

        var model = await ModelAccess.GetModelAsync(modelRepository, organizationId, request.ModelId, cancellationToken);
        var health = await modelRepository.ListHealthHistoryAsync(model.Id, 10, cancellationToken);

        return new ModelDetailResponse
        {
            Id = model.Id,
            OrganizationId = model.OrganizationId,
            PodId = model.PodId,
            PodName = model.Pod?.Name ?? string.Empty,
            Name = model.Name,
            Tag = model.Tag,
            FullName = model.FullName,
            Family = model.Family,
            Size = model.Size,
            Quantization = model.Quantization,
            ContextLength = model.ContextLength,
            Parameters = model.Parameters,
            License = model.License,
            IsDefault = model.IsDefault,
            Status = model.Status.ToString(),
            LastUsed = model.LastUsed,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            HealthHistory = health.Select(ModelMapper.ToHealthResponse).ToList(),
            Downloads = model.Downloads
                .OrderByDescending(d => d.StartedAt)
                .Take(10)
                .Select(ModelMapper.ToDownloadResponse)
                .ToList(),
        };
    }
}
