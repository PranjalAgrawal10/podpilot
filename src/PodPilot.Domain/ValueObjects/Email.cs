using System.Text.RegularExpressions;

namespace PodPilot.Domain.ValueObjects;

/// <summary>
/// Represents a validated email address value object.
/// </summary>
public sealed partial class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex = MyRegex();

    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> class.
    /// </summary>
    /// <param name="value">The email address value.</param>
    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the email address value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an <see cref="Email"/> from a string value.
    /// </summary>
    /// <param name="value">The raw email string.</param>
    /// <returns>A validated <see cref="Email"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the email format is invalid.</exception>
    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email address is required.", nameof(value));
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalized))
        {
            throw new ArgumentException("Email address format is invalid.", nameof(value));
        }

        return new Email(normalized);
    }

    /// <summary>
    /// Attempts to create an <see cref="Email"/> from a string value.
    /// </summary>
    /// <param name="value">The raw email string.</param>
    /// <param name="email">The resulting email when valid.</param>
    /// <returns><c>true</c> when the email is valid; otherwise <c>false</c>.</returns>
    public static bool TryCreate(string value, out Email? email)
    {
        try
        {
            email = Create(value);
            return true;
        }
        catch (ArgumentException)
        {
            email = null;
            return false;
        }
    }

    /// <inheritdoc />
    public bool Equals(Email? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Email other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MyRegex();
}
