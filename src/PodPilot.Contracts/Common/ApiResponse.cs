namespace PodPilot.Contracts.Common;

/// <summary>
/// Standard API response envelope.
/// </summary>
/// <typeparam name="T">The response data type.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the request succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the response data.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets the error message when unsuccessful.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets validation or field-level errors.
    /// </summary>
    public IDictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Gets or sets the request correlation identifier.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <returns>A successful API response.</returns>
    public static ApiResponse<T> Ok(T data, string? correlationId = null) =>
        new() { Success = true, Data = data, CorrelationId = correlationId };

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <returns>A failed API response.</returns>
    public static ApiResponse<T> Fail(string message, string? correlationId = null) =>
        new() { Success = false, Message = message, CorrelationId = correlationId };
}
