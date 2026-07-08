using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models;
using PodPilot.Contracts.Models;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Models.Commands.PullModel;

/// <summary>
/// Handles model pull commands.
/// </summary>
public sealed class PullModelCommandHandler : IRequestHandler<PullModelCommand, ModelDownloadResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IModelService modelService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PullModelCommandHandler"/> class.
    /// </summary>
    public PullModelCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IModelService modelService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.modelService = modelService;
    }

    /// <inheritdoc />
    public async Task<ModelDownloadResponse> Handle(PullModelCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ModelAccess.RequireOrganizationContext(currentUserService);
        await ModelAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ModelPull,
            cancellationToken);

        return await modelService.StartPullAsync(
            organizationId,
            request.PodId,
            request.Model,
            userId,
            cancellationToken);
    }
}
