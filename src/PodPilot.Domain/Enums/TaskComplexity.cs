namespace PodPilot.Domain.Enums;

/// <summary>
/// Estimated complexity of an AI task for token and model selection.
/// </summary>
public enum TaskComplexity
{
    /// <summary>Short, straightforward prompts.</summary>
    Low = 0,

    /// <summary>Moderate multi-turn or structured work.</summary>
    Medium = 1,

    /// <summary>Long-context or multi-step difficult work.</summary>
    High = 2,
}
