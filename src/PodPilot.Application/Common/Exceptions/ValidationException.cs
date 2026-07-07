namespace PodPilot.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="failures">Validation failures grouped by property name.</param>
    public ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Gets validation errors grouped by property name.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }
}
