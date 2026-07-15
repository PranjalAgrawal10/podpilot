using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders;

/// <summary>
/// Base implementation for OpenAI-compatible AI providers.
/// </summary>
public abstract class OpenAiCompatibleAiProvider : IAiProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAiCompatibleAiProvider"/> class.
    /// </summary>
    protected OpenAiCompatibleAiProvider(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    public abstract AiProviderKind ProviderKind { get; }

    /// <summary>Gets the default base URL for this provider.</summary>
    protected abstract string DefaultBaseUrl { get; }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<AiModelInfo>> ListModelsAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(connection);
        using var response = await client.GetAsync("models", cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "ListModels failed for {ProviderKind}: {StatusCode} {Body}",
                ProviderKind,
                (int)response.StatusCode,
                Truncate(json));
            response.EnsureSuccessStatusCode();
        }

        return MapModelList(json);
    }

    /// <inheritdoc />
    public virtual async Task<AiChatResponse> ChatAsync(
        AiProviderConnection connection,
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(connection);
        var payload = BuildChatPayload(request, stream: false);
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("chat/completions", content, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Chat failed for {ProviderKind}: {(int)response.StatusCode} {Truncate(json)}");
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var choice = root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0
            ? choices[0]
            : default;
        var messageContent = string.Empty;
        string? finishReason = null;
        if (choice.ValueKind != JsonValueKind.Undefined)
        {
            if (choice.TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var contentEl))
            {
                messageContent = contentEl.GetString() ?? string.Empty;
            }

            if (choice.TryGetProperty("finish_reason", out var fr))
            {
                finishReason = fr.GetString();
            }
        }

        int? promptTokens = null;
        int? completionTokens = null;
        int? totalTokens = null;
        if (root.TryGetProperty("usage", out var usage))
        {
            if (usage.TryGetProperty("prompt_tokens", out var pt) && pt.TryGetInt32(out var ptv))
            {
                promptTokens = ptv;
            }

            if (usage.TryGetProperty("completion_tokens", out var ct) && ct.TryGetInt32(out var ctv))
            {
                completionTokens = ctv;
            }

            if (usage.TryGetProperty("total_tokens", out var tt) && tt.TryGetInt32(out var ttv))
            {
                totalTokens = ttv;
            }
        }

        return new AiChatResponse
        {
            Id = root.TryGetProperty("id", out var id) ? id.GetString() ?? Guid.NewGuid().ToString("N") : Guid.NewGuid().ToString("N"),
            Model = root.TryGetProperty("model", out var model) ? model.GetString() ?? request.Model : request.Model,
            Content = messageContent,
            FinishReason = finishReason,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            TotalTokens = totalTokens,
            ProviderKind = ProviderKind,
        };
    }

    /// <inheritdoc />
    public virtual async Task StreamChatAsync(
        AiProviderConnection connection,
        AiChatRequest request,
        Stream responseStream,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(connection);
        var payload = BuildChatPayload(request, stream: true);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json"),
        };

        using var response = await client.SendAsync(
            httpRequest,
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

            if (!line.StartsWith("data:", StringComparison.Ordinal))
            {
                continue;
            }

            var data = line["data:".Length..].Trim();
            if (data == "[DONE]")
            {
                var doneBytes = Encoding.UTF8.GetBytes("data: [DONE]\n\n");
                await responseStream.WriteAsync(doneBytes, cancellationToken);
                await responseStream.FlushAsync(cancellationToken);
                break;
            }

            var outBytes = Encoding.UTF8.GetBytes($"data: {data}\n\n");
            await responseStream.WriteAsync(outBytes, cancellationToken);
            await responseStream.FlushAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public virtual async Task<AiEmbeddingResponse> EmbeddingsAsync(
        AiProviderConnection connection,
        AiEmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(connection);
        var payload = JsonSerializer.Serialize(
            new { model = request.Model, input = request.Input },
            JsonOptions);
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("embeddings", content, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Embeddings failed for {ProviderKind}: {(int)response.StatusCode} {Truncate(json)}");
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var embeddings = new List<IReadOnlyList<float>>();
        if (root.TryGetProperty("data", out var data))
        {
            foreach (var item in data.EnumerateArray())
            {
                if (!item.TryGetProperty("embedding", out var emb))
                {
                    continue;
                }

                var vector = new List<float>();
                foreach (var v in emb.EnumerateArray())
                {
                    vector.Add(v.GetSingle());
                }

                embeddings.Add(vector);
            }
        }

        int? totalTokens = null;
        if (root.TryGetProperty("usage", out var usage) &&
            usage.TryGetProperty("total_tokens", out var tt) &&
            tt.TryGetInt32(out var ttv))
        {
            totalTokens = ttv;
        }

        return new AiEmbeddingResponse
        {
            Model = root.TryGetProperty("model", out var model) ? model.GetString() ?? request.Model : request.Model,
            Embeddings = embeddings,
            TotalTokens = totalTokens,
        };
    }

    /// <inheritdoc />
    public virtual async Task<AiProviderHealthResult> HealthAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var client = CreateClient(connection);
            using var response = await client.GetAsync("models", cancellationToken);
            sw.Stop();
            if (response.IsSuccessStatusCode)
            {
                return new AiProviderHealthResult
                {
                    IsHealthy = true,
                    LatencyMs = (int)sw.ElapsedMilliseconds,
                    Message = "OK",
                };
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return new AiProviderHealthResult
            {
                IsHealthy = false,
                LatencyMs = (int)sw.ElapsedMilliseconds,
                Message = $"HTTP {(int)response.StatusCode}: {Truncate(body)}",
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new AiProviderHealthResult
            {
                IsHealthy = false,
                LatencyMs = (int)sw.ElapsedMilliseconds,
                Message = ex.Message,
            };
        }
    }

    /// <inheritdoc />
    public virtual async Task<AiCredentialValidationResult> ValidateCredentialsAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var health = await HealthAsync(connection, cancellationToken);
            return new AiCredentialValidationResult
            {
                IsValid = health.IsHealthy,
                Message = health.Message,
            };
        }
        catch (Exception ex)
        {
            return new AiCredentialValidationResult
            {
                IsValid = false,
                Message = ex.Message,
            };
        }
    }

    /// <inheritdoc />
    public virtual Task<int?> CountTokensAsync(
        AiProviderConnection connection,
        string model,
        string text,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(text))
        {
            return Task.FromResult<int?>(0);
        }

        var words = text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
        var byWords = (int)Math.Ceiling(words * 1.3);
        var byChars = (int)Math.Ceiling(text.Length / 4.0);
        return Task.FromResult<int?>(Math.Max(byWords, byChars));
    }

    /// <summary>
    /// Builds the Authorization header value.
    /// </summary>
    protected virtual AuthenticationHeaderValue? BuildAuthHeader(AiProviderConnection connection)
    {
        if (string.IsNullOrWhiteSpace(connection.ApiKey))
        {
            return null;
        }

        return new AuthenticationHeaderValue("Bearer", connection.ApiKey);
    }

    /// <summary>
    /// Maps a models list JSON payload into catalog entries.
    /// </summary>
    protected virtual IReadOnlyList<AiModelInfo> MapModelList(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (!root.TryGetProperty("data", out var data))
        {
            return [];
        }

        var models = new List<AiModelInfo>();
        foreach (var item in data.EnumerateArray())
        {
            var id = item.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            models.Add(new AiModelInfo
            {
                ModelName = id,
                DisplayName = id,
                SupportsStreaming = true,
                SupportsEmbeddings = id.Contains("embed", StringComparison.OrdinalIgnoreCase),
            });
        }

        return models;
    }

    /// <summary>
    /// Creates an HTTP client configured for the connection.
    /// </summary>
    protected HttpClient CreateClient(AiProviderConnection connection)
    {
        var client = httpClientFactory.CreateClient(nameof(OpenAiCompatibleAiProvider));
        var baseUrl = string.IsNullOrWhiteSpace(connection.BaseUrl) ? DefaultBaseUrl : connection.BaseUrl.TrimEnd('/');
        if (!baseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase) &&
            !baseUrl.Contains("/v1/", StringComparison.OrdinalIgnoreCase) &&
            DefaultBaseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
        {
            // keep as provided
        }

        client.BaseAddress = new Uri(EnsureTrailingSlash(baseUrl));
        client.DefaultRequestHeaders.Remove("Authorization");
        var auth = BuildAuthHeader(connection);
        if (auth is not null)
        {
            client.DefaultRequestHeaders.Authorization = auth;
        }

        return client;
    }

    /// <summary>
    /// Builds OpenAI chat completion JSON.
    /// </summary>
    protected static string BuildChatPayload(AiChatRequest request, bool stream)
    {
        var messages = new List<object>();
        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            messages.Add(new { role = "system", content = request.SystemPrompt });
        }

        foreach (var message in request.Messages)
        {
            messages.Add(new { role = message.Role, content = message.Content, name = message.Name });
        }

        var payload = new Dictionary<string, object?>
        {
            ["model"] = request.Model,
            ["messages"] = messages,
            ["stream"] = stream,
        };

        if (request.Temperature.HasValue)
        {
            payload["temperature"] = request.Temperature.Value;
        }

        if (request.MaxTokens.HasValue)
        {
            payload["max_tokens"] = request.MaxTokens.Value;
        }

        if (request.TopP.HasValue)
        {
            payload["top_p"] = request.TopP.Value;
        }

        if (request.Stop is { Count: > 0 })
        {
            payload["stop"] = request.Stop;
        }

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static string EnsureTrailingSlash(string url) =>
        url.EndsWith('/') ? url : url + "/";

    private static string Truncate(string value) =>
        value.Length <= 500 ? value : value[..500];
}
