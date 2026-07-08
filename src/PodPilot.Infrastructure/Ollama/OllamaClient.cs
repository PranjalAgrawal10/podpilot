using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Ollama;

namespace PodPilot.Infrastructure.Ollama;

/// <summary>
/// HTTP client for Ollama APIs on GPU pods.
/// </summary>
public sealed class OllamaClient : IOllamaClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<OllamaClient> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaClient"/> class.
    /// </summary>
    public OllamaClient(IHttpClientFactory httpClientFactory, ILogger<OllamaClient> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<OllamaVersionResult> GetVersionAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        var client = CreateClient(TimeSpan.FromSeconds(15));
        using var response = await client.GetAsync(OllamaUrlHelper.Combine(baseUrl, "/api/version"), cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OllamaVersionPayload>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Ollama version response was empty.");

        return new OllamaVersionResult { Version = payload.Version };
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<OllamaModelTag>> ListModelsAsync(string baseUrl, CancellationToken cancellationToken = default) =>
        GetTagsAsync(baseUrl, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<OllamaModelTag>> GetTagsAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        var client = CreateClient(TimeSpan.FromSeconds(30));
        using var response = await client.GetAsync(OllamaUrlHelper.Combine(baseUrl, "/api/tags"), cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OllamaTagsPayload>(JsonOptions, cancellationToken)
            ?? new OllamaTagsPayload();

        return payload.Models
            .Select(m => new OllamaModelTag
            {
                Name = m.Name,
                Digest = m.Digest,
                Size = m.Size,
                ModifiedAt = m.ModifiedAt,
            })
            .ToList();
    }

    /// <inheritdoc />
    public async Task<OllamaModelDetails> ShowModelAsync(
        string baseUrl,
        string modelName,
        CancellationToken cancellationToken = default)
    {
        var client = CreateClient(TimeSpan.FromSeconds(30));
        using var request = new HttpRequestMessage(HttpMethod.Post, OllamaUrlHelper.Combine(baseUrl, "/api/show"))
        {
            Content = JsonContent.Create(new { name = modelName }),
        };

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return OllamaMetadataParser.ParseShowResponse(document.RootElement, modelName);
    }

    /// <inheritdoc />
    public async Task PullModelAsync(
        string baseUrl,
        string modelName,
        Func<OllamaPullProgress, Task> onProgress,
        CancellationToken cancellationToken = default)
    {
        var client = CreateClient(TimeSpan.FromHours(2));
        using var request = new HttpRequestMessage(HttpMethod.Post, OllamaUrlHelper.Combine(baseUrl, "/api/pull"))
        {
            Content = JsonContent.Create(new { name = modelName, stream = true }),
        };

        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            OllamaPullLine? pullLine;
            try
            {
                pullLine = JsonSerializer.Deserialize<OllamaPullLine>(line, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogDebug(ex, "Skipping unparsable Ollama pull line.");
                continue;
            }

            if (pullLine is null)
            {
                continue;
            }

            await onProgress(new OllamaPullProgress
            {
                Status = pullLine.Status ?? string.Empty,
                Completed = pullLine.Completed,
                Total = pullLine.Total,
            });

            if (string.Equals(pullLine.Status, "success", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }
    }

    /// <inheritdoc />
    public async Task DeleteModelAsync(string baseUrl, string modelName, CancellationToken cancellationToken = default)
    {
        var client = CreateClient(TimeSpan.FromMinutes(2));
        using var request = new HttpRequestMessage(HttpMethod.Delete, OllamaUrlHelper.Combine(baseUrl, "/api/delete"))
        {
            Content = JsonContent.Create(new { name = modelName }),
        };

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc />
    public async Task<OllamaGenerateResult> GenerateAsync(
        string baseUrl,
        string modelName,
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var client = CreateClient(TimeSpan.FromMinutes(2));
        using var request = new HttpRequestMessage(HttpMethod.Post, OllamaUrlHelper.Combine(baseUrl, "/api/generate"))
        {
            Content = JsonContent.Create(new
            {
                model = modelName,
                prompt,
                stream = false,
            }),
        };

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OllamaGeneratePayload>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Ollama generate response was empty.");

        return new OllamaGenerateResult
        {
            Response = payload.Response ?? string.Empty,
            Done = payload.Done,
        };
    }

    /// <inheritdoc />
    public async Task<OllamaEmbeddingsResult> EmbeddingsAsync(
        string baseUrl,
        string modelName,
        string input,
        CancellationToken cancellationToken = default)
    {
        var client = CreateClient(TimeSpan.FromMinutes(2));
        using var request = new HttpRequestMessage(HttpMethod.Post, OllamaUrlHelper.Combine(baseUrl, "/api/embeddings"))
        {
            Content = JsonContent.Create(new { model = modelName, prompt = input }),
        };

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OllamaEmbeddingsPayload>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Ollama embeddings response was empty.");

        return new OllamaEmbeddingsResult
        {
            Embedding = payload.Embedding ?? [],
        };
    }

    /// <inheritdoc />
    public async Task<bool> IsReachableAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient(TimeSpan.FromSeconds(10));
            using var response = await client.GetAsync(OllamaUrlHelper.Combine(baseUrl, "/api/version"), cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogDebug("Ollama is not reachable at {BaseUrl}: {Error}", baseUrl, ex.Message);
            return false;
        }
    }

    private HttpClient CreateClient(TimeSpan timeout)
    {
        var client = httpClientFactory.CreateClient(nameof(OllamaClient));
        client.Timeout = timeout;
        return client;
    }

    private sealed class OllamaVersionPayload
    {
        public string Version { get; set; } = string.Empty;
    }

    private sealed class OllamaTagsPayload
    {
        public List<OllamaTagItem> Models { get; set; } = [];
    }

    private sealed class OllamaTagItem
    {
        public string Name { get; set; } = string.Empty;

        public string? Digest { get; set; }

        public long Size { get; set; }

        public DateTime? ModifiedAt { get; set; }
    }

    private sealed class OllamaPullLine
    {
        public string? Status { get; set; }

        public long? Completed { get; set; }

        public long? Total { get; set; }
    }

    private sealed class OllamaGeneratePayload
    {
        public string? Response { get; set; }

        public bool Done { get; set; }
    }

    private sealed class OllamaEmbeddingsPayload
    {
        public List<float>? Embedding { get; set; }
    }
}
