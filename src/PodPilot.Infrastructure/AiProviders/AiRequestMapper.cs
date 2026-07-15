using System.Text.Json;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.AiProviders;

namespace PodPilot.Infrastructure.AiProviders;

/// <summary>
/// Maps provider-specific request JSON into internal chat requests.
/// </summary>
public sealed class AiRequestMapper : IAiRequestMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <inheritdoc />
    public AiChatRequest MapFromOpenAiChatJson(string json, string? fallbackModel = null)
    {
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        var root = doc.RootElement;
        var model = root.TryGetProperty("model", out var modelEl) ? modelEl.GetString() : null;
        var messages = new List<AiChatMessage>();
        string? systemPrompt = null;

        if (root.TryGetProperty("messages", out var messagesEl) && messagesEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var message in messagesEl.EnumerateArray())
            {
                var role = message.TryGetProperty("role", out var roleEl) ? roleEl.GetString() ?? "user" : "user";
                var content = ExtractContent(message);
                if (string.Equals(role, "system", StringComparison.OrdinalIgnoreCase))
                {
                    systemPrompt = string.IsNullOrWhiteSpace(systemPrompt) ? content : systemPrompt + "\n" + content;
                    continue;
                }

                messages.Add(new AiChatMessage
                {
                    Role = role,
                    Content = content,
                    Name = message.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null,
                });
            }
        }

        return new AiChatRequest
        {
            Model = string.IsNullOrWhiteSpace(model) ? fallbackModel ?? string.Empty : model,
            Messages = messages,
            Temperature = TryGetDouble(root, "temperature"),
            MaxTokens = TryGetInt(root, "max_tokens"),
            TopP = TryGetDouble(root, "top_p"),
            Stream = root.TryGetProperty("stream", out var streamEl) && streamEl.ValueKind == JsonValueKind.True,
            SystemPrompt = systemPrompt,
            Stop = TryGetStringArray(root, "stop"),
        };
    }

    /// <inheritdoc />
    public AiChatRequest MapFromAnthropicMessagesJson(string json, string? fallbackModel = null)
    {
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        var root = doc.RootElement;
        var model = root.TryGetProperty("model", out var modelEl) ? modelEl.GetString() : null;
        var systemPrompt = root.TryGetProperty("system", out var systemEl) ? systemEl.GetString() : null;
        var messages = new List<AiChatMessage>();

        if (root.TryGetProperty("messages", out var messagesEl) && messagesEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var message in messagesEl.EnumerateArray())
            {
                messages.Add(new AiChatMessage
                {
                    Role = message.TryGetProperty("role", out var roleEl) ? roleEl.GetString() ?? "user" : "user",
                    Content = ExtractContent(message),
                });
            }
        }

        return new AiChatRequest
        {
            Model = string.IsNullOrWhiteSpace(model) ? fallbackModel ?? string.Empty : model,
            Messages = messages,
            Temperature = TryGetDouble(root, "temperature"),
            MaxTokens = TryGetInt(root, "max_tokens"),
            TopP = TryGetDouble(root, "top_p"),
            Stream = root.TryGetProperty("stream", out var streamEl) && streamEl.ValueKind == JsonValueKind.True,
            SystemPrompt = systemPrompt,
        };
    }

    /// <inheritdoc />
    public AiChatRequest MapFromGeminiJson(string json, string? fallbackModel = null)
    {
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        var root = doc.RootElement;
        var messages = new List<AiChatMessage>();
        string? systemPrompt = null;

        if (root.TryGetProperty("systemInstruction", out var system) &&
            system.TryGetProperty("parts", out var systemParts))
        {
            systemPrompt = string.Join(
                "\n",
                systemParts.EnumerateArray()
                    .Select(p => p.TryGetProperty("text", out var t) ? t.GetString() : null)
                    .Where(t => !string.IsNullOrWhiteSpace(t)));
        }

        if (root.TryGetProperty("contents", out var contents) && contents.ValueKind == JsonValueKind.Array)
        {
            foreach (var content in contents.EnumerateArray())
            {
                var role = content.TryGetProperty("role", out var roleEl) ? roleEl.GetString() ?? "user" : "user";
                if (string.Equals(role, "model", StringComparison.OrdinalIgnoreCase))
                {
                    role = "assistant";
                }

                var text = string.Empty;
                if (content.TryGetProperty("parts", out var parts))
                {
                    text = string.Join(
                        string.Empty,
                        parts.EnumerateArray()
                            .Select(p => p.TryGetProperty("text", out var t) ? t.GetString() : null)
                            .Where(t => t is not null));
                }

                messages.Add(new AiChatMessage { Role = role, Content = text });
            }
        }

        double? temperature = null;
        int? maxTokens = null;
        double? topP = null;
        if (root.TryGetProperty("generationConfig", out var config))
        {
            temperature = TryGetDouble(config, "temperature");
            maxTokens = TryGetInt(config, "maxOutputTokens");
            topP = TryGetDouble(config, "topP");
        }

        return new AiChatRequest
        {
            Model = fallbackModel ?? string.Empty,
            Messages = messages,
            Temperature = temperature,
            MaxTokens = maxTokens,
            TopP = topP,
            SystemPrompt = systemPrompt,
        };
    }

    /// <inheritdoc />
    public AiChatRequest MapFromOllamaChatJson(string json, string? fallbackModel = null)
    {
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        var root = doc.RootElement;
        var model = root.TryGetProperty("model", out var modelEl) ? modelEl.GetString() : null;
        var messages = new List<AiChatMessage>();

        if (root.TryGetProperty("messages", out var messagesEl) && messagesEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var message in messagesEl.EnumerateArray())
            {
                messages.Add(new AiChatMessage
                {
                    Role = message.TryGetProperty("role", out var roleEl) ? roleEl.GetString() ?? "user" : "user",
                    Content = ExtractContent(message),
                });
            }
        }
        else if (root.TryGetProperty("prompt", out var promptEl))
        {
            messages.Add(new AiChatMessage { Role = "user", Content = promptEl.GetString() ?? string.Empty });
        }

        return new AiChatRequest
        {
            Model = string.IsNullOrWhiteSpace(model) ? fallbackModel ?? string.Empty : model,
            Messages = messages,
            Stream = !root.TryGetProperty("stream", out var streamEl) || streamEl.ValueKind != JsonValueKind.False,
            Temperature = root.TryGetProperty("options", out var options) ? TryGetDouble(options, "temperature") : null,
        };
    }

    private static string ExtractContent(JsonElement message)
    {
        if (!message.TryGetProperty("content", out var content))
        {
            return string.Empty;
        }

        if (content.ValueKind == JsonValueKind.String)
        {
            return content.GetString() ?? string.Empty;
        }

        if (content.ValueKind == JsonValueKind.Array)
        {
            return string.Join(
                string.Empty,
                content.EnumerateArray()
                    .Select(p => p.TryGetProperty("text", out var t) ? t.GetString() : null)
                    .Where(t => t is not null));
        }

        return content.ToString();
    }

    private static double? TryGetDouble(JsonElement root, string name) =>
        root.TryGetProperty(name, out var el) && el.TryGetDouble(out var value) ? value : null;

    private static int? TryGetInt(JsonElement root, string name) =>
        root.TryGetProperty(name, out var el) && el.TryGetInt32(out var value) ? value : null;

    private static IReadOnlyList<string>? TryGetStringArray(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var el))
        {
            return null;
        }

        if (el.ValueKind == JsonValueKind.String)
        {
            var value = el.GetString();
            return string.IsNullOrWhiteSpace(value) ? null : [value];
        }

        if (el.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        return el.EnumerateArray()
            .Select(i => i.GetString())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Cast<string>()
            .ToList();
    }
}
