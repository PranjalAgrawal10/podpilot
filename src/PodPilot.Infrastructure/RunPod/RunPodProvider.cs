using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Compute;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.RunPod;

/// <summary>
/// RunPod compute provider integration via GraphQL and REST APIs.
/// </summary>
public sealed class RunPodProvider : IComputeProvider
{
    private const string GraphQlEndpoint = "https://api.runpod.io/graphql";
    private const string RestBaseUrl = "https://rest.runpod.io/v1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IHttpClientFactory httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunPodProvider"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public RunPodProvider(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public ProviderType ProviderType => ProviderType.RunPod;

    /// <inheritdoc />
    public async Task<ProviderValidationResult> ValidateCredentialsAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await GetAccountInfoAsync(apiKey, cancellationToken);
            var regions = await ListRegionsAsync(apiKey, cancellationToken);
            var gpus = await ListGpusAsync(apiKey, cancellationToken);
            var templates = await ListTemplatesAsync(apiKey, cancellationToken);

            return new ProviderValidationResult
            {
                IsValid = true,
                ConnectionStatus = ProviderConnectionStatus.Connected,
                AccountInfo = account,
                Regions = regions,
                Gpus = gpus,
                Templates = templates,
            };
        }
        catch (Exception ex)
        {
            return new ProviderValidationResult
            {
                IsValid = false,
                ConnectionStatus = ProviderConnectionStatus.Disconnected,
                ErrorMessage = ex.Message,
            };
        }
    }

    /// <inheritdoc />
    public async Task<ProviderAccountInfo> GetAccountInfoAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        const string accountQuery = "query { myself { id email clientBalance } }";
        var response = await ExecuteGraphQlAsync(apiKey, accountQuery, cancellationToken);

        var myself = response.RootElement.GetProperty("data").GetProperty("myself");
        return new ProviderAccountInfo
        {
            AccountId = myself.GetProperty("id").GetString() ?? string.Empty,
            Email = myself.TryGetProperty("email", out var email) ? email.GetString() : null,
            DisplayName = myself.TryGetProperty("email", out var displayEmail) ? displayEmail.GetString() : null,
            Balance = myself.TryGetProperty("clientBalance", out var balance) && balance.ValueKind == JsonValueKind.Number
                ? balance.GetDecimal()
                : null,
            Currency = "USD",
        };
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ProviderRegionInfo>> ListRegionsAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ProviderRegionInfo> regions =
        [
            new() { RegionId = "US", Name = "United States", IsAvailable = true },
            new() { RegionId = "EU", Name = "Europe", IsAvailable = true },
            new() { RegionId = "CA", Name = "Canada", IsAvailable = true },
            new() { RegionId = "AP", Name = "Asia Pacific", IsAvailable = true },
        ];

        return Task.FromResult(regions);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProviderGpuInfo>> ListGpusAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var client = CreateRestClient(apiKey);
        using var response = await client.GetAsync($"{RestBaseUrl}/gpu-types", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return GetDefaultGpuCatalog();
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return GetDefaultGpuCatalog();
        }

        var gpus = new List<ProviderGpuInfo>();
        foreach (var element in document.RootElement.EnumerateArray())
        {
            var id = element.TryGetProperty("id", out var idProp)
                ? idProp.GetString()
                : element.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;

            var name = element.TryGetProperty("displayName", out var displayNameProp)
                ? displayNameProp.GetString()
                : element.TryGetProperty("name", out var gpuNameProp) ? gpuNameProp.GetString() : id;

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            gpus.Add(new ProviderGpuInfo
            {
                GpuId = id,
                Name = name,
                GpuType = MapGpuType(name),
                MemoryGb = element.TryGetProperty("memoryInGb", out var memoryProp) && memoryProp.TryGetInt32(out var memory)
                    ? memory
                    : null,
                IsAvailable = true,
            });
        }

        return gpus.Count > 0 ? gpus : GetDefaultGpuCatalog();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProviderTemplateInfo>> ListTemplatesAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var client = CreateRestClient(apiKey);
        using var response = await client.GetAsync($"{RestBaseUrl}/templates", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var templates = new List<ProviderTemplateInfo>();
        foreach (var element in document.RootElement.EnumerateArray())
        {
            var id = element.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
            var name = element.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            templates.Add(new ProviderTemplateInfo
            {
                TemplateId = id,
                Name = name,
                ImageName = element.TryGetProperty("imageName", out var imageProp) ? imageProp.GetString() : null,
                Description = element.TryGetProperty("description", out var descriptionProp)
                    ? descriptionProp.GetString()
                    : null,
            });
        }

        return templates;
    }

    /// <inheritdoc />
    public async Task<ProviderHealthResult> CheckHealthAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await GetAccountInfoAsync(apiKey, cancellationToken);
            stopwatch.Stop();

            return new ProviderHealthResult
            {
                Status = ProviderConnectionStatus.Connected,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                CheckedAt = DateTime.UtcNow,
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new ProviderHealthResult
            {
                Status = ProviderConnectionStatus.Disconnected,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ErrorMessage = ex.Message,
                CheckedAt = DateTime.UtcNow,
            };
        }
    }

    private async Task<JsonDocument> ExecuteGraphQlAsync(
        string apiKey,
        string query,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(nameof(RunPodProvider));
        var requestUri = $"{GraphQlEndpoint}?api_key={Uri.EscapeDataString(apiKey)}";
        using var response = await client.PostAsJsonAsync(
            requestUri,
            new { query },
            JsonOptions,
            cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"RunPod GraphQL request failed: {content}");
        }

        var document = JsonDocument.Parse(content);
        if (document.RootElement.TryGetProperty("errors", out var errors)
            && errors.ValueKind == JsonValueKind.Array
            && errors.GetArrayLength() > 0)
        {
            var message = errors[0].TryGetProperty("message", out var messageProp)
                ? messageProp.GetString()
                : "RunPod GraphQL request failed.";

            throw new InvalidOperationException(message);
        }

        return document;
    }

    private HttpClient CreateRestClient(string apiKey)
    {
        var client = httpClientFactory.CreateClient(nameof(RunPodProvider));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return client;
    }

    private static GpuType MapGpuType(string name)
    {
        var normalized = name.ToUpperInvariant();
        if (normalized.Contains("4090", StringComparison.Ordinal))
        {
            return GpuType.RTX4090;
        }

        if (normalized.Contains("5090", StringComparison.Ordinal))
        {
            return GpuType.RTX5090;
        }

        if (normalized.Contains("A100", StringComparison.Ordinal))
        {
            return GpuType.A100;
        }

        if (normalized.Contains("H100", StringComparison.Ordinal))
        {
            return GpuType.H100;
        }

        if (normalized.Contains("L40S", StringComparison.Ordinal))
        {
            return GpuType.L40S;
        }

        if (normalized.Contains("A40", StringComparison.Ordinal))
        {
            return GpuType.A40;
        }

        if (normalized.Contains("V100", StringComparison.Ordinal))
        {
            return GpuType.V100;
        }

        return GpuType.Custom;
    }

    private static IReadOnlyList<ProviderGpuInfo> GetDefaultGpuCatalog() =>
    [
        new() { GpuId = "NVIDIA GeForce RTX 4090", Name = "NVIDIA GeForce RTX 4090", GpuType = GpuType.RTX4090, MemoryGb = 24 },
        new() { GpuId = "NVIDIA A100 80GB", Name = "NVIDIA A100 80GB", GpuType = GpuType.A100, MemoryGb = 80 },
        new() { GpuId = "NVIDIA H100 80GB", Name = "NVIDIA H100 80GB", GpuType = GpuType.H100, MemoryGb = 80 },
        new() { GpuId = "NVIDIA L40S", Name = "NVIDIA L40S", GpuType = GpuType.L40S, MemoryGb = 48 },
    ];
}
