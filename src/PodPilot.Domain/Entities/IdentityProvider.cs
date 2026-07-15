using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Organization-scoped SSO identity provider configuration.
/// </summary>
public class IdentityProvider : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the provider kind.</summary>
    public IdentityProviderKind ProviderKind { get; set; }

    /// <summary>Gets or sets the protocol.</summary>
    public IdentityProtocol Protocol { get; set; }

    /// <summary>Gets or sets whether the provider is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Gets or sets the OIDC/OAuth client identifier.</summary>
    public string? ClientId { get; set; }

    /// <summary>Gets or sets the encrypted client secret.</summary>
    public string? EncryptedClientSecret { get; set; }

    /// <summary>Gets or sets the issuer / authority URL.</summary>
    public string? Issuer { get; set; }

    /// <summary>Gets or sets the authorization endpoint.</summary>
    public string? AuthorizationEndpoint { get; set; }

    /// <summary>Gets or sets the token endpoint.</summary>
    public string? TokenEndpoint { get; set; }

    /// <summary>Gets or sets the JWKS URI.</summary>
    public string? JwksUri { get; set; }

    /// <summary>Gets or sets the SAML entity identifier.</summary>
    public string? SamlEntityId { get; set; }

    /// <summary>Gets or sets the SAML SSO URL.</summary>
    public string? SamlSsoUrl { get; set; }

    /// <summary>Gets or sets the encrypted SAML certificate/metadata.</summary>
    public string? EncryptedSamlCertificate { get; set; }

    /// <summary>Gets or sets space-delimited OIDC scopes.</summary>
    public string Scopes { get; set; } = "openid profile email";

    /// <summary>Gets or sets the callback path override.</summary>
    public string? CallbackPath { get; set; }

    /// <summary>Gets the organization.</summary>
    public Organization Organization { get; set; } = null!;
}
