using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PodPilot.Sdk;

/// <summary>
/// Minimal typed client for PodPilot HTTP APIs.
/// </summary>
public sealed class PodPilotClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient http;
    private readonly bool ownsHttp;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodPilotClient"/> class.
    /// </summary>
    /// <param name="baseUrl">API base URL (e.g. http://localhost:5000).</param>
    /// <param name="accessToken">Optional bearer token.</param>
    public PodPilotClient(string baseUrl, string? accessToken = null)
        : this(CreateHttpClient(baseUrl, accessToken), ownsHttp: true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PodPilotClient"/> class using an existing HttpClient.
    /// </summary>
    /// <param name="httpClient">Configured HttpClient with BaseAddress set.</param>
    /// <param name="ownsHttp">Whether this instance should dispose the client.</param>
    public PodPilotClient(HttpClient httpClient, bool ownsHttp = false)
    {
        this.http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.ownsHttp = ownsHttp;
    }

    /// <summary>
    /// Authenticates with email and password.
    /// </summary>
    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var payload = new { email, password };
        using var response = await this.http.PostAsJsonAsync("api/v1/auth/login", payload, JsonOptions, cancellationToken);
        var envelope = await ReadEnvelopeAsync<AuthResult>(response, cancellationToken);
        if (!string.IsNullOrWhiteSpace(envelope.AccessToken))
        {
            this.http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", envelope.AccessToken);
        }

        return envelope;
    }

    /// <summary>
    /// Lists GPU pods for the current organization.
    /// </summary>
    public async Task<IReadOnlyList<PodSummary>> ListPodsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await this.http.GetAsync("api/v1/pods", cancellationToken);
        return await ReadEnvelopeAsync<IReadOnlyList<PodSummary>>(response, cancellationToken);
    }

    /// <summary>
    /// Returns API health status.
    /// </summary>
    public async Task<HealthStatus> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        using var response = await this.http.GetAsync("api/v1/health", cancellationToken);
        return await ReadEnvelopeAsync<HealthStatus>(response, cancellationToken);
    }

    /// <summary>
    /// Returns gateway dashboard statistics (authenticated).
    /// </summary>
    public async Task<GatewayHealth> GetGatewayStatsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await this.http.GetAsync("api/v1/gateway/stats", cancellationToken);
        return await ReadEnvelopeAsync<GatewayHealth>(response, cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (this.ownsHttp)
        {
            this.http.Dispose();
        }
    }

    private static HttpClient CreateHttpClient(string baseUrl, string? accessToken)
    {
        var client = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return client;
    }

    private static async Task<T> ReadEnvelopeAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        var envelope = JsonSerializer.Deserialize<ApiEnvelope<T>>(raw, JsonOptions)
            ?? throw new PodPilotSdkException("Empty API response.");

        if (!response.IsSuccessStatusCode || !envelope.Success || envelope.Data is null)
        {
            throw new PodPilotSdkException(
                envelope.Message
                ?? $"HTTP {(int)response.StatusCode}: {Truncate(raw)}");
        }

        return envelope.Data;
    }

    private static string Truncate(string value, int max = 300) =>
        value.Length <= max ? value : value[..max] + "…";
}

internal sealed class ApiEnvelope<T>
{
    public bool Success { get; set; }

    public T? Data { get; set; }

    public string? Message { get; set; }
}

/// <summary>Authentication result.</summary>
public sealed class AuthResult
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public int ExpiresIn { get; set; }

    public string TokenType { get; set; } = "Bearer";
}

/// <summary>Pod list item.</summary>
public sealed class PodSummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

/// <summary>Health payload.</summary>
public sealed class HealthStatus
{
    public string Status { get; set; } = string.Empty;

    public TimeSpan TotalDuration { get; set; }
}

/// <summary>Gateway stats used as a health signal.</summary>
public sealed class GatewayHealth
{
    public int ActiveRequests { get; set; }

    public int RecentErrors { get; set; }

    public double AverageLatencyMs { get; set; }
}

/// <summary>SDK exception for failed API calls.</summary>
public sealed class PodPilotSdkException : Exception
{
    public PodPilotSdkException(string message)
        : base(message)
    {
    }
}
