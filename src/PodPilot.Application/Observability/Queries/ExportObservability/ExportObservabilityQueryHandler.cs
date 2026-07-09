using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Observability;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Observability.Queries.ExportObservability;

/// <summary>
/// Handles exporting observability data.
/// </summary>
public sealed class ExportObservabilityQueryHandler : IRequestHandler<ExportObservabilityQuery, ExportResult>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IObservabilityExportService exportService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportObservabilityQueryHandler"/> class.
    /// </summary>
    public ExportObservabilityQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IObservabilityExportService exportService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.exportService = exportService;
    }

    /// <inheritdoc />
    public async Task<ExportResult> Handle(ExportObservabilityQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ObservabilityAccess.RequireOrganizationContext(currentUserService);

        await ObservabilityAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ObservabilityExport,
            cancellationToken);

        return await exportService.ExportAsync(
            organizationId,
            request.Format,
            request.ExportType,
            new ObservabilityExportFilter
            {
                From = request.From,
                To = request.To,
                ProviderId = request.ProviderId,
                PodId = request.PodId,
                ModelName = request.ModelName,
            },
            cancellationToken);
    }
}
