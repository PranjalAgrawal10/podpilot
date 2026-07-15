using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Security;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Security;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using AppCreateSecret = PodPilot.Application.Models.Security.CreateSecretRequest;
using AppUpdateSecret = PodPilot.Application.Models.Security.UpdateSecretRequest;
using ContractsCreateIdp = PodPilot.Contracts.Security.CreateIdentityProviderRequest;

namespace PodPilot.Application.Security;

/// <summary>Lists identity providers for an organization.</summary>
public sealed class ListIdentityProvidersQuery : IRequest<IReadOnlyList<IdentityProviderResponse>>
{
    /// <summary>Gets optional organization id for anonymous SSO discovery.</summary>
    public Guid? OrganizationId { get; init; }

    /// <summary>Gets whether only enabled providers should be returned (SSO catalog).</summary>
    public bool PublicCatalog { get; init; }
}

/// <summary>Handles <see cref="ListIdentityProvidersQuery"/>.</summary>
public sealed class ListIdentityProvidersQueryHandler
    : IRequestHandler<ListIdentityProvidersQuery, IReadOnlyList<IdentityProviderResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;

    /// <summary>Initializes a new instance of the <see cref="ListIdentityProvidersQueryHandler"/> class.</summary>
    public ListIdentityProvidersQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IdentityProviderResponse>> Handle(
        ListIdentityProvidersQuery request,
        CancellationToken cancellationToken)
    {
        Guid organizationId;
        if (request.PublicCatalog)
        {
            organizationId = request.OrganizationId
                ?? throw new ValidationException(
                [
                    new FluentValidation.Results.ValidationFailure(
                        nameof(request.OrganizationId),
                        "organizationId is required."),
                ]);
        }
        else
        {
            var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
            await SecurityAccess.EnsurePermissionAsync(
                auth, orgId, userId, PermissionNames.SecurityRead, cancellationToken);
            organizationId = orgId;
        }

        var query = db.IdentityProviders.AsNoTracking()
            .Where(p => p.OrganizationId == organizationId);
        if (request.PublicCatalog)
        {
            query = query.Where(p => p.IsEnabled);
        }

        var providers = await query.OrderBy(p => p.Name).ToListAsync(cancellationToken);
        return providers.Select(SecurityMapper.ToIdentityProviderResponse).ToList();
    }
}

/// <summary>Creates an identity provider.</summary>
public sealed class CreateIdentityProviderCommand : IRequest<IdentityProviderResponse>
{
    /// <summary>Gets the create request.</summary>
    public ContractsCreateIdp Request { get; init; } = new();
}

/// <summary>Handles <see cref="CreateIdentityProviderCommand"/>.</summary>
public sealed class CreateIdentityProviderCommandHandler
    : IRequestHandler<CreateIdentityProviderCommand, IdentityProviderResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;
    private readonly IEncryptionService encryptionService;
    private readonly IDateTimeService dateTimeService;
    private readonly IEnterpriseAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>Initializes a new instance of the <see cref="CreateIdentityProviderCommandHandler"/> class.</summary>
    public CreateIdentityProviderCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db,
        IEncryptionService encryptionService,
        IDateTimeService dateTimeService,
        IEnterpriseAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
        this.encryptionService = encryptionService;
        this.dateTimeService = dateTimeService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<IdentityProviderResponse> Handle(
        CreateIdentityProviderCommand command,
        CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(
            auth, orgId, userId, PermissionNames.SecurityManage, cancellationToken);

        var request = command.Request;
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.Name), "Name is required."),
            ]);
        }

        var exists = await db.IdentityProviders.AnyAsync(
            p => p.OrganizationId == orgId && p.Name == request.Name.Trim(),
            cancellationToken);
        if (exists)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.Name), "An identity provider with this name already exists."),
            ]);
        }

        if (!Enum.TryParse<IdentityProviderKind>(request.ProviderKind, true, out var kind))
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.ProviderKind), "Invalid provider kind."),
            ]);
        }

        if (!Enum.TryParse<IdentityProtocol>(request.Protocol, true, out var protocol))
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.Protocol), "Invalid protocol."),
            ]);
        }

        var now = dateTimeService.UtcNow;
        var provider = new IdentityProvider
        {
            OrganizationId = orgId,
            Name = request.Name.Trim(),
            ProviderKind = kind,
            Protocol = protocol,
            IsEnabled = request.IsEnabled,
            ClientId = request.ClientId,
            EncryptedClientSecret = string.IsNullOrWhiteSpace(request.ClientSecret)
                ? null
                : encryptionService.Encrypt(request.ClientSecret),
            Issuer = request.Issuer,
            AuthorizationEndpoint = request.AuthorizationEndpoint,
            TokenEndpoint = request.TokenEndpoint,
            JwksUri = request.JwksUri,
            SamlEntityId = request.SamlEntityId,
            SamlSsoUrl = request.SamlSsoUrl,
            EncryptedSamlCertificate = string.IsNullOrWhiteSpace(request.SamlCertificate)
                ? null
                : encryptionService.Encrypt(request.SamlCertificate),
            Scopes = string.IsNullOrWhiteSpace(request.Scopes) ? "openid profile email" : request.Scopes,
            CreatedAt = now,
        };

        await db.AddIdentityProviderAsync(provider, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.AppendAsync(
            new EnterpriseAuditEntry
            {
                OrganizationId = orgId,
                UserId = userId,
                Category = AuditEventCategory.Authentication,
                EventType = "IdentityProviderCreated",
                EntityType = nameof(IdentityProvider),
                EntityId = provider.Id.ToString(),
                Summary = $"Created identity provider '{provider.Name}'",
                IpAddress = httpContextService.IpAddress,
                CorrelationId = httpContextService.CorrelationId,
                OccurredAt = now,
            },
            cancellationToken);

        return SecurityMapper.ToIdentityProviderResponse(provider);
    }
}

/// <summary>Deletes an identity provider.</summary>
public sealed class DeleteIdentityProviderCommand : IRequest<Unit>
{
    /// <summary>Gets the identity provider id.</summary>
    public Guid IdentityProviderId { get; init; }
}

/// <summary>Handles <see cref="DeleteIdentityProviderCommand"/>.</summary>
public sealed class DeleteIdentityProviderCommandHandler : IRequestHandler<DeleteIdentityProviderCommand, Unit>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;
    private readonly IEnterpriseAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>Initializes a new instance of the <see cref="DeleteIdentityProviderCommandHandler"/> class.</summary>
    public DeleteIdentityProviderCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db,
        IEnterpriseAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteIdentityProviderCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(
            auth, orgId, userId, PermissionNames.SecurityManage, cancellationToken);

        var provider = await db.IdentityProviders
            .FirstOrDefaultAsync(
                p => p.Id == request.IdentityProviderId && p.OrganizationId == orgId,
                cancellationToken)
            ?? throw new NotFoundException("Identity provider", request.IdentityProviderId);

        await db.RemoveIdentityProviderAsync(provider.Id, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.AppendAsync(
            new EnterpriseAuditEntry
            {
                OrganizationId = orgId,
                UserId = userId,
                Category = AuditEventCategory.Authentication,
                EventType = "IdentityProviderDeleted",
                EntityType = nameof(IdentityProvider),
                EntityId = provider.Id.ToString(),
                Summary = $"Deleted identity provider '{provider.Name}'",
                IpAddress = httpContextService.IpAddress,
                CorrelationId = httpContextService.CorrelationId,
                OccurredAt = dateTimeService.UtcNow,
            },
            cancellationToken);

        return Unit.Value;
    }
}

/// <summary>Begins an SSO challenge.</summary>
public sealed class BeginSsoCommand : IRequest<SsoChallengeResponse>
{
    /// <summary>Gets the organization id.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets the identity provider id.</summary>
    public Guid IdentityProviderId { get; init; }

    /// <summary>Gets the redirect URI.</summary>
    public string RedirectUri { get; init; } = string.Empty;
}

/// <summary>Handles <see cref="BeginSsoCommand"/>.</summary>
public sealed class BeginSsoCommandHandler : IRequestHandler<BeginSsoCommand, SsoChallengeResponse>
{
    private readonly ISsoService ssoService;

    /// <summary>Initializes a new instance of the <see cref="BeginSsoCommandHandler"/> class.</summary>
    public BeginSsoCommandHandler(ISsoService ssoService) => this.ssoService = ssoService;

    /// <inheritdoc />
    public async Task<SsoChallengeResponse> Handle(BeginSsoCommand request, CancellationToken cancellationToken)
    {
        var result = await ssoService.BeginAsync(
            new SsoBeginRequest
            {
                OrganizationId = request.OrganizationId,
                IdentityProviderId = request.IdentityProviderId,
                RedirectUri = request.RedirectUri,
            },
            cancellationToken);
        return SecurityMapper.ToSsoChallengeResponse(result);
    }
}

/// <summary>Completes SSO and issues tokens.</summary>
public sealed class CompleteSsoCommand : IRequest<AuthResponse>
{
    /// <summary>Gets the organization id.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets the identity provider id.</summary>
    public Guid IdentityProviderId { get; init; }

    /// <summary>Gets the OIDC code.</summary>
    public string? Code { get; init; }

    /// <summary>Gets the OIDC state.</summary>
    public string? State { get; init; }

    /// <summary>Gets the SAML response.</summary>
    public string? SamlResponse { get; init; }

    /// <summary>Gets the redirect URI.</summary>
    public string RedirectUri { get; init; } = string.Empty;
}

/// <summary>Handles <see cref="CompleteSsoCommand"/>.</summary>
public sealed class CompleteSsoCommandHandler : IRequestHandler<CompleteSsoCommand, AuthResponse>
{
    private readonly ISsoService ssoService;
    private readonly IAuthTokenIssuer authTokenIssuer;
    private readonly IMfaService mfaService;
    private readonly IMfaChallengeStore mfaChallengeStore;

    /// <summary>Initializes a new instance of the <see cref="CompleteSsoCommandHandler"/> class.</summary>
    public CompleteSsoCommandHandler(
        ISsoService ssoService,
        IAuthTokenIssuer authTokenIssuer,
        IMfaService mfaService,
        IMfaChallengeStore mfaChallengeStore)
    {
        this.ssoService = ssoService;
        this.authTokenIssuer = authTokenIssuer;
        this.mfaService = mfaService;
        this.mfaChallengeStore = mfaChallengeStore;
    }

    /// <inheritdoc />
    public async Task<AuthResponse> Handle(CompleteSsoCommand request, CancellationToken cancellationToken)
    {
        var result = await ssoService.CompleteAsync(
            new SsoCompleteRequest
            {
                OrganizationId = request.OrganizationId,
                IdentityProviderId = request.IdentityProviderId,
                Code = request.Code,
                State = request.State,
                SamlResponse = request.SamlResponse,
                RedirectUri = request.RedirectUri,
            },
            cancellationToken);

        if (result.RequiresMfa || await mfaService.IsEnabledAsync(result.UserId, cancellationToken))
        {
            var token = result.MfaChallengeToken ?? Guid.NewGuid().ToString("N");
            await mfaChallengeStore.StoreAsync(token, result.UserId, TimeSpan.FromMinutes(5), cancellationToken);
            return new AuthResponse
            {
                RequiresMfa = true,
                MfaToken = token,
                User = new UserSummary
                {
                    Id = result.UserId,
                    Email = result.Email,
                },
            };
        }

        if (!string.IsNullOrWhiteSpace(result.AccessToken))
        {
            return new AuthResponse
            {
                AccessToken = result.AccessToken!,
                RefreshToken = result.RefreshToken ?? string.Empty,
                User = new UserSummary
                {
                    Id = result.UserId,
                    Email = result.Email,
                },
            };
        }

        return await authTokenIssuer.IssueTokensAsync(result.UserId, request.OrganizationId, cancellationToken);
    }
}

/// <summary>Starts MFA enrollment for the current user.</summary>
public sealed class EnrollMfaCommand : IRequest<MfaEnrollmentResponse>
{
}

/// <summary>Handles <see cref="EnrollMfaCommand"/>.</summary>
public sealed class EnrollMfaCommandHandler : IRequestHandler<EnrollMfaCommand, MfaEnrollmentResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IMfaService mfaService;

    /// <summary>Initializes a new instance of the <see cref="EnrollMfaCommandHandler"/> class.</summary>
    public EnrollMfaCommandHandler(ICurrentUserService currentUserService, IMfaService mfaService)
    {
        this.currentUserService = currentUserService;
        this.mfaService = mfaService;
    }

    /// <inheritdoc />
    public async Task<MfaEnrollmentResponse> Handle(EnrollMfaCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var result = await mfaService.BeginEnrollmentAsync(currentUserService.UserId.Value, cancellationToken);
        return SecurityMapper.ToMfaEnrollmentResponse(result);
    }
}

/// <summary>Confirms MFA enrollment with a TOTP code.</summary>
public sealed class ConfirmMfaCommand : IRequest<Unit>
{
    /// <summary>Gets the TOTP code.</summary>
    public string Code { get; init; } = string.Empty;
}

/// <summary>Handles <see cref="ConfirmMfaCommand"/>.</summary>
public sealed class ConfirmMfaCommandHandler : IRequestHandler<ConfirmMfaCommand, Unit>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IMfaService mfaService;

    /// <summary>Initializes a new instance of the <see cref="ConfirmMfaCommandHandler"/> class.</summary>
    public ConfirmMfaCommandHandler(ICurrentUserService currentUserService, IMfaService mfaService)
    {
        this.currentUserService = currentUserService;
        this.mfaService = mfaService;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(ConfirmMfaCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        await mfaService.ConfirmEnrollmentAsync(currentUserService.UserId.Value, request.Code, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Verifies MFA during login using a challenge token.</summary>
public sealed class VerifyMfaCommand : IRequest<AuthResponse>
{
    /// <summary>Gets the TOTP code.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Gets the MFA challenge token.</summary>
    public string MfaToken { get; init; } = string.Empty;
}

/// <summary>Handles <see cref="VerifyMfaCommand"/>.</summary>
public sealed class VerifyMfaCommandHandler : IRequestHandler<VerifyMfaCommand, AuthResponse>
{
    private readonly IMfaChallengeStore mfaChallengeStore;
    private readonly IMfaService mfaService;
    private readonly IAuthTokenIssuer authTokenIssuer;
    private readonly ISessionTracker sessionTracker;
    private readonly IEnterpriseAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IPolicyEngine policyEngine;

    /// <summary>Initializes a new instance of the <see cref="VerifyMfaCommandHandler"/> class.</summary>
    public VerifyMfaCommandHandler(
        IMfaChallengeStore mfaChallengeStore,
        IMfaService mfaService,
        IAuthTokenIssuer authTokenIssuer,
        ISessionTracker sessionTracker,
        IEnterpriseAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IPolicyEngine policyEngine)
    {
        this.mfaChallengeStore = mfaChallengeStore;
        this.mfaService = mfaService;
        this.authTokenIssuer = authTokenIssuer;
        this.sessionTracker = sessionTracker;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.policyEngine = policyEngine;
    }

    /// <inheritdoc />
    public async Task<AuthResponse> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.MfaToken) || string.IsNullOrWhiteSpace(request.Code))
        {
            throw new UnauthorizedException("Invalid MFA challenge.");
        }

        var userId = await mfaChallengeStore.ConsumeAsync(request.MfaToken, cancellationToken)
            ?? throw new UnauthorizedException("Invalid or expired MFA token.");

        var valid = await mfaService.VerifyAsync(userId, request.Code, cancellationToken);
        if (!valid)
        {
            throw new UnauthorizedException("Invalid MFA code.");
        }

        var membership = await organizationAuthorizationService.GetDefaultMembershipAsync(userId, cancellationToken);
        if (membership is not null)
        {
            await policyEngine.EnsureIpAllowedAsync(
                membership.OrganizationId,
                httpContextService.IpAddress,
                cancellationToken);
        }

        var sessionId = Guid.NewGuid().ToString("N");
        await sessionTracker.TrackLoginAsync(
            new SessionTrackRequest
            {
                UserId = userId,
                OrganizationId = membership?.OrganizationId,
                SessionId = sessionId,
                IpAddress = httpContextService.IpAddress,
                Succeeded = true,
            },
            cancellationToken);

        await auditService.AppendAsync(
            new EnterpriseAuditEntry
            {
                OrganizationId = membership?.OrganizationId,
                UserId = userId,
                Category = AuditEventCategory.Authentication,
                EventType = "MfaVerified",
                EntityType = nameof(User),
                EntityId = userId.ToString(),
                Summary = "MFA verification succeeded",
                IpAddress = httpContextService.IpAddress,
                CorrelationId = httpContextService.CorrelationId,
                OccurredAt = dateTimeService.UtcNow,
            },
            cancellationToken);

        return await authTokenIssuer.IssueTokensAsync(userId, membership?.OrganizationId, cancellationToken);
    }
}

/// <summary>Lists organization secrets (metadata only).</summary>
public sealed class ListSecretsQuery : IRequest<IReadOnlyList<SecretResponse>>
{
}

/// <summary>Handles <see cref="ListSecretsQuery"/>.</summary>
public sealed class ListSecretsQueryHandler : IRequestHandler<ListSecretsQuery, IReadOnlyList<SecretResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;

    /// <summary>Initializes a new instance of the <see cref="ListSecretsQueryHandler"/> class.</summary>
    public ListSecretsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SecretResponse>> Handle(ListSecretsQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.SecretsRead, cancellationToken);

        var secrets = await db.SecretReferences.AsNoTracking()
            .Where(s => s.OrganizationId == orgId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
        return secrets.Select(SecurityMapper.ToSecretResponse).ToList();
    }
}

/// <summary>Creates a secret.</summary>
public sealed class CreateSecretCommand : IRequest<SecretResponse>
{
    /// <summary>Gets the secret name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the secret kind.</summary>
    public string SecretKind { get; init; } = "Generic";

    /// <summary>Gets the backend kind.</summary>
    public string BackendKind { get; init; } = "LocalEncrypted";

    /// <summary>Gets the plaintext value.</summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>Gets optional expiry.</summary>
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>Handles <see cref="CreateSecretCommand"/>.</summary>
public sealed class CreateSecretCommandHandler : IRequestHandler<CreateSecretCommand, SecretResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly ISecretManager secretManager;
    private readonly IApplicationDbContext db;
    private readonly IEnterpriseAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>Initializes a new instance of the <see cref="CreateSecretCommandHandler"/> class.</summary>
    public CreateSecretCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        ISecretManager secretManager,
        IApplicationDbContext db,
        IEnterpriseAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.secretManager = secretManager;
        this.db = db;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<SecretResponse> Handle(CreateSecretCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.SecretsManage, cancellationToken);

        if (!Enum.TryParse<SecretKind>(request.SecretKind, true, out var kind))
        {
            kind = SecretKind.Generic;
        }

        if (!Enum.TryParse<SecretBackendKind>(request.BackendKind, true, out var backend))
        {
            backend = SecretBackendKind.LocalEncrypted;
        }

        var secretId = await secretManager.CreateAsync(
            orgId,
            new AppCreateSecret
            {
                Name = request.Name,
                SecretKind = kind,
                BackendKind = backend,
                Plaintext = request.Value,
                ExpiresAt = request.ExpiresAt,
            },
            cancellationToken);

        var secret = await db.SecretReferences.AsNoTracking()
            .FirstAsync(s => s.Id == secretId && s.OrganizationId == orgId, cancellationToken);

        await auditService.AppendAsync(
            new EnterpriseAuditEntry
            {
                OrganizationId = orgId,
                UserId = userId,
                Category = AuditEventCategory.Secret,
                EventType = "SecretCreated",
                EntityType = nameof(SecretReference),
                EntityId = secretId.ToString(),
                Summary = $"Created secret '{secret.Name}'",
                IpAddress = httpContextService.IpAddress,
                CorrelationId = httpContextService.CorrelationId,
                OccurredAt = dateTimeService.UtcNow,
            },
            cancellationToken);

        return SecurityMapper.ToSecretResponse(secret);
    }
}

/// <summary>Updates a secret.</summary>
public sealed class UpdateSecretCommand : IRequest<SecretResponse>
{
    /// <summary>Gets the secret id.</summary>
    public Guid SecretId { get; init; }

    /// <summary>Gets optional new name.</summary>
    public string? Name { get; init; }

    /// <summary>Gets optional new value.</summary>
    public string? Value { get; init; }

    /// <summary>Gets optional expiry.</summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>Gets optional enabled flag.</summary>
    public bool? IsEnabled { get; init; }
}

/// <summary>Handles <see cref="UpdateSecretCommand"/>.</summary>
public sealed class UpdateSecretCommandHandler : IRequestHandler<UpdateSecretCommand, SecretResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly ISecretManager secretManager;
    private readonly IApplicationDbContext db;
    private readonly IEnterpriseAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>Initializes a new instance of the <see cref="UpdateSecretCommandHandler"/> class.</summary>
    public UpdateSecretCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        ISecretManager secretManager,
        IApplicationDbContext db,
        IEnterpriseAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.secretManager = secretManager;
        this.db = db;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<SecretResponse> Handle(UpdateSecretCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.SecretsManage, cancellationToken);

        await secretManager.UpdateAsync(
            orgId,
            request.SecretId,
            new AppUpdateSecret
            {
                Name = request.Name,
                Plaintext = request.Value,
                ExpiresAt = request.ExpiresAt,
                IsEnabled = request.IsEnabled,
            },
            cancellationToken);

        var secret = await db.SecretReferences.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SecretId && s.OrganizationId == orgId, cancellationToken)
            ?? throw new NotFoundException("Secret", request.SecretId);

        await auditService.AppendAsync(
            new EnterpriseAuditEntry
            {
                OrganizationId = orgId,
                UserId = userId,
                Category = AuditEventCategory.Secret,
                EventType = "SecretUpdated",
                EntityType = nameof(SecretReference),
                EntityId = secret.Id.ToString(),
                Summary = $"Updated secret '{secret.Name}'",
                IpAddress = httpContextService.IpAddress,
                CorrelationId = httpContextService.CorrelationId,
                OccurredAt = dateTimeService.UtcNow,
            },
            cancellationToken);

        return SecurityMapper.ToSecretResponse(secret);
    }
}

/// <summary>Deletes a secret.</summary>
public sealed class DeleteSecretCommand : IRequest<Unit>
{
    /// <summary>Gets the secret id.</summary>
    public Guid SecretId { get; init; }
}

/// <summary>Handles <see cref="DeleteSecretCommand"/>.</summary>
public sealed class DeleteSecretCommandHandler : IRequestHandler<DeleteSecretCommand, Unit>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly ISecretManager secretManager;
    private readonly IEnterpriseAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>Initializes a new instance of the <see cref="DeleteSecretCommandHandler"/> class.</summary>
    public DeleteSecretCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        ISecretManager secretManager,
        IEnterpriseAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.secretManager = secretManager;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteSecretCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.SecretsManage, cancellationToken);

        await secretManager.DeleteAsync(orgId, request.SecretId, cancellationToken);

        await auditService.AppendAsync(
            new EnterpriseAuditEntry
            {
                OrganizationId = orgId,
                UserId = userId,
                Category = AuditEventCategory.Secret,
                EventType = "SecretDeleted",
                EntityType = nameof(SecretReference),
                EntityId = request.SecretId.ToString(),
                Summary = "Deleted secret",
                IpAddress = httpContextService.IpAddress,
                CorrelationId = httpContextService.CorrelationId,
                OccurredAt = dateTimeService.UtcNow,
            },
            cancellationToken);

        return Unit.Value;
    }
}

/// <summary>Lists enterprise audit events.</summary>
public sealed class ListAuditEventsQuery : IRequest<IReadOnlyList<AuditEventResponse>>
{
    /// <summary>Gets optional category filter.</summary>
    public string? Category { get; init; }

    /// <summary>Gets optional event type filter.</summary>
    public string? EventType { get; init; }

    /// <summary>Gets optional from timestamp.</summary>
    public DateTime? FromUtc { get; init; }

    /// <summary>Gets optional to timestamp.</summary>
    public DateTime? ToUtc { get; init; }

    /// <summary>Gets max rows to return.</summary>
    public int Take { get; init; } = 100;
}

/// <summary>Handles <see cref="ListAuditEventsQuery"/>.</summary>
public sealed class ListAuditEventsQueryHandler : IRequestHandler<ListAuditEventsQuery, IReadOnlyList<AuditEventResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IEnterpriseAuditService auditService;

    /// <summary>Initializes a new instance of the <see cref="ListAuditEventsQueryHandler"/> class.</summary>
    public ListAuditEventsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IEnterpriseAuditService auditService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.auditService = auditService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AuditEventResponse>> Handle(ListAuditEventsQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.AuditRead, cancellationToken);

        AuditEventCategory? category = null;
        if (!string.IsNullOrWhiteSpace(request.Category) &&
            Enum.TryParse<AuditEventCategory>(request.Category, true, out var parsed))
        {
            category = parsed;
        }

        var entries = await auditService.QueryAsync(
            orgId,
            new AuditQueryRequest
            {
                Category = category,
                EventType = request.EventType,
                FromUtc = request.FromUtc,
                ToUtc = request.ToUtc,
                Take = request.Take <= 0 ? 100 : Math.Min(request.Take, 500),
            },
            cancellationToken);

        return entries.Select(SecurityMapper.ToAuditEventResponse).ToList();
    }
}

/// <summary>Gets a single audit event.</summary>
public sealed class GetAuditEventQuery : IRequest<AuditEventResponse>
{
    /// <summary>Gets the audit event id.</summary>
    public Guid AuditEventId { get; init; }
}

/// <summary>Handles <see cref="GetAuditEventQuery"/>.</summary>
public sealed class GetAuditEventQueryHandler : IRequestHandler<GetAuditEventQuery, AuditEventResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;

    /// <summary>Initializes a new instance of the <see cref="GetAuditEventQueryHandler"/> class.</summary>
    public GetAuditEventQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
    }

    /// <inheritdoc />
    public async Task<AuditEventResponse> Handle(GetAuditEventQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.AuditRead, cancellationToken);

        var entry = await db.AuditEvents.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.AuditEventId && e.OrganizationId == orgId, cancellationToken)
            ?? throw new NotFoundException("Audit event", request.AuditEventId);

        return SecurityMapper.ToAuditEventResponse(entry);
    }
}

/// <summary>Gets organization policies.</summary>
public sealed class GetPoliciesQuery : IRequest<OrganizationPoliciesResponse>
{
}

/// <summary>Handles <see cref="GetPoliciesQuery"/>.</summary>
public sealed class GetPoliciesQueryHandler : IRequestHandler<GetPoliciesQuery, OrganizationPoliciesResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;

    /// <summary>Initializes a new instance of the <see cref="GetPoliciesQueryHandler"/> class.</summary>
    public GetPoliciesQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
    }

    /// <inheritdoc />
    public async Task<OrganizationPoliciesResponse> Handle(GetPoliciesQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.PolicyRead, cancellationToken);

        var security = await db.OrganizationSecurityPolicies.AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId, cancellationToken);
        var governance = await db.OrganizationGovernancePolicies.AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId, cancellationToken);
        return SecurityMapper.ToPoliciesResponse(security, governance);
    }
}

/// <summary>Updates organization policies.</summary>
public sealed class UpdatePoliciesCommand : IRequest<OrganizationPoliciesResponse>
{
    /// <summary>Gets the update request.</summary>
    public UpdatePoliciesRequest Request { get; init; } = new();
}

/// <summary>Handles <see cref="UpdatePoliciesCommand"/>.</summary>
public sealed class UpdatePoliciesCommandHandler : IRequestHandler<UpdatePoliciesCommand, OrganizationPoliciesResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;
    private readonly IDateTimeService dateTimeService;
    private readonly IEnterpriseAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>Initializes a new instance of the <see cref="UpdatePoliciesCommandHandler"/> class.</summary>
    public UpdatePoliciesCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db,
        IDateTimeService dateTimeService,
        IEnterpriseAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
        this.dateTimeService = dateTimeService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<OrganizationPoliciesResponse> Handle(UpdatePoliciesCommand command, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.PolicyManage, cancellationToken);

        var now = dateTimeService.UtcNow;
        var request = command.Request;

        var security = await db.OrganizationSecurityPolicies
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId, cancellationToken);
        if (security is null)
        {
            security = new OrganizationSecurityPolicy
            {
                OrganizationId = orgId,
                CreatedAt = now,
            };
            await db.AddOrganizationSecurityPolicyAsync(security, cancellationToken);
        }

        if (request.Security is not null)
        {
            var s = request.Security;
            security.MinPasswordLength = s.MinPasswordLength;
            security.RequireUppercase = s.RequireUppercase;
            security.RequireDigit = s.RequireDigit;
            security.RequireNonAlphanumeric = s.RequireNonAlphanumeric;
            security.RequireMfa = s.RequireMfa;
            security.SessionTimeoutMinutes = s.SessionTimeoutMinutes;
            security.MaxConcurrentSessions = s.MaxConcurrentSessions;
            security.IpAllowListJson = SecurityMapper.ToJsonArray(s.IpAllowList);
            security.GeoAllowListJson = SecurityMapper.ToJsonArray(s.GeoAllowList);
            security.ApiKeyExpirationDays = s.ApiKeyExpirationDays;
            security.EnforceApiKeyRotation = s.EnforceApiKeyRotation;
            security.FailedLoginAlertThreshold = s.FailedLoginAlertThreshold;
            security.UpdatedAt = now;
        }

        var governance = await db.OrganizationGovernancePolicies
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId, cancellationToken);
        if (governance is null)
        {
            governance = new OrganizationGovernancePolicy
            {
                OrganizationId = orgId,
                CreatedAt = now,
            };
            await db.AddOrganizationGovernancePolicyAsync(governance, cancellationToken);
        }

        if (request.Governance is not null)
        {
            var g = request.Governance;
            governance.AllowedProvidersJson = SecurityMapper.ToJsonArray(g.AllowedProviders);
            governance.AllowedModelsJson = SecurityMapper.ToJsonArray(g.AllowedModels);
            governance.MaximumGpuCostPerHour = g.MaximumGpuCostPerHour;
            governance.MaximumRunningPods = g.MaximumRunningPods;
            governance.MaximumQueueSize = g.MaximumQueueSize;
            governance.MaximumDailySpendUsd = g.MaximumDailySpendUsd;
            governance.AllowedPluginsJson = SecurityMapper.ToJsonArray(g.AllowedPlugins);
            governance.AllowedMcpServersJson = SecurityMapper.ToJsonArray(g.AllowedMcpServers);
            governance.EmptyAllowListMeansAllowAll = g.EmptyAllowListMeansAllowAll;
            governance.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);

        await auditService.AppendAsync(
            new EnterpriseAuditEntry
            {
                OrganizationId = orgId,
                UserId = userId,
                Category = AuditEventCategory.Policy,
                EventType = "PoliciesUpdated",
                EntityType = "OrganizationPolicies",
                EntityId = orgId.ToString(),
                Summary = "Updated organization security/governance policies",
                IpAddress = httpContextService.IpAddress,
                CorrelationId = httpContextService.CorrelationId,
                OccurredAt = now,
            },
            cancellationToken);

        return SecurityMapper.ToPoliciesResponse(security, governance);
    }
}

/// <summary>Gets compliance status.</summary>
public sealed class GetComplianceQuery : IRequest<ComplianceStatusResponse>
{
}

/// <summary>Handles <see cref="GetComplianceQuery"/>.</summary>
public sealed class GetComplianceQueryHandler : IRequestHandler<GetComplianceQuery, ComplianceStatusResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IComplianceService complianceService;

    /// <summary>Initializes a new instance of the <see cref="GetComplianceQueryHandler"/> class.</summary>
    public GetComplianceQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IComplianceService complianceService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.complianceService = complianceService;
    }

    /// <inheritdoc />
    public async Task<ComplianceStatusResponse> Handle(GetComplianceQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.ComplianceRead, cancellationToken);
        var status = await complianceService.GetStatusAsync(orgId, cancellationToken);
        return SecurityMapper.ToComplianceResponse(status);
    }
}

/// <summary>Exports compliance/user data.</summary>
public sealed class ExportComplianceCommand : IRequest<ComplianceExportResult>
{
}

/// <summary>Handles <see cref="ExportComplianceCommand"/>.</summary>
public sealed class ExportComplianceCommandHandler : IRequestHandler<ExportComplianceCommand, ComplianceExportResult>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IComplianceService complianceService;

    /// <summary>Initializes a new instance of the <see cref="ExportComplianceCommandHandler"/> class.</summary>
    public ExportComplianceCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IComplianceService complianceService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.complianceService = complianceService;
    }

    /// <inheritdoc />
    public async Task<ComplianceExportResult> Handle(ExportComplianceCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.ComplianceManage, cancellationToken);
        return await complianceService.ExportAsync(orgId, userId, cancellationToken);
    }
}

/// <summary>Performs right-to-erasure for a user.</summary>
public sealed class EraseUserComplianceCommand : IRequest<Unit>
{
    /// <summary>Gets the target user id.</summary>
    public Guid TargetUserId { get; init; }
}

/// <summary>Handles <see cref="EraseUserComplianceCommand"/>.</summary>
public sealed class EraseUserComplianceCommandHandler : IRequestHandler<EraseUserComplianceCommand, Unit>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IComplianceService complianceService;

    /// <summary>Initializes a new instance of the <see cref="EraseUserComplianceCommandHandler"/> class.</summary>
    public EraseUserComplianceCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IComplianceService complianceService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.complianceService = complianceService;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(EraseUserComplianceCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.ComplianceManage, cancellationToken);
        await complianceService.EraseUserAsync(orgId, request.TargetUserId, userId, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Lists active sessions.</summary>
public sealed class ListSessionsQuery : IRequest<IReadOnlyList<SessionResponse>>
{
}

/// <summary>Handles <see cref="ListSessionsQuery"/>.</summary>
public sealed class ListSessionsQueryHandler : IRequestHandler<ListSessionsQuery, IReadOnlyList<SessionResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly ISessionTracker sessionTracker;

    /// <summary>Initializes a new instance of the <see cref="ListSessionsQueryHandler"/> class.</summary>
    public ListSessionsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        ISessionTracker sessionTracker)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.sessionTracker = sessionTracker;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SessionResponse>> Handle(ListSessionsQuery request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.SecurityRead, cancellationToken);
        var sessions = await sessionTracker.ListActiveAsync(orgId, cancellationToken);
        return sessions.Select(SecurityMapper.ToSessionResponse).ToList();
    }
}

/// <summary>Lists trusted devices for the current user.</summary>
public sealed class ListTrustedDevicesQuery : IRequest<IReadOnlyList<TrustedDeviceResponse>>
{
}

/// <summary>Handles <see cref="ListTrustedDevicesQuery"/>.</summary>
public sealed class ListTrustedDevicesQueryHandler : IRequestHandler<ListTrustedDevicesQuery, IReadOnlyList<TrustedDeviceResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;

    /// <summary>Initializes a new instance of the <see cref="ListTrustedDevicesQueryHandler"/> class.</summary>
    public ListTrustedDevicesQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TrustedDeviceResponse>> Handle(
        ListTrustedDevicesQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.SecurityRead, cancellationToken);

        var devices = await db.TrustedDevices.AsNoTracking()
            .Where(d => d.UserId == userId && (d.OrganizationId == null || d.OrganizationId == orgId))
            .OrderByDescending(d => d.LastSeenAt)
            .ToListAsync(cancellationToken);
        return devices.Select(SecurityMapper.ToTrustedDeviceResponse).ToList();
    }
}

/// <summary>Revokes a trusted device.</summary>
public sealed class RevokeTrustedDeviceCommand : IRequest<Unit>
{
    /// <summary>Gets the device id.</summary>
    public Guid DeviceId { get; init; }
}

/// <summary>Handles <see cref="RevokeTrustedDeviceCommand"/>.</summary>
public sealed class RevokeTrustedDeviceCommandHandler : IRequestHandler<RevokeTrustedDeviceCommand, Unit>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IApplicationDbContext db;
    private readonly IDateTimeService dateTimeService;

    /// <summary>Initializes a new instance of the <see cref="RevokeTrustedDeviceCommandHandler"/> class.</summary>
    public RevokeTrustedDeviceCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IApplicationDbContext db,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.db = db;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(RevokeTrustedDeviceCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.SecurityManage, cancellationToken);

        var device = await db.TrustedDevices
            .FirstOrDefaultAsync(
                d => d.Id == request.DeviceId && d.UserId == userId &&
                     (d.OrganizationId == null || d.OrganizationId == orgId),
                cancellationToken)
            ?? throw new NotFoundException("Trusted device", request.DeviceId);

        device.IsRevoked = true;
        device.UpdatedAt = dateTimeService.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Gets the security dashboard.</summary>
public sealed class GetSecurityDashboardQuery : IRequest<SecurityDashboardResponse>
{
}

/// <summary>Handles <see cref="GetSecurityDashboardQuery"/>.</summary>
public sealed class GetSecurityDashboardQueryHandler : IRequestHandler<GetSecurityDashboardQuery, SecurityDashboardResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly ISecurityDashboardService dashboardService;

    /// <summary>Initializes a new instance of the <see cref="GetSecurityDashboardQueryHandler"/> class.</summary>
    public GetSecurityDashboardQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        ISecurityDashboardService dashboardService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.dashboardService = dashboardService;
    }

    /// <inheritdoc />
    public async Task<SecurityDashboardResponse> Handle(
        GetSecurityDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.SecurityRead, cancellationToken);
        var dashboard = await dashboardService.GetAsync(orgId, cancellationToken);
        return SecurityMapper.ToDashboardResponse(dashboard);
    }
}

/// <summary>Upserts a SCIM user.</summary>
public sealed class UpsertScimUserCommand : IRequest<ScimUserResult>
{
    /// <summary>Gets the SCIM user request.</summary>
    public ScimUserRequest Request { get; init; } = new();
}

/// <summary>Handles <see cref="UpsertScimUserCommand"/>.</summary>
public sealed class UpsertScimUserCommandHandler : IRequestHandler<UpsertScimUserCommand, ScimUserResult>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IScimProvisioningService scimService;

    /// <summary>Initializes a new instance of the <see cref="UpsertScimUserCommandHandler"/> class.</summary>
    public UpsertScimUserCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IScimProvisioningService scimService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.scimService = scimService;
    }

    /// <inheritdoc />
    public async Task<ScimUserResult> Handle(UpsertScimUserCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.SecurityManage, cancellationToken);
        return await scimService.UpsertUserAsync(orgId, request.Request, cancellationToken);
    }
}

/// <summary>Disables a SCIM user.</summary>
public sealed class DisableScimUserCommand : IRequest<Unit>
{
    /// <summary>Gets the external user id.</summary>
    public string ExternalUserId { get; init; } = string.Empty;
}

/// <summary>Handles <see cref="DisableScimUserCommand"/>.</summary>
public sealed class DisableScimUserCommandHandler : IRequestHandler<DisableScimUserCommand, Unit>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IScimProvisioningService scimService;

    /// <summary>Initializes a new instance of the <see cref="DisableScimUserCommandHandler"/> class.</summary>
    public DisableScimUserCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IScimProvisioningService scimService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.scimService = scimService;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(DisableScimUserCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.SecurityManage, cancellationToken);
        await scimService.DisableUserAsync(orgId, request.ExternalUserId, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Syncs a SCIM group.</summary>
public sealed class SyncScimGroupCommand : IRequest<Unit>
{
    /// <summary>Gets the SCIM group request.</summary>
    public ScimGroupRequest Request { get; init; } = new();
}

/// <summary>Handles <see cref="SyncScimGroupCommand"/>.</summary>
public sealed class SyncScimGroupCommandHandler : IRequestHandler<SyncScimGroupCommand, Unit>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService auth;
    private readonly IScimProvisioningService scimService;

    /// <summary>Initializes a new instance of the <see cref="SyncScimGroupCommandHandler"/> class.</summary>
    public SyncScimGroupCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService auth,
        IScimProvisioningService scimService)
    {
        this.currentUserService = currentUserService;
        this.auth = auth;
        this.scimService = scimService;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(SyncScimGroupCommand request, CancellationToken cancellationToken)
    {
        var (userId, orgId) = SecurityAccess.RequireOrganizationContext(currentUserService);
        await SecurityAccess.EnsurePermissionAsync(auth, orgId, userId, PermissionNames.SecurityManage, cancellationToken);
        await scimService.SyncGroupAsync(orgId, request.Request, cancellationToken);
        return Unit.Value;
    }
}
