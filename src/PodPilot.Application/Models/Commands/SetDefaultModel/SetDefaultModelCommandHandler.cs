using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models;
using PodPilot.Contracts.Models;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Models.Commands.SetDefaultModel;

/// <summary>
/// Handles set default model commands.
/// </summary>
public sealed class SetDefaultModelCommandHandler : IRequestHandler<SetDefaultModelCommand, ModelResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IModelService modelService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetDefaultModelCommandHandler"/> class.
    /// </summary>
    public SetDefaultModelCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IModelService modelService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.modelService = modelService;
    }

    /// <inheritdoc />
    public async Task<ModelResponse> Handle(SetDefaultModelCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ModelAccess.RequireOrganizationContext(currentUserService);
        await ModelAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ModelManage,
            cancellationToken);

        return await modelService.SetDefaultModelAsync(
            organizationId,
            request.ModelId,
            userId,
            cancellationToken);
    }
}
