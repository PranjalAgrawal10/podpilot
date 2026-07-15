using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Plugins;

namespace PodPilot.Infrastructure.Mcp;

/// <summary>
/// MCP client over HTTP JSON-RPC (initialize, tools/list, resources/list, prompts/list, tools/call).
/// </summary>
public sealed class HttpJsonRpcMcpConnection : IMcpConnection
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient httpClient;
    private readonly string endpoint;
    private readonly ILogger logger;
    private int nextId = 1;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpJsonRpcMcpConnection"/> class.
    /// </summary>
    public HttpJsonRpcMcpConnection(
        HttpClient httpClient,
        string endpoint,
        string authScheme,
        string? credential,
        ILogger logger)
    {
        this.httpClient = httpClient;
        this.endpoint = endpoint.TrimEnd('/');
        this.logger = logger;

        if (!string.IsNullOrWhiteSpace(credential) &&
            !string.Equals(authScheme, "None", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(authScheme, "Bearer", StringComparison.OrdinalIgnoreCase))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credential);
            }
            else if (string.Equals(authScheme, "ApiKey", StringComparison.OrdinalIgnoreCase))
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", credential);
            }
            else if (string.Equals(authScheme, "Basic", StringComparison.OrdinalIgnoreCase))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credential);
            }
        }

        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <inheritdoc />
    public bool IsConnected { get; private set; }

    /// <inheritdoc />
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendAsync(
            "initialize",
            new
            {
                protocolVersion = "2024-11-05",
                capabilities = new { },
                clientInfo = new { name = "PodPilot", version = "1.0.0" },
            },
            cancellationToken);

        IsConnected = result is not null;
        if (IsConnected)
        {
            _ = await SendAsync("notifications/initialized", new { }, cancellationToken, notification: true);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<McpToolInfo>> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        var root = await SendAsync("tools/list", new { }, cancellationToken);
        if (root is null || !root.Value.TryGetProperty("tools", out var tools) || tools.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return tools.EnumerateArray().Select(t => new McpToolInfo
        {
            Name = t.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty,
            Description = t.TryGetProperty("description", out var d) ? d.GetString() : null,
            InputSchemaJson = t.TryGetProperty("inputSchema", out var s) ? s.GetRawText() : null,
        }).Where(t => !string.IsNullOrWhiteSpace(t.Name)).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<McpResourceInfo>> ListResourcesAsync(CancellationToken cancellationToken = default)
    {
        var root = await SendAsync("resources/list", new { }, cancellationToken);
        if (root is null || !root.Value.TryGetProperty("resources", out var resources) || resources.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return resources.EnumerateArray().Select(r => new McpResourceInfo
        {
            Uri = r.TryGetProperty("uri", out var u) ? u.GetString() ?? string.Empty : string.Empty,
            Name = r.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty,
            MimeType = r.TryGetProperty("mimeType", out var m) ? m.GetString() : null,
            Description = r.TryGetProperty("description", out var d) ? d.GetString() : null,
        }).Where(r => !string.IsNullOrWhiteSpace(r.Uri)).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<McpPromptInfo>> ListPromptsAsync(CancellationToken cancellationToken = default)
    {
        var root = await SendAsync("prompts/list", new { }, cancellationToken);
        if (root is null || !root.Value.TryGetProperty("prompts", out var prompts) || prompts.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return prompts.EnumerateArray().Select(p => new McpPromptInfo
        {
            Name = p.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty,
            Description = p.TryGetProperty("description", out var d) ? d.GetString() : null,
            ArgumentsJson = p.TryGetProperty("arguments", out var a) ? a.GetRawText() : null,
        }).Where(p => !string.IsNullOrWhiteSpace(p.Name)).ToList();
    }

    /// <inheritdoc />
    public async Task<McpToolCallResult> CallToolAsync(
        string toolName,
        string argumentsJson,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            object args;
            try
            {
                args = JsonSerializer.Deserialize<JsonElement>(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            }
            catch (JsonException)
            {
                args = new { };
            }

            var root = await SendAsync(
                "tools/call",
                new { name = toolName, arguments = args },
                cancellationToken);

            sw.Stop();
            if (root is null)
            {
                return new McpToolCallResult
                {
                    Succeeded = false,
                    ErrorMessage = "Empty MCP response",
                    DurationMs = (int)sw.ElapsedMilliseconds,
                };
            }

            var isError = root.Value.TryGetProperty("isError", out var err) && err.ValueKind == JsonValueKind.True;
            return new McpToolCallResult
            {
                Succeeded = !isError,
                ContentJson = root.Value.GetRawText(),
                ErrorMessage = isError ? "Tool reported an error" : null,
                DurationMs = (int)sw.ElapsedMilliseconds,
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogWarning(ex, "MCP tool call failed for {Tool}", toolName);
            return new McpToolCallResult
            {
                Succeeded = false,
                ErrorMessage = "Tool execution failed",
                DurationMs = (int)sw.ElapsedMilliseconds,
            };
        }
    }

    /// <inheritdoc />
    public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var tools = await ListToolsAsync(cancellationToken);
            IsConnected = true;
            return true;
        }
        catch
        {
            IsConnected = false;
            return false;
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (!disposed)
        {
            disposed = true;
            IsConnected = false;
        }

        return ValueTask.CompletedTask;
    }

    private async Task<JsonElement?> SendAsync(
        string method,
        object parameters,
        CancellationToken cancellationToken,
        bool notification = false)
    {
        object payload = notification
            ? new { jsonrpc = "2.0", method, @params = parameters }
            : new { jsonrpc = "2.0", id = Interlocked.Increment(ref nextId), method, @params = parameters };

        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await httpClient.PostAsync(endpoint, content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("MCP {Method} failed with {Status}: {Body}", method, (int)response.StatusCode, Truncate(body));
            response.EnsureSuccessStatusCode();
        }

        if (notification || string.IsNullOrWhiteSpace(body))
        {
            return default(JsonElement);
        }

        using var doc = JsonDocument.Parse(body);
        if (doc.RootElement.TryGetProperty("error", out var error))
        {
            logger.LogWarning("MCP {Method} returned error {Error}", method, Truncate(error.GetRawText()));
            return null;
        }

        if (doc.RootElement.TryGetProperty("result", out var result))
        {
            return result.Clone();
        }

        return doc.RootElement.Clone();
    }

    private static string Truncate(string value) =>
        value.Length <= 300 ? value : value[..300];
}
