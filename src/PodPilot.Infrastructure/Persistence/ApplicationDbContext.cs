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
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    IQueryable<RefreshToken> IApplicationDbContext.RefreshTokens => RefreshTokens;

    IQueryable<Organization> IApplicationDbContext.Organizations => Organizations;

    IQueryable<OrganizationMember> IApplicationDbContext.OrganizationMembers => OrganizationMembers;

    IQueryable<AuditLog> IApplicationDbContext.AuditLogs => AuditLogs;

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
