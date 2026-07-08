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
