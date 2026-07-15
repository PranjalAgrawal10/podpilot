namespace PodPilot.Domain.Enums;

/// <summary>
/// Authentication protocol used by an identity provider.
/// </summary>
public enum IdentityProtocol
{
    /// <summary>OpenID Connect.</summary>
    Oidc = 0,

    /// <summary>OAuth 2.0 (without full OIDC).</summary>
    OAuth2 = 1,

    /// <summary>SAML 2.0.</summary>
    Saml2 = 2,
}
