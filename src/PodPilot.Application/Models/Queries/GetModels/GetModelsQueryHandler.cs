using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models;
using PodPilot.Contracts.Models;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Models.Queries.GetModels;

/// <summary>
/// Handles listing AI models.
/// </summary>
public sealed class GetModelsQueryHandler : IRequestHandler<GetModelsQuery, IReadOnlyList<ModelResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IModelRepository modelRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetModelsQueryHandler"/> class.
    /// </summary>
    public GetModelsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IModelRepository modelRepository)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.modelRepository = modelRepository;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ModelResponse>> Handle(GetModelsQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ModelAccess.RequireOrganizationContext(currentUserService);
        await ModelAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ModelRead,
            cancellationToken);

        var models = await modelRepository.ListAsync(organizationId, request.PodId, cancellationToken);
        return models.Select(ModelMapper.ToResponse).ToList();
    }
}
