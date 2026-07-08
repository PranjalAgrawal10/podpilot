namespace PodPilot.Application.Models.Ollama;

/// <summary>
/// Ollama version response.
/// </summary>
public sealed class OllamaVersionResult
{
    /// <summary>
    /// Gets or sets the Ollama version string.
    /// </summary>
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Ollama model tag entry.
/// </summary>
public sealed class OllamaModelTag
{
    /// <summary>
    /// Gets or sets the model name including tag.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model digest.
    /// </summary>
    public string? Digest { get; set; }

    /// <summary>
    /// Gets or sets the model size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets when the model was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>
/// Detailed Ollama model metadata.
/// </summary>
public sealed class OllamaModelDetails
{
    /// <summary>
    /// Gets or sets the model reference.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model family.
    /// </summary>
    public string? Family { get; set; }

    /// <summary>
    /// Gets or sets the parameter label.
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// Gets or sets the quantization format.
    /// </summary>
    public string? Quantization { get; set; }

    /// <summary>
    /// Gets or sets the context length.
    /// </summary>
    public int? ContextLength { get; set; }

    /// <summary>
    /// Gets or sets the model size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the license.
    /// </summary>
    public string? License { get; set; }
}

/// <summary>
/// Ollama pull progress update.
/// </summary>
public sealed class OllamaPullProgress
{
    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets completed bytes.
    /// </summary>
    public long? Completed { get; set; }

    /// <summary>
    /// Gets or sets total bytes.
    /// </summary>
    public long? Total { get; set; }
}

/// <summary>
/// Ollama generate response.
/// </summary>
public sealed class OllamaGenerateResult
{
    /// <summary>
    /// Gets or sets the generated response text.
    /// </summary>
    public string Response { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether generation completed.
    /// </summary>
    public bool Done { get; set; }
}

/// <summary>
/// Ollama embeddings response.
/// </summary>
public sealed class OllamaEmbeddingsResult
{
    /// <summary>
    /// Gets or sets the embedding vector.
    /// </summary>
    public IReadOnlyList<float> Embedding { get; set; } = [];
}
