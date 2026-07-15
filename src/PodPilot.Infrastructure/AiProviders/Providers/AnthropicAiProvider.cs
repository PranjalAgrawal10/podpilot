using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders.Providers;

/// <summary>Anthropic Messages API provider.</summary>
public sealed class AnthropicAiProvider : IAiProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private static readonly IReadOnlyList<AiModelInfo> KnownModels =
    [
        new() { ModelName = "claude-opus-4-20250514", DisplayName = "Claude Opus 4", SupportsStreaming = true, SupportsTools = true },
        new() { ModelName = "claude-sonnet-4-20250514", DisplayName = "Claude Sonnet 4", SupportsStreaming = true, SupportsTools = true },
        new() { ModelName = "claude-3-5-haiku-20241022", DisplayName = "Claude 3.5 Haiku", SupportsStreaming = true, SupportsTools = true },
        new() { ModelName = "claude-3-5-sonnet-20241022", DisplayName = "Claude 3.5 Sonnet", SupportsStreaming = true, SupportsTools = true },
    ];

    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<AnthropicAiProvider> logger;

    /// <summary>Initializes a new instance of the <see cref="AnthropicAiProvider"/> class.</summary>
    public AnthropicAiProvider(IHttpClientFactory httpClientFactory, ILogger<AnthropicAiProvider> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    public AiProviderKind ProviderKind => AiProviderKind.Anthropic;

    /// <inheritdoc />
    public Task<IReadOnlyList<AiModelInfo>> ListModelsAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(KnownModels);
    }

    /// <inheritdoc />
    public async Task<AiChatResponse> ChatAsync(
        AiProviderConnection connection,
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(connection);
        var payload = BuildMessagesPayload(request, stream: false);
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("v1/messages", content, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Anthropic chat failed: {(int)response.StatusCode} {json[..Math.Min(json.Length, 500)]}");
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var text = new StringBuilder();
        if (root.TryGetProperty("content", out var contentArr))
        {
            foreach (var block in contentArr.EnumerateArray())
            {
                if (block.TryGetProperty("type", out var type) &&
                    type.GetString() == "text" &&
                    block.TryGetProperty("text", out var t))
                {
                    text.Append(t.GetString());
                }
            }
        }

        int? promptTokens = null;
        int? completionTokens = null;
        if (root.TryGetProperty("usage", out var usage))
        {
            if (usage.TryGetProperty("input_tokens", out var it) && it.TryGetInt32(out var itv))
            {
                promptTokens = itv;
            }

            if (usage.TryGetProperty("output_tokens", out var ot) && ot.TryGetInt32(out var otv))
            {
                completionTokens = otv;
            }
        }

        return new AiChatResponse
        {
            Id = root.TryGetProperty("id", out var id) ? id.GetString() ?? Guid.NewGuid().ToString("N") : Guid.NewGuid().ToString("N"),
            Model = root.TryGetProperty("model", out var model) ? model.GetString() ?? request.Model : request.Model,
            Content = text.ToString(),
            FinishReason = root.TryGetProperty("stop_reason", out var sr) ? sr.GetString() : null,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            TotalTokens = promptTokens.HasValue && completionTokens.HasValue
                ? promptTokens.Value + completionTokens.Value
                : null,
            ProviderKind = ProviderKind,
        };
    }

    /// <inheritdoc />
    public async Task StreamChatAsync(
        AiProviderConnection connection,
        AiChatRequest request,
        Stream responseStream,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(connection);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/messages")
        {
            Content = new StringContent(BuildMessagesPayload(request, stream: true), Encoding.UTF8, "application/json"),
        };

        using var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        while (true)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:", StringComparison.Ordinal))
            {
                continue;
            }

            var data = line["data:".Length..].Trim();
            if (data is "[DONE]" or { Length: 0 })
            {
                continue;
            }

            try
            {
                using var doc = JsonDocument.Parse(data);
                var root = doc.RootElement;
                var type = root.TryGetProperty("type", out var t) ? t.GetString() : null;
                if (type == "content_block_delta" &&
                    root.TryGetProperty("delta", out var delta) &&
                    delta.TryGetProperty("text", out var text))
                {
                    var chunk = text.GetString() ?? string.Empty;
                    var openAiChunk = JsonSerializer.Serialize(new
                    {
                        id = "chatcmpl-anthropic",
                        @object = "chat.completion.chunk",
                        model = request.Model,
                        choices = new[]
                        {
                            new { index = 0, delta = new { content = chunk }, finish_reason = (string?)null },
                        },
                    });
                    var bytes = Encoding.UTF8.GetBytes($"data: {openAiChunk}\n\n");
                    await responseStream.WriteAsync(bytes, cancellationToken);
                    await responseStream.FlushAsync(cancellationToken);
                }
            }
            catch (JsonException ex)
            {
                logger.LogDebug(ex, "Skipping non-JSON Anthropic SSE line");
            }
        }

        var done = Encoding.UTF8.GetBytes("data: [DONE]\n\n");
        await responseStream.WriteAsync(done, cancellationToken);
        await responseStream.FlushAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<AiEmbeddingResponse> EmbeddingsAsync(
        AiProviderConnection connection,
        AiEmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        throw new NotSupportedException("Anthropic does not support embeddings via the Messages API.");
    }

    /// <inheritdoc />
    public async Task<AiProviderHealthResult> HealthAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var client = CreateClient(connection);
            using var response = await client.GetAsync("v1/models", cancellationToken);
            sw.Stop();
            return new AiProviderHealthResult
            {
                IsHealthy = response.IsSuccessStatusCode || (int)response.StatusCode is 401 or 403 or 404,
                LatencyMs = (int)sw.ElapsedMilliseconds,
                Message = response.IsSuccessStatusCode ? "OK" : $"HTTP {(int)response.StatusCode}",
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
    public async Task<AiCredentialValidationResult> ValidateCredentialsAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connection.ApiKey))
        {
            return new AiCredentialValidationResult { IsValid = false, Message = "API key is required." };
        }

        var health = await HealthAsync(connection, cancellationToken);
        return new AiCredentialValidationResult
        {
            IsValid = !string.IsNullOrWhiteSpace(connection.ApiKey) &&
                      (health.IsHealthy || health.Message?.Contains("401", StringComparison.Ordinal) != true),
            Message = health.Message,
        };
    }

    /// <inheritdoc />
    public Task<int?> CountTokensAsync(
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
        return Task.FromResult<int?>((int)Math.Ceiling(Math.Max(words * 1.3, text.Length / 4.0)));
    }

    private HttpClient CreateClient(AiProviderConnection connection)
    {
        var client = httpClientFactory.CreateClient(nameof(AnthropicAiProvider));
        var baseUrl = string.IsNullOrWhiteSpace(connection.BaseUrl)
            ? "https://api.anthropic.com"
            : connection.BaseUrl.TrimEnd('/');
        client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
        client.DefaultRequestHeaders.Remove("x-api-key");
        client.DefaultRequestHeaders.Remove("anthropic-version");
        client.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", connection.ApiKey);
        client.DefaultRequestHeaders.TryAddWithoutValidation("anthropic-version", "2023-06-01");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    private static string BuildMessagesPayload(AiChatRequest request, bool stream)
    {
        string? system = request.SystemPrompt;
        var messages = new List<object>();
        foreach (var message in request.Messages)
        {
            if (string.Equals(message.Role, "system", StringComparison.OrdinalIgnoreCase))
            {
                system = string.IsNullOrWhiteSpace(system) ? message.Content : system + "\n" + message.Content;
                continue;
            }

            messages.Add(new { role = message.Role == "assistant" ? "assistant" : "user", content = message.Content });
        }

        var payload = new Dictionary<string, object?>
        {
            ["model"] = request.Model,
            ["messages"] = messages,
            ["max_tokens"] = request.MaxTokens ?? 1024,
            ["stream"] = stream,
        };

        if (!string.IsNullOrWhiteSpace(system))
        {
            payload["system"] = system;
        }

        if (request.Temperature.HasValue)
        {
            payload["temperature"] = request.Temperature.Value;
        }

        if (request.TopP.HasValue)
        {
            payload["top_p"] = request.TopP.Value;
        }

        return JsonSerializer.Serialize(payload, JsonOptions);
    }
}
