namespace PodPilot.Application.Models;

/// <summary>
/// Parses Ollama model references and metadata.
/// </summary>
public static class ModelReferenceParser
{
    /// <summary>
    /// Parses a model reference into name and tag components.
    /// </summary>
    public static (string Name, string Tag) Parse(string modelReference)
    {
        var trimmed = modelReference.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ArgumentException("Model reference is required.", nameof(modelReference));
        }

        var colonIndex = trimmed.LastIndexOf(':');
        if (colonIndex > 0 && colonIndex < trimmed.Length - 1)
        {
            return (trimmed[..colonIndex], trimmed[(colonIndex + 1)..]);
        }

        return (trimmed, "latest");
    }

    /// <summary>
    /// Builds a full model reference from name and tag.
    /// </summary>
    public static string ToReference(string name, string tag) =>
        string.IsNullOrWhiteSpace(tag) || tag == "latest" ? name : $"{name}:{tag}";

    /// <summary>
    /// Extracts parameter label from model metadata or name.
    /// </summary>
    public static string? ExtractParameters(string? metadataValue, string modelName)
    {
        if (!string.IsNullOrWhiteSpace(metadataValue))
        {
            return metadataValue;
        }

        var match = System.Text.RegularExpressions.Regex.Match(
            modelName,
            @"(\d+(?:\.\d+)?[bBmM])",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);

        return match.Success ? match.Groups[1].Value.ToUpperInvariant() : null;
    }

    /// <summary>
    /// Extracts quantization from model file name.
    /// </summary>
    public static string? ExtractQuantization(string? details, string modelName)
    {
        if (!string.IsNullOrWhiteSpace(details))
        {
            return details;
        }

        var source = modelName.ToUpperInvariant();
        string[] candidates = ["Q2_K", "Q3_K", "Q4_K_M", "Q4_0", "Q5_K_M", "Q5_0", "Q6_K", "Q8_0", "F16", "F32"];
        return candidates.FirstOrDefault(source.Contains);
    }
}
