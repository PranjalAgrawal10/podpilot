using System.Text.Json;
using PodPilot.Application.Models;
using PodPilot.Application.Models.Ollama;

namespace PodPilot.Infrastructure.Ollama;

/// <summary>
/// Parses Ollama show API responses into application models.
/// </summary>
internal static class OllamaMetadataParser
{
    /// <summary>
    /// Parses an Ollama show response.
    /// </summary>
    public static OllamaModelDetails ParseShowResponse(JsonElement root, string fallbackName)
    {
        var details = new OllamaModelDetails
        {
            Name = root.TryGetProperty("modelfile", out _) ? fallbackName : fallbackName,
            Size = root.TryGetProperty("size", out var sizeElement) && sizeElement.TryGetInt64(out var size)
                ? size
                : 0,
        };

        if (root.TryGetProperty("model_info", out var modelInfo) && modelInfo.ValueKind == JsonValueKind.Object)
        {
            details.Family = ReadString(modelInfo, "general.architecture")
                ?? ReadString(modelInfo, "general.basename")
                ?? ReadString(modelInfo, "family");

            details.Parameters = ModelReferenceParser.ExtractParameters(
                ReadString(modelInfo, "general.parameter_count")
                    ?? ReadString(modelInfo, "general.size_label"),
                fallbackName);

            details.Quantization = ModelReferenceParser.ExtractQuantization(
                ReadString(modelInfo, "general.quantization_version")
                    ?? ReadString(modelInfo, "general.file_type"),
                fallbackName);

            if (modelInfo.TryGetProperty("llama.context_length", out var contextElement)
                && contextElement.TryGetInt32(out var contextLength))
            {
                details.ContextLength = contextLength;
            }
        }

        if (root.TryGetProperty("details", out var detailsElement) && detailsElement.ValueKind == JsonValueKind.Object)
        {
            details.Family ??= ReadString(detailsElement, "family");
            details.Parameters ??= ReadString(detailsElement, "parameter_size");
            details.Quantization ??= ReadString(detailsElement, "quantization_level");
        }

        if (root.TryGetProperty("license", out var licenseElement))
        {
            details.License = licenseElement.ValueKind == JsonValueKind.String
                ? licenseElement.GetString()
                : licenseElement.ToString();
        }

        return details;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null,
        };
    }
}
