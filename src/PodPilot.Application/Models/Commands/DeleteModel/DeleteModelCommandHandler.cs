using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Models.Commands.DeleteModel;

/// <summary>
/// Handles model delete commands.
/// </summary>
public sealed class DeleteModelCommandHandler : IRequestHandler<DeleteModelCommand>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IModelService modelService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteModelCommandHandler"/> class.
    /// </summary>
    public DeleteModelCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IModelService modelService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.modelService = modelService;
    }

    /// <inheritdoc />
    public async Task Handle(DeleteModelCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ModelAccess.RequireOrganizationContext(currentUserService);
        await ModelAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ModelDelete,
            cancellationToken);

        await modelService.DeleteModelAsync(
            organizationId,
            request.ModelId,
            request.ForceDefault,
            userId,
            cancellationToken);
    }
}
