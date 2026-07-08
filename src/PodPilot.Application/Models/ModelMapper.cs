using PodPilot.Application.Models.Ollama;
using PodPilot.Contracts.Models;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models;

/// <summary>
/// Maps model entities to contract responses.
/// </summary>
public static class ModelMapper
{
    /// <summary>
    /// Maps an AI model to a response DTO.
    /// </summary>
    public static ModelResponse ToResponse(AiModel model) =>
        new()
        {
            Id = model.Id,
            OrganizationId = model.OrganizationId,
            PodId = model.PodId,
            PodName = model.Pod?.Name ?? string.Empty,
            Name = model.Name,
            Tag = model.Tag,
            FullName = model.FullName,
            Family = model.Family,
            Size = model.Size,
            Quantization = model.Quantization,
            ContextLength = model.ContextLength,
            Parameters = model.Parameters,
            License = model.License,
            IsDefault = model.IsDefault,
            Status = model.Status.ToString(),
            LastUsed = model.LastUsed,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
        };

    /// <summary>
    /// Maps a model download to a response DTO.
    /// </summary>
    public static ModelDownloadResponse ToDownloadResponse(ModelDownload download) =>
        new()
        {
            Id = download.Id,
            ModelId = download.ModelId,
            ModelName = download.Model?.FullName ?? string.Empty,
            PodId = download.Model?.PodId ?? Guid.Empty,
            Progress = download.Progress,
            Status = download.Status.ToString(),
            DownloadSpeed = download.DownloadSpeed,
            StartedAt = download.StartedAt,
            CompletedAt = download.CompletedAt,
            ErrorMessage = download.ErrorMessage,
        };

    /// <summary>
    /// Maps health history to a response DTO.
    /// </summary>
    public static ModelHealthResponse ToHealthResponse(ModelHealthHistory history) =>
        new()
        {
            Id = history.Id,
            ModelId = history.ModelId,
            ModelName = history.Model?.FullName ?? string.Empty,
            PodId = history.Model?.PodId ?? Guid.Empty,
            Status = history.Status.ToString(),
            ResponseTime = history.ResponseTime,
            LastChecked = history.LastChecked,
            ErrorMessage = history.ErrorMessage,
        };

    /// <summary>
    /// Applies Ollama metadata to an AI model entity.
    /// </summary>
    public static void ApplyOllamaDetails(AiModel model, OllamaModelDetails details)
    {
        var (name, tag) = ModelReferenceParser.Parse(details.Name);
        model.Name = name;
        model.Tag = tag;
        model.Family = details.Family;
        model.Parameters = details.Parameters ?? ModelReferenceParser.ExtractParameters(null, details.Name);
        model.Quantization = details.Quantization ?? ModelReferenceParser.ExtractQuantization(null, details.Name);
        model.ContextLength = details.ContextLength;
        model.Size = details.Size;
        model.License = details.License;
    }

    /// <summary>
    /// Applies tag list metadata to an AI model entity.
    /// </summary>
    public static void ApplyOllamaTag(AiModel model, OllamaModelTag tag)
    {
        var (name, parsedTag) = ModelReferenceParser.Parse(tag.Name);
        model.Name = name;
        model.Tag = parsedTag;
        model.Size = tag.Size;
        model.Parameters ??= ModelReferenceParser.ExtractParameters(null, tag.Name);
        model.Quantization ??= ModelReferenceParser.ExtractQuantization(null, tag.Name);
    }
}
