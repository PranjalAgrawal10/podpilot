using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Gateway;
using PodPilot.Contracts.Health;
using PodPilot.Contracts.Lifecycle;
using PodPilot.Contracts.Models;
using PodPilot.Contracts.Pods;
using PodPilot.Contracts.Providers;

namespace PodPilot.Cli;

internal sealed class ApiClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient http;
    private readonly string baseUrl;

    public ApiClient(string baseUrl, string? accessToken = null)
    {
        this.baseUrl = baseUrl.TrimEnd('/');
        this.http = new HttpClient { BaseAddress = new Uri(this.baseUrl + "/") };
        this.http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            this.http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }

    public string BaseUrl => this.baseUrl;

    public async Task<AuthResponse> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var request = new LoginRequest { Email = email, Password = password };
        using var response = await this.http.PostAsJsonAsync("api/v1/auth/login", request, JsonOptions, cancellationToken);
        return await ReadDataAsync<AuthResponse>(response, cancellationToken);
    }

    public async Task<ProviderResponse> AddProviderAsync(
        string name,
        string providerType,
        string displayName,
        string apiKey,
        string? defaultRegion,
        CancellationToken cancellationToken)
    {
        var body = new CreateProviderRequest
        {
            Name = name,
            ProviderType = providerType,
            DisplayName = displayName,
            ApiKey = apiKey,
            DefaultRegion = defaultRegion,
            IsEnabled = true,
        };

        using var response = await this.http.PostAsJsonAsync("api/v1/providers", body, JsonOptions, cancellationToken);
        return await ReadDataAsync<ProviderResponse>(response, cancellationToken);
    }

    public async Task<PodResponse> CreatePodAsync(CreatePodRequest body, CancellationToken cancellationToken)
    {
        using var response = await this.http.PostAsJsonAsync("api/v1/pods", body, JsonOptions, cancellationToken);
        return await ReadDataAsync<PodResponse>(response, cancellationToken);
    }

    public async Task<ModelDownloadResponse> PullModelAsync(Guid podId, string model, CancellationToken cancellationToken)
    {
        var body = new PullModelRequest { PodId = podId, Model = model };
        using var response = await this.http.PostAsJsonAsync("api/v1/models/pull", body, JsonOptions, cancellationToken);
        return await ReadDataAsync<ModelDownloadResponse>(response, cancellationToken);
    }

    public async Task<HealthResponse> GetHealthAsync(CancellationToken cancellationToken)
    {
        using var response = await this.http.GetAsync("api/v1/health", cancellationToken);
        return await ReadDataAsync<HealthResponse>(response, cancellationToken);
    }

    public async Task<GatewayStatsResponse> GetGatewayStatsAsync(CancellationToken cancellationToken)
    {
        using var response = await this.http.GetAsync("api/v1/gateway/stats", cancellationToken);
        return await ReadDataAsync<GatewayStatsResponse>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<PodResponse>> ListPodsAsync(CancellationToken cancellationToken)
    {
        using var response = await this.http.GetAsync("api/v1/pods", cancellationToken);
        return await ReadDataAsync<IReadOnlyList<PodResponse>>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<PodActivityResponse>> GetPodActivityAsync(
        Guid podId,
        CancellationToken cancellationToken)
    {
        using var response = await this.http.GetAsync($"api/v1/pods/{podId}/activity", cancellationToken);
        return await ReadDataAsync<IReadOnlyList<PodActivityResponse>>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<GatewayRequestSummaryResponse>> ListGatewayRequestsAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        using var response = await this.http.GetAsync($"api/v1/gateway/requests?limit={limit}", cancellationToken);
        return await ReadDataAsync<IReadOnlyList<GatewayRequestSummaryResponse>>(response, cancellationToken);
    }

    public void Dispose() => this.http.Dispose();

    private static async Task<T> ReadDataAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new CliException(
                $"Empty response from API (HTTP {(int)response.StatusCode} {response.ReasonPhrase}).");
        }

        ApiResponse<T>? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<ApiResponse<T>>(raw, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new CliException(
                $"Failed to parse API response (HTTP {(int)response.StatusCode}): {Truncate(raw)}",
                ex);
        }

        if (envelope is null)
        {
            throw new CliException($"Unexpected empty envelope (HTTP {(int)response.StatusCode}).");
        }

        if (!response.IsSuccessStatusCode || !envelope.Success)
        {
            var message = BuildErrorMessage(response, envelope, raw);
            throw new CliException(message);
        }

        if (envelope.Data is null)
        {
            throw new CliException("API returned success without data.");
        }

        return envelope.Data;
    }

    private static string BuildErrorMessage<T>(HttpResponseMessage response, ApiResponse<T> envelope, string raw)
    {
        var sb = new StringBuilder();
        sb.Append($"API error HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
        if (!string.IsNullOrWhiteSpace(envelope.Message))
        {
            sb.Append($": {envelope.Message}");
        }

        if (envelope.Errors is { Count: > 0 })
        {
            foreach (var pair in envelope.Errors)
            {
                sb.Append($" | {pair.Key}: {string.Join(", ", pair.Value)}");
            }
        }

        if (string.IsNullOrWhiteSpace(envelope.Message) && (envelope.Errors is null || envelope.Errors.Count == 0))
        {
            sb.Append($" — {Truncate(raw)}");
        }

        if (!string.IsNullOrWhiteSpace(envelope.CorrelationId))
        {
            sb.Append($" (correlationId={envelope.CorrelationId})");
        }

        return sb.ToString();
    }

    private static string Truncate(string value, int max = 400) =>
        value.Length <= max ? value : value[..max] + "…";
}

internal sealed class CliException : Exception
{
    public CliException(string message)
        : base(message)
    {
    }

    public CliException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
