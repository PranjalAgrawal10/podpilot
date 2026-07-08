using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Application database context abstraction.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Gets the refresh tokens set.
    /// </summary>
    IQueryable<RefreshToken> RefreshTokens { get; }

    /// <summary>
    /// Gets the organizations set.
    /// </summary>
    IQueryable<Organization> Organizations { get; }

    /// <summary>
    /// Gets the organization members set.
    /// </summary>
    IQueryable<OrganizationMember> OrganizationMembers { get; }

    /// <summary>
    /// Gets the invitations set.
    /// </summary>
    IQueryable<Invitation> Invitations { get; }

    /// <summary>
    /// Gets the permissions set.
    /// </summary>
    IQueryable<Permission> Permissions { get; }

    /// <summary>
    /// Gets the roles set.
    /// </summary>
    IQueryable<Role> Roles { get; }

    /// <summary>
    /// Gets the audit logs set.
    /// </summary>
    IQueryable<AuditLog> AuditLogs { get; }

    /// <summary>
    /// Gets the compute providers set.
    /// </summary>
    IQueryable<ComputeProvider> ComputeProviders { get; }

    /// <summary>
    /// Gets the provider credentials set.
    /// </summary>
    IQueryable<ProviderCredential> ProviderCredentials { get; }

    /// <summary>
    /// Gets the provider regions set.
    /// </summary>
    IQueryable<ProviderRegion> ProviderRegions { get; }

    /// <summary>
    /// Gets the provider GPUs set.
    /// </summary>
    IQueryable<ProviderGpu> ProviderGpus { get; }

    /// <summary>
    /// Gets the provider health snapshots set.
    /// </summary>
    IQueryable<ProviderHealth> ProviderHealthSnapshots { get; }

    /// <summary>
    /// Gets the provider health history set.
    /// </summary>
    IQueryable<ProviderHealthHistory> ProviderHealthHistory { get; }

    /// <summary>
    /// Gets the GPU pods set.
    /// </summary>
    IQueryable<GpuPod> GpuPods { get; }

    /// <summary>
    /// Gets the pod configurations set.
    /// </summary>
    IQueryable<PodConfiguration> PodConfigurations { get; }

    /// <summary>
    /// Gets the pod endpoints set.
    /// </summary>
    IQueryable<PodEndpoint> PodEndpoints { get; }

    /// <summary>
    /// Gets the pod status history set.
    /// </summary>
    IQueryable<PodStatusHistory> PodStatusHistory { get; }

    /// <summary>
    /// Gets the pod activities set.
    /// </summary>
    IQueryable<PodActivity> PodActivities { get; }

    /// <summary>
    /// Gets the pod lifecycle events set.
    /// </summary>
    IQueryable<PodLifecycleEvent> PodLifecycleEvents { get; }

    /// <summary>
    /// Gets the pod idle policies set.
    /// </summary>
    IQueryable<PodIdlePolicy> PodIdlePolicies { get; }

    /// <summary>
    /// Gets the pod lifecycle locks set.
    /// </summary>
    IQueryable<PodLifecycleLock> PodLifecycleLocks { get; }

    /// <summary>
    /// Gets the pod wake requests set.
    /// </summary>
    IQueryable<PodWakeRequest> PodWakeRequests { get; }

    /// <summary>
    /// Adds an audit log entry.
    /// </summary>
    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a refresh token.
    /// </summary>
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an organization.
    /// </summary>
    Task AddOrganizationAsync(Organization organization, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an organization member.
    /// </summary>
    Task AddOrganizationMemberAsync(OrganizationMember member, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an invitation.
    /// </summary>
    Task AddInvitationAsync(Invitation invitation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a compute provider.
    /// </summary>
    Task AddComputeProviderAsync(ComputeProvider provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a provider credential.
    /// </summary>
    Task AddProviderCredentialAsync(ProviderCredential credential, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a provider health snapshot.
    /// </summary>
    Task AddProviderHealthAsync(ProviderHealth health, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a provider health history entry.
    /// </summary>
    Task AddProviderHealthHistoryAsync(ProviderHealthHistory history, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a provider region.
    /// </summary>
    Task AddProviderRegionAsync(ProviderRegion region, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a provider GPU.
    /// </summary>
    Task AddProviderGpuAsync(ProviderGpu gpu, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes cached regions and GPUs for a provider.
    /// </summary>
    Task ClearProviderCatalogAsync(Guid computeProviderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a compute provider.
    /// </summary>
    Task RemoveComputeProviderAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a GPU pod.
    /// </summary>
    Task AddGpuPodAsync(GpuPod pod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod status history entry.
    /// </summary>
    Task AddPodStatusHistoryAsync(PodStatusHistory history, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod activity entry.
    /// </summary>
    Task AddPodActivityAsync(PodActivity activity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod lifecycle event.
    /// </summary>
    Task AddPodLifecycleEventAsync(PodLifecycleEvent lifecycleEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod idle policy.
    /// </summary>
    Task AddPodIdlePolicyAsync(PodIdlePolicy policy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod wake request.
    /// </summary>
    Task AddPodWakeRequestAsync(PodWakeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a pod lifecycle lock.
    /// </summary>
    Task AddPodLifecycleLockAsync(PodLifecycleLock lifecycleLock, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a pod lifecycle lock.
    /// </summary>
    Task RemovePodLifecycleLockAsync(PodLifecycleLock lifecycleLock, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a GPU pod.
    /// </summary>
    Task RemoveGpuPodAsync(Guid podId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
