using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Infrastructure.Identity;

namespace PodPilot.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">The context options.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <inheritdoc />
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <inheritdoc />
    public DbSet<Organization> Organizations => Set<Organization>();

    /// <inheritdoc />
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();

    /// <inheritdoc />
    public DbSet<Invitation> Invitations => Set<Invitation>();

    /// <inheritdoc />
    public DbSet<Permission> Permissions => Set<Permission>();

    /// <inheritdoc />
    public DbSet<Role> OrgRoles => Set<Role>();

    /// <inheritdoc />
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <inheritdoc />
    public DbSet<ComputeProvider> ComputeProviders => Set<ComputeProvider>();

    /// <inheritdoc />
    public DbSet<ProviderCredential> ProviderCredentials => Set<ProviderCredential>();

    /// <inheritdoc />
    public DbSet<ProviderRegion> ProviderRegions => Set<ProviderRegion>();

    /// <inheritdoc />
    public DbSet<ProviderGpu> ProviderGpus => Set<ProviderGpu>();

    /// <inheritdoc />
    public DbSet<ProviderHealth> ProviderHealthSnapshots => Set<ProviderHealth>();

    /// <inheritdoc />
    public DbSet<ProviderHealthHistory> ProviderHealthHistoryEntries => Set<ProviderHealthHistory>();

    /// <inheritdoc />
    public DbSet<GpuPod> GpuPods => Set<GpuPod>();

    /// <inheritdoc />
    public DbSet<PodConfiguration> PodConfigurations => Set<PodConfiguration>();

    /// <inheritdoc />
    public DbSet<PodEndpoint> PodEndpoints => Set<PodEndpoint>();

    /// <inheritdoc />
    public DbSet<PodStatusHistory> PodStatusHistoryEntries => Set<PodStatusHistory>();

    /// <inheritdoc />
    public DbSet<PodActivity> PodActivities => Set<PodActivity>();

    /// <inheritdoc />
    public DbSet<PodLifecycleEvent> PodLifecycleEvents => Set<PodLifecycleEvent>();

    /// <inheritdoc />
    public DbSet<PodIdlePolicy> PodIdlePolicies => Set<PodIdlePolicy>();

    /// <inheritdoc />
    public DbSet<PodLifecycleLock> PodLifecycleLocks => Set<PodLifecycleLock>();

    /// <inheritdoc />
    public DbSet<PodWakeRequest> PodWakeRequests => Set<PodWakeRequest>();

    /// <inheritdoc />
    public DbSet<GatewayApiKey> GatewayApiKeys => Set<GatewayApiKey>();

    /// <inheritdoc />
    public DbSet<GatewayRoute> GatewayRoutes => Set<GatewayRoute>();

    /// <inheritdoc />
    public DbSet<GatewayRequest> GatewayRequests => Set<GatewayRequest>();

    IQueryable<RefreshToken> IApplicationDbContext.RefreshTokens => RefreshTokens;

    IQueryable<Organization> IApplicationDbContext.Organizations => Organizations;

    IQueryable<OrganizationMember> IApplicationDbContext.OrganizationMembers => OrganizationMembers;

    IQueryable<Invitation> IApplicationDbContext.Invitations => Invitations;

    IQueryable<Permission> IApplicationDbContext.Permissions => Permissions;

    IQueryable<Role> IApplicationDbContext.Roles => OrgRoles;

    IQueryable<AuditLog> IApplicationDbContext.AuditLogs => AuditLogs;

    IQueryable<ComputeProvider> IApplicationDbContext.ComputeProviders => ComputeProviders;

    IQueryable<ProviderCredential> IApplicationDbContext.ProviderCredentials => ProviderCredentials;

    IQueryable<ProviderRegion> IApplicationDbContext.ProviderRegions => ProviderRegions;

    IQueryable<ProviderGpu> IApplicationDbContext.ProviderGpus => ProviderGpus;

    IQueryable<ProviderHealth> IApplicationDbContext.ProviderHealthSnapshots => ProviderHealthSnapshots;

    IQueryable<ProviderHealthHistory> IApplicationDbContext.ProviderHealthHistory => ProviderHealthHistoryEntries;

    IQueryable<GpuPod> IApplicationDbContext.GpuPods => GpuPods;

    IQueryable<PodConfiguration> IApplicationDbContext.PodConfigurations => PodConfigurations;

    IQueryable<PodEndpoint> IApplicationDbContext.PodEndpoints => PodEndpoints;

    IQueryable<PodStatusHistory> IApplicationDbContext.PodStatusHistory => PodStatusHistoryEntries;

    IQueryable<PodActivity> IApplicationDbContext.PodActivities => PodActivities;

    IQueryable<PodLifecycleEvent> IApplicationDbContext.PodLifecycleEvents => PodLifecycleEvents;

    IQueryable<PodIdlePolicy> IApplicationDbContext.PodIdlePolicies => PodIdlePolicies;

    IQueryable<PodLifecycleLock> IApplicationDbContext.PodLifecycleLocks => PodLifecycleLocks;

    IQueryable<PodWakeRequest> IApplicationDbContext.PodWakeRequests => PodWakeRequests;

    IQueryable<GatewayApiKey> IApplicationDbContext.GatewayApiKeys => GatewayApiKeys;

    IQueryable<GatewayRoute> IApplicationDbContext.GatewayRoutes => GatewayRoutes;

    IQueryable<GatewayRequest> IApplicationDbContext.GatewayRequests => GatewayRequests;

    /// <inheritdoc />
    public Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default) =>
        AuditLogs.AddAsync(auditLog, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default) =>
        RefreshTokens.AddAsync(refreshToken, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddOrganizationAsync(Organization organization, CancellationToken cancellationToken = default) =>
        Organizations.AddAsync(organization, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddOrganizationMemberAsync(OrganizationMember member, CancellationToken cancellationToken = default) =>
        OrganizationMembers.AddAsync(member, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddInvitationAsync(Invitation invitation, CancellationToken cancellationToken = default) =>
        Invitations.AddAsync(invitation, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddComputeProviderAsync(ComputeProvider provider, CancellationToken cancellationToken = default) =>
        ComputeProviders.AddAsync(provider, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddProviderCredentialAsync(ProviderCredential credential, CancellationToken cancellationToken = default) =>
        ProviderCredentials.AddAsync(credential, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddProviderHealthAsync(ProviderHealth health, CancellationToken cancellationToken = default) =>
        ProviderHealthSnapshots.AddAsync(health, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddProviderHealthHistoryAsync(ProviderHealthHistory history, CancellationToken cancellationToken = default) =>
        ProviderHealthHistoryEntries.AddAsync(history, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddProviderRegionAsync(ProviderRegion region, CancellationToken cancellationToken = default) =>
        ProviderRegions.AddAsync(region, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddProviderGpuAsync(ProviderGpu gpu, CancellationToken cancellationToken = default) =>
        ProviderGpus.AddAsync(gpu, cancellationToken).AsTask();

    /// <inheritdoc />
    public async Task ClearProviderCatalogAsync(Guid computeProviderId, CancellationToken cancellationToken = default)
    {
        var regions = await ProviderRegions
            .Where(r => r.ComputeProviderId == computeProviderId)
            .ToListAsync(cancellationToken);

        if (regions.Count > 0)
        {
            ProviderRegions.RemoveRange(regions);
        }

        var gpus = await ProviderGpus
            .Where(g => g.ComputeProviderId == computeProviderId)
            .ToListAsync(cancellationToken);

        if (gpus.Count > 0)
        {
            ProviderGpus.RemoveRange(gpus);
        }
    }

    /// <inheritdoc />
    public async Task RemoveComputeProviderAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var provider = await ComputeProviders
            .FirstOrDefaultAsync(p => p.Id == providerId, cancellationToken);

        if (provider is not null)
        {
            ComputeProviders.Remove(provider);
        }
    }

    /// <inheritdoc />
    public Task AddGpuPodAsync(GpuPod pod, CancellationToken cancellationToken = default) =>
        GpuPods.AddAsync(pod, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodStatusHistoryAsync(PodStatusHistory history, CancellationToken cancellationToken = default) =>
        PodStatusHistoryEntries.AddAsync(history, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodActivityAsync(PodActivity activity, CancellationToken cancellationToken = default) =>
        PodActivities.AddAsync(activity, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodLifecycleEventAsync(PodLifecycleEvent lifecycleEvent, CancellationToken cancellationToken = default) =>
        PodLifecycleEvents.AddAsync(lifecycleEvent, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodIdlePolicyAsync(PodIdlePolicy policy, CancellationToken cancellationToken = default) =>
        PodIdlePolicies.AddAsync(policy, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodWakeRequestAsync(PodWakeRequest request, CancellationToken cancellationToken = default) =>
        PodWakeRequests.AddAsync(request, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddPodLifecycleLockAsync(PodLifecycleLock lifecycleLock, CancellationToken cancellationToken = default) =>
        PodLifecycleLocks.AddAsync(lifecycleLock, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddGatewayApiKeyAsync(GatewayApiKey apiKey, CancellationToken cancellationToken = default) =>
        GatewayApiKeys.AddAsync(apiKey, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddGatewayRouteAsync(GatewayRoute route, CancellationToken cancellationToken = default) =>
        GatewayRoutes.AddAsync(route, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task AddGatewayRequestAsync(GatewayRequest request, CancellationToken cancellationToken = default) =>
        GatewayRequests.AddAsync(request, cancellationToken).AsTask();

    /// <inheritdoc />
    public async Task RemoveGatewayRouteAsync(Guid routeId, CancellationToken cancellationToken = default)
    {
        var route = await GatewayRoutes.FirstOrDefaultAsync(r => r.Id == routeId, cancellationToken);
        if (route is not null)
        {
            GatewayRoutes.Remove(route);
        }
    }

    /// <inheritdoc />
    public async Task RemovePodEndpointsAsync(Guid podId, CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<PodEndpoint>()
            .Where(e => e.Entity.GpuPodId == podId)
            .ToList())
        {
            entry.State = EntityState.Detached;
        }

        await PodEndpoints
            .Where(e => e.GpuPodId == podId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task AddPodEndpointAsync(PodEndpoint endpoint, CancellationToken cancellationToken = default) =>
        PodEndpoints.AddAsync(endpoint, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task RemovePodLifecycleLockAsync(PodLifecycleLock lifecycleLock, CancellationToken cancellationToken = default)
    {
        PodLifecycleLocks.Remove(lifecycleLock);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task RemoveGpuPodAsync(Guid podId, CancellationToken cancellationToken = default)
    {
        var pod = await GpuPods.FirstOrDefaultAsync(p => p.Id == podId, cancellationToken);
        if (pod is not null)
        {
            GpuPods.Remove(pod);
        }
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
    }
}
