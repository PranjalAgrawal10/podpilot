namespace PodPilot.Domain.Enums;

/// <summary>
/// Strategy used when an AI provider fails.
/// </summary>
public enum AiFailoverStrategy
{
    /// <summary>Do not fail over; return the error.</summary>
    None = 0,

    /// <summary>Retry the same provider, then fail over to fallbacks.</summary>
    RetryThenFailover = 1,

    /// <summary>Immediately fail over to the next configured provider.</summary>
    ImmediateFailover = 2,
}
