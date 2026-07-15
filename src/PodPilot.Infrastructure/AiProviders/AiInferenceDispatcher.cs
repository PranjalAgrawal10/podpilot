using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders;

/// <summary>
/// Dispatches inference through AI providers with retry and failover.
/// </summary>
public sealed class AiInferenceDispatcher : IAiInferenceDispatcher
{
    private readonly IAiProviderFactory providerFactory;
    private readonly IAiRequestMapper requestMapper;
    private readonly IAiResponseMapper responseMapper;
    private readonly IAiFailoverService failoverService;
    private readonly IRoutingNotificationService routingNotificationService;
    private readonly ILogger<AiInferenceDispatcher> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiInferenceDispatcher"/> class.
    /// </summary>
    public AiInferenceDispatcher(
        IAiProviderFactory providerFactory,
        IAiRequestMapper requestMapper,
        IAiResponseMapper responseMapper,
        IAiFailoverService failoverService,
        IRoutingNotificationService routingNotificationService,
        ILogger<AiInferenceDispatcher> logger)
    {
        this.providerFactory = providerFactory;
        this.requestMapper = requestMapper;
        this.responseMapper = responseMapper;
        this.failoverService = failoverService;
        this.routingNotificationService = routingNotificationService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<AiDispatchResult> DispatchAsync(
        AiDispatchContext context,
        CancellationToken cancellationToken = default)
    {
        var path = context.Path.Trim('/');
        var isEmbeddings = path.Contains("embeddings", StringComparison.OrdinalIgnoreCase);
        var isStream = context.Stream ||
                       path.Contains("stream", StringComparison.OrdinalIgnoreCase) ||
                       LooksLikeStream(context.BodyJson);

        var connections = new List<AiProviderConnection> { context.Route.Connection };
        connections.AddRange(context.Route.FallbackConnections);

        Exception? lastError = null;
        var failoverOccurred = false;

        for (var i = 0; i < connections.Count; i++)
        {
            var connection = connections[i];
            var maxAttempts = i == 0 && context.Route.FailoverStrategy == AiFailoverStrategy.RetryThenFailover
                ? Math.Max(1, context.Route.MaxRetries + 1)
                : 1;

            if (i == 0 && context.Route.FailoverStrategy == AiFailoverStrategy.ImmediateFailover)
            {
                maxAttempts = 1;
            }

            if (i == 0 && context.Route.FailoverStrategy == AiFailoverStrategy.None)
            {
                maxAttempts = Math.Max(1, context.Route.MaxRetries + 1);
            }

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    logger.LogInformation(
                        "Routing AI request via {ProviderKind} provider {ProviderId} model {Model}",
                        connection.ProviderKind,
                        connection.ProviderId,
                        context.Route.Model);

                    if (isEmbeddings)
                    {
                        await DispatchEmbeddingsAsync(connection, context, cancellationToken);
                    }
                    else if (isStream)
                    {
                        await DispatchStreamAsync(connection, context, cancellationToken);
                    }
                    else
                    {
                        await DispatchChatAsync(connection, context, cancellationToken);
                    }

                    if (i > 0)
                    {
                        failoverOccurred = true;
                        var reason = lastError?.Message ?? "Primary provider failed";
                        await failoverService.RecordFailoverAsync(
                            context.OrganizationId,
                            context.Route.Connection.ProviderId,
                            connection.ProviderId,
                            context.Route.Model,
                            reason,
                            succeeded: true,
                            context.GatewayRequestId,
                            cancellationToken);
                        await routingNotificationService.NotifyFallbackOccurredAsync(
                            context.OrganizationId,
                            context.Route.Connection.ProviderId,
                            connection.ProviderId,
                            context.Route.Model,
                            reason,
                            cancellationToken);
                        await routingNotificationService.NotifyProviderChangedAsync(
                            context.OrganizationId,
                            context.Route.Connection.ProviderId,
                            connection.ProviderId,
                            context.Route.Model,
                            cancellationToken);
                    }

                    return new AiDispatchResult
                    {
                        Success = true,
                        StatusCode = 200,
                        HandledByProviderId = connection.ProviderId,
                        FailoverOccurred = failoverOccurred,
                    };
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    logger.LogWarning(
                        ex,
                        "AI provider {ProviderId} attempt {Attempt} failed",
                        connection.ProviderId,
                        attempt + 1);
                }
            }

            if (context.Route.FailoverStrategy == AiFailoverStrategy.None)
            {
                break;
            }

            if (i + 1 < connections.Count)
            {
                failoverOccurred = true;
            }
        }

        if (failoverOccurred && connections.Count > 1)
        {
            await failoverService.RecordFailoverAsync(
                context.OrganizationId,
                context.Route.Connection.ProviderId,
                null,
                context.Route.Model,
                lastError?.Message ?? "All providers failed",
                succeeded: false,
                context.GatewayRequestId,
                cancellationToken);
        }

        logger.LogError(
            lastError,
            "AI inference dispatch failed for organization {OrganizationId}",
            context.OrganizationId);

        return new AiDispatchResult
        {
            Success = false,
            StatusCode = 502,
            ErrorMessage = lastError?.Message ?? "AI provider request failed.",
            FailoverOccurred = failoverOccurred,
        };
    }

    private async Task DispatchChatAsync(
        AiProviderConnection connection,
        AiDispatchContext context,
        CancellationToken cancellationToken)
    {
        var provider = providerFactory.GetProvider(connection.ProviderKind);
        var request = MapChatRequest(connection.ProviderKind, context.BodyJson, context.Route.Model);
        var response = await provider.ChatAsync(connection, request, cancellationToken);
        var json = responseMapper.ToOpenAiChatCompletionJson(response);
        var bytes = Encoding.UTF8.GetBytes(json);
        await context.ResponseBody.WriteAsync(bytes, cancellationToken);
        await context.ResponseBody.FlushAsync(cancellationToken);
    }

    private async Task DispatchStreamAsync(
        AiProviderConnection connection,
        AiDispatchContext context,
        CancellationToken cancellationToken)
    {
        var provider = providerFactory.GetProvider(connection.ProviderKind);
        var request = MapChatRequest(connection.ProviderKind, context.BodyJson, context.Route.Model);
        request = new AiChatRequest
        {
            Model = request.Model,
            Messages = request.Messages,
            Temperature = request.Temperature,
            MaxTokens = request.MaxTokens,
            TopP = request.TopP,
            Stream = true,
            SystemPrompt = request.SystemPrompt,
            Stop = request.Stop,
        };
        await provider.StreamChatAsync(connection, request, context.ResponseBody, cancellationToken);
    }

    private async Task DispatchEmbeddingsAsync(
        AiProviderConnection connection,
        AiDispatchContext context,
        CancellationToken cancellationToken)
    {
        var provider = providerFactory.GetProvider(connection.ProviderKind);
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(context.BodyJson) ? "{}" : context.BodyJson);
        var root = doc.RootElement;
        var model = root.TryGetProperty("model", out var modelEl)
            ? modelEl.GetString() ?? context.Route.Model
            : context.Route.Model;
        var inputs = new List<string>();
        if (root.TryGetProperty("input", out var inputEl))
        {
            if (inputEl.ValueKind == JsonValueKind.String)
            {
                inputs.Add(inputEl.GetString() ?? string.Empty);
            }
            else if (inputEl.ValueKind == JsonValueKind.Array)
            {
                inputs.AddRange(inputEl.EnumerateArray().Select(i => i.GetString() ?? string.Empty));
            }
        }

        var response = await provider.EmbeddingsAsync(
            connection,
            new AiEmbeddingRequest { Model = model, Input = inputs },
            cancellationToken);
        var json = responseMapper.ToOpenAiEmbeddingsJson(response);
        var bytes = Encoding.UTF8.GetBytes(json);
        await context.ResponseBody.WriteAsync(bytes, cancellationToken);
        await context.ResponseBody.FlushAsync(cancellationToken);
    }

    private AiChatRequest MapChatRequest(AiProviderKind kind, string bodyJson, string fallbackModel) =>
        kind switch
        {
            AiProviderKind.Anthropic => requestMapper.MapFromAnthropicMessagesJson(bodyJson, fallbackModel),
            AiProviderKind.GoogleGemini => requestMapper.MapFromGeminiJson(bodyJson, fallbackModel),
            AiProviderKind.Ollama => LooksLikeOllama(bodyJson)
                ? requestMapper.MapFromOllamaChatJson(bodyJson, fallbackModel)
                : requestMapper.MapFromOpenAiChatJson(bodyJson, fallbackModel),
            _ => requestMapper.MapFromOpenAiChatJson(bodyJson, fallbackModel),
        };

    private static bool LooksLikeStream(string bodyJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(bodyJson) ? "{}" : bodyJson);
            return doc.RootElement.TryGetProperty("stream", out var stream) && stream.ValueKind == JsonValueKind.True;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool LooksLikeOllama(string bodyJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(bodyJson) ? "{}" : bodyJson);
            return doc.RootElement.TryGetProperty("prompt", out _);
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
