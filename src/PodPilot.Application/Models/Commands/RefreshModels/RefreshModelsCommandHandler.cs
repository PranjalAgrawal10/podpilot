using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models;
using PodPilot.Contracts.Models;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Models.Commands.RefreshModels;

/// <summary>
/// Handles refresh models commands.
/// </summary>
public sealed class RefreshModelsCommandHandler : IRequestHandler<RefreshModelsCommand, IReadOnlyList<ModelResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IModelService modelService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshModelsCommandHandler"/> class.
    /// </summary>
    public RefreshModelsCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IModelService modelService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.modelService = modelService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ModelResponse>> Handle(RefreshModelsCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ModelAccess.RequireOrganizationContext(currentUserService);
        await ModelAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ModelRead,
            cancellationToken);

        return await modelService.RefreshModelsAsync(
            organizationId,
            request.PodId,
            userId,
            cancellationToken);
    }
}
