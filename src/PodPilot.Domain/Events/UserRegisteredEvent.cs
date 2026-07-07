namespace PodPilot.Domain.Events;

/// <summary>
/// Domain event raised when a new user registers.
/// </summary>
public sealed class UserRegisteredEvent : IDomainEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRegisteredEvent"/> class.
    /// </summary>
    /// <param name="userId">The registered user's identifier.</param>
    /// <param name="email">The registered user's email.</param>
    public UserRegisteredEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
        OccurredOn = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the registered user's identifier.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Gets the registered user's email.
    /// </summary>
    public string Email { get; }

    /// <inheritdoc />
    public DateTime OccurredOn { get; }
}
