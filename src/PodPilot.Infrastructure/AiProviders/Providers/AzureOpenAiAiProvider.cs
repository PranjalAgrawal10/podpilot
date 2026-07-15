using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders.Providers;

/// <summary>Azure OpenAI provider using deployment-based paths.</summary>
public sealed class AzureOpenAiAiProvider : OpenAiCompatibleAiProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Initializes a new instance of the <see cref="AzureOpenAiAiProvider"/> class.</summary>
    public AzureOpenAiAiProvider(IHttpClientFactory httpClientFactory, ILogger<AzureOpenAiAiProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override AiProviderKind ProviderKind => AiProviderKind.AzureOpenAi;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => string.Empty;

    /// <inheritdoc />
    public override Task<IReadOnlyList<AiModelInfo>> ListModelsAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!string.IsNullOrWhiteSpace(connection.DeploymentName))
        {
            IReadOnlyList<AiModelInfo> models =
            [
                new AiModelInfo
                {
                    ModelName = connection.DeploymentName,
                    DisplayName = connection.DeploymentName,
                    SupportsStreaming = true,
                },
            ];
            return Task.FromResult(models);
        }

        return Task.FromResult<IReadOnlyList<AiModelInfo>>([]);
    }

    /// <inheritdoc />
    public override async Task<AiChatResponse> ChatAsync(
        AiProviderConnection connection,
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateAzureClient(connection);
        var deployment = connection.DeploymentName ?? request.Model;
        var apiVersion = connection.ApiVersion ?? "2024-02-15-preview";
        var path = $"openai/deployments/{Uri.EscapeDataString(deployment)}/chat/completions?api-version={Uri.EscapeDataString(apiVersion)}";
        var chatRequest = new AiChatRequest
        {
            Model = deployment,
            Messages = request.Messages,
            Temperature = request.Temperature,
            MaxTokens = request.MaxTokens,
            TopP = request.TopP,
            Stream = false,
            SystemPrompt = request.SystemPrompt,
            Stop = request.Stop,
        };
        var payload = BuildChatPayload(chatRequest, stream: false);
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(path, content, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var messageContent = string.Empty;
        string? finishReason = null;
        if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
        {
            var choice = choices[0];
            if (choice.TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var c))
            {
                messageContent = c.GetString() ?? string.Empty;
            }

            if (choice.TryGetProperty("finish_reason", out var fr))
            {
                finishReason = fr.GetString();
            }
        }

        return new AiChatResponse
        {
            Id = root.TryGetProperty("id", out var id) ? id.GetString() ?? Guid.NewGuid().ToString("N") : Guid.NewGuid().ToString("N"),
            Model = deployment,
            Content = messageContent,
            FinishReason = finishReason,
            ProviderKind = ProviderKind,
        };
    }

    /// <inheritdoc />
    public override async Task StreamChatAsync(
        AiProviderConnection connection,
        AiChatRequest request,
        Stream responseStream,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateAzureClient(connection);
        var deployment = connection.DeploymentName ?? request.Model;
        var apiVersion = connection.ApiVersion ?? "2024-02-15-preview";
        var path = $"openai/deployments/{Uri.EscapeDataString(deployment)}/chat/completions?api-version={Uri.EscapeDataString(apiVersion)}";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(
                BuildChatPayload(
                    new AiChatRequest
                    {
                        Model = deployment,
                        Messages = request.Messages,
                        Temperature = request.Temperature,
                        MaxTokens = request.MaxTokens,
                        TopP = request.TopP,
                        Stream = true,
                        SystemPrompt = request.SystemPrompt,
                        Stop = request.Stop,
                    },
                    stream: true),
                Encoding.UTF8,
                "application/json"),
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

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var bytes = Encoding.UTF8.GetBytes(line + "\n");
            await responseStream.WriteAsync(bytes, cancellationToken);
            await responseStream.FlushAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public override async Task<AiEmbeddingResponse> EmbeddingsAsync(
        AiProviderConnection connection,
        AiEmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateAzureClient(connection);
        var deployment = connection.DeploymentName ?? request.Model;
        var apiVersion = connection.ApiVersion ?? "2024-02-15-preview";
        var path = $"openai/deployments/{Uri.EscapeDataString(deployment)}/embeddings?api-version={Uri.EscapeDataString(apiVersion)}";
        var payload = JsonSerializer.Serialize(new { input = request.Input }, JsonOptions);
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(path, content, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(json);
        var embeddings = new List<IReadOnlyList<float>>();
        if (doc.RootElement.TryGetProperty("data", out var data))
        {
            foreach (var item in data.EnumerateArray())
            {
                if (!item.TryGetProperty("embedding", out var emb))
                {
                    continue;
                }

                var vector = emb.EnumerateArray().Select(v => v.GetSingle()).ToList();
                embeddings.Add(vector);
            }
        }

        return new AiEmbeddingResponse { Model = deployment, Embeddings = embeddings };
    }

    /// <inheritdoc />
    public override async Task<AiProviderHealthResult> HealthAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (string.IsNullOrWhiteSpace(connection.BaseUrl) || string.IsNullOrWhiteSpace(connection.ApiKey))
            {
                return new AiProviderHealthResult { IsHealthy = false, Message = "Base URL and API key are required." };
            }

            using var client = CreateAzureClient(connection);
            var apiVersion = connection.ApiVersion ?? "2024-02-15-preview";
            using var response = await client.GetAsync($"openai/models?api-version={Uri.EscapeDataString(apiVersion)}", cancellationToken);
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
    protected override AuthenticationHeaderValue? BuildAuthHeader(AiProviderConnection connection) => null;

    private HttpClient CreateAzureClient(AiProviderConnection connection)
    {
        var client = CreateClient(connection);
        client.DefaultRequestHeaders.Remove("api-key");
        if (!string.IsNullOrWhiteSpace(connection.ApiKey))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("api-key", connection.ApiKey);
        }

        return client;
    }
}
