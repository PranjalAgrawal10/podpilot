namespace PodPilot.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when authentication or authorization fails.
/// </summary>
public class UnauthorizedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    public UnauthorizedException()
        : base("Unauthorized access.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
