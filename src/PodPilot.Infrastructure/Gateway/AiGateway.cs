using System.Text;
using System.Text.Json;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Gateway;
using PodPilot.Application.Models.Plugins;
using PodPilot.Application.Models.Scheduler;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Orchestrates end-to-end AI gateway request handling via MCP tools, AI providers, or the scheduler.
/// </summary>
public sealed class AiGateway : IAiGateway
{
    private readonly IRequestScheduler scheduler;
    private readonly IAiInferenceRouter inferenceRouter;
    private readonly IAiInferenceDispatcher inferenceDispatcher;
    private readonly IMcpProxy mcpProxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiGateway"/> class.
    /// </summary>
    public AiGateway(
        IRequestScheduler scheduler,
        IAiInferenceRouter inferenceRouter,
        IAiInferenceDispatcher inferenceDispatcher,
        IMcpProxy mcpProxy)
    {
        this.scheduler = scheduler;
        this.inferenceRouter = inferenceRouter;
        this.inferenceDispatcher = inferenceDispatcher;
        this.mcpProxy = mcpProxy;
    }

    /// <inheritdoc />
    public Task<GatewayRequestResult> HandleRequestAsync(
        GatewayAuthContext auth,
        string path,
        string method,
        IReadOnlyDictionary<string, string> headers,
        Stream requestBody,
        Stream responseBody,
        GatewayErrorFormat errorFormat,
        string? correlationId,
        Func<GatewayProxyResult, Task>? onResponseHeaders = null,
        CancellationToken cancellationToken = default) =>
        HandleRequestAsync(
            auth,
            path,
            method,
            headers,
            requestBody,
            responseBody,
            errorFormat,
            correlationId,
            headers.TryGetValue("X-Request-Id", out var clientRequestId) ? clientRequestId : null,
            onResponseHeaders,
            cancellationToken);

    private async Task<GatewayRequestResult> HandleRequestAsync(
        GatewayAuthContext auth,
        string path,
        string method,
        IReadOnlyDictionary<string, string> headers,
        Stream requestBody,
        Stream responseBody,
        GatewayErrorFormat errorFormat,
        string? correlationId,
        string? clientRequestId,
        Func<GatewayProxyResult, Task>? onResponseHeaders,
        CancellationToken cancellationToken)
    {
        var bufferedBody = new MemoryStream();
        if (requestBody.CanSeek)
        {
            requestBody.Position = 0;
        }

        await requestBody.CopyToAsync(bufferedBody, cancellationToken);
        bufferedBody.Position = 0;

        var bodyJson = Encoding.UTF8.GetString(bufferedBody.ToArray());
        bufferedBody.Position = 0;

        if (IsMcpToolPath(path, method))
        {
            return await HandleMcpToolAsync(
                auth,
                bodyJson,
                responseBody,
                onResponseHeaders,
                cancellationToken);
        }

        var model = TryExtractModel(bodyJson);
        var route = await inferenceRouter.TryResolveAsync(
            auth.OrganizationId,
            model,
            path,
            bodyJson,
            cancellationToken);
        if (route is not null)
        {
            var streamRequested = false;
            try
            {
                using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(bodyJson) ? "{}" : bodyJson);
                streamRequested = doc.RootElement.TryGetProperty("stream", out var streamEl) &&
                                  streamEl.ValueKind == JsonValueKind.True;
            }
            catch (JsonException)
            {
                streamRequested = false;
            }

            if (onResponseHeaders is not null)
            {
                await onResponseHeaders(new GatewayProxyResult
                {
                    StatusCode = 200,
                    Headers = new Dictionary<string, string>
                    {
                        ["Content-Type"] = streamRequested ? "text/event-stream" : "application/json",
                    },
                });
            }

            var dispatch = await inferenceDispatcher.DispatchAsync(
                new AiDispatchContext
                {
                    OrganizationId = auth.OrganizationId,
                    Route = route,
                    Path = path,
                    Method = method,
                    BodyJson = bodyJson,
                    ResponseBody = responseBody,
                    Stream = streamRequested,
                },
                cancellationToken);

            return new GatewayRequestResult
            {
                Success = dispatch.Success,
                StatusCode = dispatch.StatusCode,
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = streamRequested ? "text/event-stream" : "application/json",
                },
                ErrorCode = dispatch.Success ? null : "ai_provider_error",
                ErrorMessage = dispatch.ErrorMessage,
            };
        }

        var result = await scheduler.ProcessAsync(
            new ScheduleRequestContext
            {
                Auth = auth,
                Path = path,
                Method = method,
                Headers = headers,
                Body = bufferedBody,
                ResponseBody = responseBody,
                ErrorFormat = errorFormat,
                CorrelationId = correlationId,
                ClientRequestId = clientRequestId,
                OnResponseHeaders = onResponseHeaders,
            },
            cancellationToken);

        return new GatewayRequestResult
        {
            Success = result.Success,
            StatusCode = result.StatusCode,
            Headers = result.Headers,
            ErrorCode = result.ErrorCode,
            ErrorMessage = result.ErrorMessage,
        };
    }

    private async Task<GatewayRequestResult> HandleMcpToolAsync(
        GatewayAuthContext auth,
        string bodyJson,
        Stream responseBody,
        Func<GatewayProxyResult, Task>? onResponseHeaders,
        CancellationToken cancellationToken)
    {
        string toolName;
        string argumentsJson = "{}";
        Guid? serverId = null;

        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(bodyJson) ? "{}" : bodyJson);
            var root = doc.RootElement;
            toolName = root.TryGetProperty("tool", out var toolEl)
                ? toolEl.GetString() ?? string.Empty
                : root.TryGetProperty("name", out var nameEl)
                    ? nameEl.GetString() ?? string.Empty
                    : string.Empty;

            if (root.TryGetProperty("arguments", out var argsEl))
            {
                argumentsJson = argsEl.ValueKind == JsonValueKind.String
                    ? argsEl.GetString() ?? "{}"
                    : argsEl.GetRawText();
            }
            else if (root.TryGetProperty("argumentsJson", out var argsJsonEl))
            {
                argumentsJson = argsJsonEl.GetString() ?? "{}";
            }

            if (root.TryGetProperty("serverId", out var serverEl) &&
                serverEl.ValueKind == JsonValueKind.String &&
                Guid.TryParse(serverEl.GetString(), out var parsed))
            {
                serverId = parsed;
            }
        }
        catch (JsonException)
        {
            return new GatewayRequestResult
            {
                Success = false,
                StatusCode = 400,
                ErrorCode = "invalid_mcp_request",
                ErrorMessage = "Invalid MCP tool request body.",
            };
        }

        if (string.IsNullOrWhiteSpace(toolName))
        {
            return new GatewayRequestResult
            {
                Success = false,
                StatusCode = 400,
                ErrorCode = "invalid_mcp_request",
                ErrorMessage = "tool is required.",
            };
        }

        var result = await mcpProxy.ExecuteToolAsync(
            new McpToolCallRequest
            {
                OrganizationId = auth.OrganizationId,
                ServerId = serverId ?? Guid.Empty,
                ToolName = toolName.Trim(),
                ArgumentsJson = argumentsJson,
            },
            cancellationToken);

        var payload = JsonSerializer.Serialize(new
        {
            succeeded = result.Succeeded,
            content = result.ContentJson,
            error = result.ErrorMessage,
            durationMs = result.DurationMs,
        });

        if (onResponseHeaders is not null)
        {
            await onResponseHeaders(new GatewayProxyResult
            {
                StatusCode = result.Succeeded ? 200 : 502,
                Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" },
            });
        }

        var bytes = Encoding.UTF8.GetBytes(payload);
        await responseBody.WriteAsync(bytes, cancellationToken);

        return new GatewayRequestResult
        {
            Success = result.Succeeded,
            StatusCode = result.Succeeded ? 200 : 502,
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" },
            ErrorCode = result.Succeeded ? null : "mcp_tool_error",
            ErrorMessage = result.ErrorMessage,
        };
    }

    private static bool IsMcpToolPath(string path, string method)
    {
        if (!string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var normalized = path.Trim('/').Replace('\\', '/');
        return normalized.EndsWith("mcp/tools/call", StringComparison.OrdinalIgnoreCase)
               || normalized.EndsWith("v1/mcp/tools/call", StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryExtractModel(string bodyJson)
    {
        if (string.IsNullOrWhiteSpace(bodyJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(bodyJson);
            return doc.RootElement.TryGetProperty("model", out var modelEl)
                ? modelEl.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
