using System.Text.Json;
using System.Text.RegularExpressions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Classifies AI requests into task type, complexity, and token estimates.
/// </summary>
public sealed class TaskClassifier : ITaskClassifier
{
    private static readonly Regex CodeRegex = new(
        @"\b(function|class|def |import |```|typescript|python|bug|refactor|compile|stacktrace)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ReasoningRegex = new(
        @"\b(reason|think step|prove|analyze|why|logic|deduce|chain of thought)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TranslateRegex = new(
        @"\b(translate|translation|from english|to spanish|to french|to german|to hindi)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SummarizeRegex = new(
        @"\b(summarize|summary|tldr|tl;dr|key points|condense)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PlanningRegex = new(
        @"\b(plan|roadmap|outline|break down|milestones|strategy)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex VisionRegex = new(
        @"\b(image|picture|photo|screenshot|vision|describe this)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public RoutingRequestAnalysis Analyze(string? path, string? bodyJson, string? prompt)
    {
        var text = prompt ?? string.Empty;
        var requiresEmbeddings = path?.Contains("embedding", StringComparison.OrdinalIgnoreCase) == true;
        var requiresVision = false;
        var requiresTools = false;
        string? requestedModel = null;

        if (!string.IsNullOrWhiteSpace(bodyJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(bodyJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("model", out var modelEl) && modelEl.ValueKind == JsonValueKind.String)
                {
                    requestedModel = modelEl.GetString();
                }

                if (root.TryGetProperty("messages", out var messages) && messages.ValueKind == JsonValueKind.Array)
                {
                    var parts = new List<string>();
                    foreach (var message in messages.EnumerateArray())
                    {
                        if (message.TryGetProperty("content", out var content))
                        {
                            if (content.ValueKind == JsonValueKind.String)
                            {
                                parts.Add(content.GetString() ?? string.Empty);
                            }
                            else if (content.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var part in content.EnumerateArray())
                                {
                                    if (part.TryGetProperty("type", out var type) &&
                                        type.GetString()?.Contains("image", StringComparison.OrdinalIgnoreCase) == true)
                                    {
                                        requiresVision = true;
                                    }

                                    if (part.TryGetProperty("text", out var textPart) &&
                                        textPart.ValueKind == JsonValueKind.String)
                                    {
                                        parts.Add(textPart.GetString() ?? string.Empty);
                                    }
                                }
                            }
                        }
                    }

                    if (parts.Count > 0)
                    {
                        text = string.Join('\n', parts);
                    }
                }

                if (root.TryGetProperty("input", out _) || root.TryGetProperty("inputs", out _))
                {
                    requiresEmbeddings = requiresEmbeddings ||
                                         path?.Contains("embedding", StringComparison.OrdinalIgnoreCase) == true;
                }

                if (root.TryGetProperty("tools", out _) || root.TryGetProperty("functions", out _))
                {
                    requiresTools = true;
                }
            }
            catch (JsonException)
            {
                // Keep text from prompt fallback.
            }
        }

        if (string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(prompt))
        {
            text = prompt;
        }

        var taskType = ClassifyTask(text, requiresEmbeddings, requiresVision);
        var complexity = EstimateComplexity(text, taskType);
        var inputTokens = EstimateTokens(text);
        var outputTokens = EstimateOutputTokens(inputTokens, complexity, taskType);

        return new RoutingRequestAnalysis
        {
            TaskType = taskType,
            Complexity = complexity,
            EstimatedInputTokens = inputTokens,
            EstimatedOutputTokens = outputTokens,
            RequestedModel = string.IsNullOrWhiteSpace(requestedModel) ? null : requestedModel,
            RequiresVision = requiresVision || taskType == AiTaskType.Vision,
            RequiresEmbeddings = requiresEmbeddings || taskType == AiTaskType.Embeddings,
            RequiresTools = requiresTools,
            RequiresReasoning = taskType is AiTaskType.Reasoning or AiTaskType.Planning or AiTaskType.Coding
                                && complexity != TaskComplexity.Low,
            PromptPreview = text.Length <= 240 ? text : text[..240],
        };
    }

    private static AiTaskType ClassifyTask(string text, bool requiresEmbeddings, bool requiresVision)
    {
        if (requiresEmbeddings)
        {
            return AiTaskType.Embeddings;
        }

        if (requiresVision || VisionRegex.IsMatch(text))
        {
            return AiTaskType.Vision;
        }

        if (CodeRegex.IsMatch(text))
        {
            return AiTaskType.Coding;
        }

        if (ReasoningRegex.IsMatch(text))
        {
            return AiTaskType.Reasoning;
        }

        if (TranslateRegex.IsMatch(text))
        {
            return AiTaskType.Translation;
        }

        if (SummarizeRegex.IsMatch(text))
        {
            return AiTaskType.Summarization;
        }

        if (PlanningRegex.IsMatch(text))
        {
            return AiTaskType.Planning;
        }

        if (text.Length > 0)
        {
            return AiTaskType.Chat;
        }

        return AiTaskType.General;
    }

    private static TaskComplexity EstimateComplexity(string text, AiTaskType taskType)
    {
        var length = text.Length;
        if (taskType is AiTaskType.Reasoning or AiTaskType.Planning || length > 4000)
        {
            return TaskComplexity.High;
        }

        if (taskType is AiTaskType.Coding or AiTaskType.Vision || length > 800)
        {
            return TaskComplexity.Medium;
        }

        return TaskComplexity.Low;
    }

    private static int EstimateTokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 64;
        }

        // Rough heuristic: ~4 chars per token for mixed English/code.
        return Math.Clamp((int)Math.Ceiling(text.Length / 4.0), 16, 128000);
    }

    private static int EstimateOutputTokens(int inputTokens, TaskComplexity complexity, AiTaskType taskType)
    {
        var factor = complexity switch
        {
            TaskComplexity.Low => 0.5,
            TaskComplexity.High => 1.5,
            _ => 1.0,
        };

        if (taskType is AiTaskType.Summarization or AiTaskType.Translation)
        {
            factor *= 0.6;
        }
        else if (taskType is AiTaskType.Coding or AiTaskType.Reasoning)
        {
            factor *= 1.3;
        }
        else if (taskType == AiTaskType.Embeddings)
        {
            return 0;
        }

        return Math.Clamp((int)Math.Ceiling(inputTokens * factor), 32, 8192);
    }
}
