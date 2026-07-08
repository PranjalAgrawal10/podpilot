using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models;
using PodPilot.Contracts.Models;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Queries.GetDownloads;

/// <summary>
/// Handles listing model downloads.
/// </summary>
public sealed class GetDownloadsQueryHandler : IRequestHandler<GetDownloadsQuery, IReadOnlyList<ModelDownloadResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IModelRepository modelRepository;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDownloadsQueryHandler"/> class.
    /// </summary>
    public GetDownloadsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IModelRepository modelRepository,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.modelRepository = modelRepository;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ModelDownloadResponse>> Handle(GetDownloadsQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ModelAccess.RequireOrganizationContext(currentUserService);
        await ModelAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ModelRead,
            cancellationToken);

        if (request.ActiveOnly)
        {
            var active = await modelRepository.ListActiveDownloadsAsync(organizationId, cancellationToken);
            return active.Select(ModelMapper.ToDownloadResponse).ToList();
        }

        var downloads = await dbContext.ModelDownloads
            .Include(d => d.Model)
                .ThenInclude(m => m.Pod)
            .Where(d => d.Model.OrganizationId == organizationId)
            .OrderByDescending(d => d.StartedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        return downloads.Select(ModelMapper.ToDownloadResponse).ToList();
    }
}
