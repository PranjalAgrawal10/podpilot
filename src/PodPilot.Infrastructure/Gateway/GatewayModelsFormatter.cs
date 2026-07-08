using System.Text.Json;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Formats Ollama model listings for OpenAI and Anthropic clients.
/// </summary>
public static class GatewayModelsFormatter
{
    /// <summary>
    /// Converts Ollama tags JSON to a client-compatible models response.
    /// </summary>
    public static string Format(string ollamaTagsJson, GatewayErrorFormat format)
    {
        using var document = JsonDocument.Parse(ollamaTagsJson);
        var models = new List<string>();

        if (document.RootElement.TryGetProperty("models", out var modelsElement)
            && modelsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var model in modelsElement.EnumerateArray())
            {
                if (model.TryGetProperty("name", out var name))
                {
                    models.Add(name.GetString() ?? string.Empty);
                }
            }
        }

        models = models.Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().ToList();
        var created = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (format == GatewayErrorFormat.Anthropic)
        {
            var payload = new
            {
                data = models.Select(model => new
                {
                    id = model,
                    type = "model",
                    display_name = model,
                    created_at = created,
                }),
                has_more = false,
            };

            return JsonSerializer.Serialize(payload);
        }

        var openAiPayload = new
        {
            @object = "list",
            data = models.Select(model => new
            {
                id = model,
                @object = "model",
                created,
                owned_by = "ollama",
            }),
        };

        return JsonSerializer.Serialize(openAiPayload);
    }
}
