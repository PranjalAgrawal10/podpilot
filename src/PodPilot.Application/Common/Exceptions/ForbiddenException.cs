namespace PodPilot.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when the user lacks permission for an action.
/// </summary>
public sealed class ForbiddenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    public ForbiddenException()
        : base("You do not have permission to perform this action.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ForbiddenException(string message)
        : base(message)
    {
    }
}
