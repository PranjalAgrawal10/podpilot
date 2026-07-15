using Microsoft.AspNetCore.SignalR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Hubs;

namespace PodPilot.Infrastructure.Commercial;

/// <summary>
/// Broadcasts commercial events via SignalR.
/// </summary>
public sealed class CommercialNotificationService : ICommercialNotificationService
{
    private readonly IHubContext<CommercialHub> hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommercialNotificationService"/> class.
    /// </summary>
    public CommercialNotificationService(IHubContext<CommercialHub> hubContext) =>
        this.hubContext = hubContext;

    /// <inheritdoc />
    public Task NotifySubscriptionChangedAsync(Guid organizationId, string status, CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "SubscriptionChanged", new { status }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyUsageThresholdAsync(Guid organizationId, string metric, decimal percentUsed, CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "UsageThreshold", new { metric, percentUsed }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyInvoiceGeneratedAsync(Guid organizationId, string invoiceNumber, CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "InvoiceGenerated", new { invoiceNumber }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyLicenseUpdatedAsync(Guid organizationId, string edition, CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "LicenseUpdated", new { edition }, cancellationToken);

    private Task SendAsync(Guid organizationId, string method, object payload, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(CommercialHub.GetOrganizationGroupName(organizationId))
            .SendAsync(method, payload, cancellationToken);
}

/// <summary>
/// No-op notifications for Testing.
/// </summary>
public sealed class NoOpCommercialNotificationService : ICommercialNotificationService
{
    /// <inheritdoc />
    public Task NotifySubscriptionChangedAsync(Guid organizationId, string status, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyUsageThresholdAsync(Guid organizationId, string metric, decimal percentUsed, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyInvoiceGeneratedAsync(Guid organizationId, string invoiceNumber, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyLicenseUpdatedAsync(Guid organizationId, string edition, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
