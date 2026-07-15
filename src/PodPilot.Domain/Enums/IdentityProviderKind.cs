namespace PodPilot.Domain.Enums;

/// <summary>
/// Supported enterprise identity provider kinds.
/// </summary>
public enum IdentityProviderKind
{
    /// <summary>Microsoft Entra ID (Azure AD).</summary>
    EntraId = 0,

    /// <summary>Google Workspace.</summary>
    Google = 1,

    /// <summary>Okta.</summary>
    Okta = 2,

    /// <summary>Auth0.</summary>
    Auth0 = 3,

    /// <summary>Custom OpenID Connect provider.</summary>
    CustomOidc = 4,

    /// <summary>SAML 2.0 identity provider.</summary>
    Saml = 5,
}
