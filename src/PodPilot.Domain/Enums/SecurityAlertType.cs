namespace PodPilot.Domain.Enums;

/// <summary>
/// Types of security alerts raised by governance controls.
/// </summary>
public enum SecurityAlertType
{
    /// <summary>Repeated failed login attempts.</summary>
    FailedLoginAttempts = 0,

    /// <summary>Login from a suspicious IP.</summary>
    SuspiciousIp = 1,

    /// <summary>Excessive API usage.</summary>
    ExcessiveApiUsage = 2,

    /// <summary>Unauthorized provider change attempt.</summary>
    UnauthorizedProviderChange = 3,

    /// <summary>Secret access failure.</summary>
    SecretAccessFailure = 4,

    /// <summary>Organization policy violation.</summary>
    PolicyViolation = 5,

    /// <summary>New login from an unrecognized device.</summary>
    NewLogin = 6,

    /// <summary>Provider credential changed.</summary>
    ProviderCredentialChange = 7,
}
