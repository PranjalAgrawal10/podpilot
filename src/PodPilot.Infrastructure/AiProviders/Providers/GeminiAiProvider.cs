using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders.Providers;

/// <summary>Google Gemini Generative Language API provider.</summary>
public sealed class GeminiAiProvider : IAiProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private static readonly IReadOnlyList<AiModelInfo> KnownModels =
    [
        new() { ModelName = "gemini-2.0-flash", DisplayName = "Gemini 2.0 Flash", SupportsStreaming = true, SupportsVision = true },
        new() { ModelName = "gemini-1.5-pro", DisplayName = "Gemini 1.5 Pro", SupportsStreaming = true, SupportsVision = true },
        new() { ModelName = "gemini-1.5-flash", DisplayName = "Gemini 1.5 Flash", SupportsStreaming = true, SupportsVision = true },
        new() { ModelName = "text-embedding-004", DisplayName = "Text Embedding 004", SupportsEmbeddings = true, SupportsStreaming = false },
    ];

    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<GeminiAiProvider> logger;

    /// <summary>Initializes a new instance of the <see cref="GeminiAiProvider"/> class.</summary>
    public GeminiAiProvider(IHttpClientFactory httpClientFactory, ILogger<GeminiAiProvider> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    public AiProviderKind ProviderKind => AiProviderKind.GoogleGemini;

    /// <inheritdoc />
    public async Task<IReadOnlyList<AiModelInfo>> ListModelsAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient(connection);
            using var response = await client.GetAsync($"v1beta/models?key={Uri.EscapeDataString(connection.ApiKey)}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return KnownModels;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("models", out var models))
            {
                return KnownModels;
            }

            var result = new List<AiModelInfo>();
            foreach (var model in models.EnumerateArray())
            {
                var name = model.TryGetProperty("name", out var n) ? n.GetString() : null;
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var shortName = name.StartsWith("models/", StringComparison.Ordinal)
                    ? name["models/".Length..]
                    : name;
                result.Add(new AiModelInfo
                {
                    ModelName = shortName,
                    DisplayName = model.TryGetProperty("displayName", out var dn) ? dn.GetString() ?? shortName : shortName,
                    SupportsStreaming = true,
                    SupportsEmbeddings = shortName.Contains("embedding", StringComparison.OrdinalIgnoreCase),
                });
            }

            return result.Count > 0 ? result : KnownModels;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Falling back to known Gemini models");
            return KnownModels;
        }
    }

    /// <inheritdoc />
    public async Task<AiChatResponse> ChatAsync(
        AiProviderConnection connection,
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(connection);
        var model = request.Model.StartsWith("models/", StringComparison.Ordinal) ? request.Model : $"models/{request.Model}";
        var path = $"v1beta/{model}:generateContent?key={Uri.EscapeDataString(connection.ApiKey)}";
        var payload = BuildGenerateContentPayload(request);
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(path, content, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gemini chat failed: {(int)response.StatusCode} {json[..Math.Min(json.Length, 500)]}");
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var text = new StringBuilder();
        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var candidate = candidates[0];
            if (candidate.TryGetProperty("content", out var c) &&
                c.TryGetProperty("parts", out var parts))
            {
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var t))
                    {
                        text.Append(t.GetString());
                    }
                }
            }
        }

        return new AiChatResponse
        {
            Id = Guid.NewGuid().ToString("N"),
            Model = request.Model,
            Content = text.ToString(),
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
        var result = await ChatAsync(connection, request, cancellationToken);
        var chunk = JsonSerializer.Serialize(new
        {
            id = result.Id,
            @object = "chat.completion.chunk",
            model = result.Model,
            choices = new[]
            {
                new { index = 0, delta = new { content = result.Content }, finish_reason = "stop" },
            },
        });
        var bytes = Encoding.UTF8.GetBytes($"data: {chunk}\n\ndata: [DONE]\n\n");
        await responseStream.WriteAsync(bytes, cancellationToken);
        await responseStream.FlushAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AiEmbeddingResponse> EmbeddingsAsync(
        AiProviderConnection connection,
        AiEmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(connection);
        var model = request.Model.StartsWith("models/", StringComparison.Ordinal) ? request.Model : $"models/{request.Model}";
        var path = $"v1beta/{model}:batchEmbedContents?key={Uri.EscapeDataString(connection.ApiKey)}";
        var requests = request.Input.Select(text => new
        {
            model,
            content = new { parts = new[] { new { text } } },
        }).ToArray();
        var payload = JsonSerializer.Serialize(new { requests }, JsonOptions);
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(path, content, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(json);
        var embeddings = new List<IReadOnlyList<float>>();
        if (doc.RootElement.TryGetProperty("embeddings", out var embArr))
        {
            foreach (var item in embArr.EnumerateArray())
            {
                if (!item.TryGetProperty("values", out var values))
                {
                    continue;
                }

                embeddings.Add(values.EnumerateArray().Select(v => v.GetSingle()).ToList());
            }
        }

        return new AiEmbeddingResponse { Model = request.Model, Embeddings = embeddings };
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
            using var response = await client.GetAsync(
                $"v1beta/models?key={Uri.EscapeDataString(connection.ApiKey)}",
                cancellationToken);
            sw.Stop();
            return new AiProviderHealthResult
            {
                IsHealthy = response.IsSuccessStatusCode,
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
        var health = await HealthAsync(connection, cancellationToken);
        return new AiCredentialValidationResult { IsValid = health.IsHealthy, Message = health.Message };
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
        var client = httpClientFactory.CreateClient(nameof(GeminiAiProvider));
        var baseUrl = string.IsNullOrWhiteSpace(connection.BaseUrl)
            ? "https://generativelanguage.googleapis.com"
            : connection.BaseUrl.TrimEnd('/');
        client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
        return client;
    }

    private static string BuildGenerateContentPayload(AiChatRequest request)
    {
        var contents = new List<object>();
        string? system = request.SystemPrompt;
        foreach (var message in request.Messages)
        {
            if (string.Equals(message.Role, "system", StringComparison.OrdinalIgnoreCase))
            {
                system = string.IsNullOrWhiteSpace(system) ? message.Content : system + "\n" + message.Content;
                continue;
            }

            var role = string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase) ? "model" : "user";
            contents.Add(new { role, parts = new[] { new { text = message.Content } } });
        }

        var payload = new Dictionary<string, object?> { ["contents"] = contents };
        if (!string.IsNullOrWhiteSpace(system))
        {
            payload["systemInstruction"] = new { parts = new[] { new { text = system } } };
        }

        var generationConfig = new Dictionary<string, object?>();
        if (request.Temperature.HasValue)
        {
            generationConfig["temperature"] = request.Temperature.Value;
        }

        if (request.MaxTokens.HasValue)
        {
            generationConfig["maxOutputTokens"] = request.MaxTokens.Value;
        }

        if (request.TopP.HasValue)
        {
            generationConfig["topP"] = request.TopP.Value;
        }

        if (generationConfig.Count > 0)
        {
            payload["generationConfig"] = generationConfig;
        }

        return JsonSerializer.Serialize(payload, JsonOptions);
    }
}
