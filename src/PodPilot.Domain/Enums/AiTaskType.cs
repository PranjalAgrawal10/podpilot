namespace PodPilot.Domain.Enums;

/// <summary>
/// Classification of an AI inference task for routing.
/// </summary>
public enum AiTaskType
{
    /// <summary>General-purpose chat or unspecified work.</summary>
    General = 0,

    /// <summary>Code generation, completion, or debugging.</summary>
    Coding = 1,

    /// <summary>Multi-step reasoning or analysis.</summary>
    Reasoning = 2,

    /// <summary>Conversational chat.</summary>
    Chat = 3,

    /// <summary>Language translation.</summary>
    Translation = 4,

    /// <summary>Text summarization.</summary>
    Summarization = 5,

    /// <summary>Image or multimodal vision understanding.</summary>
    Vision = 6,

    /// <summary>Embedding vector generation.</summary>
    Embeddings = 7,

    /// <summary>Planning, outlining, or structured task breakdown.</summary>
    Planning = 8,
}
