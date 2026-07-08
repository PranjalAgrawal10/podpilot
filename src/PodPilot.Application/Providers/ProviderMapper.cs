using PodPilot.Contracts.Providers;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Providers;

/// <summary>
/// Maps provider entities to contract responses.
/// </summary>
internal static class ProviderMapper
{
    /// <summary>
    /// Maps a compute provider entity to a response DTO.
    /// </summary>
    public static ProviderResponse ToResponse(ComputeProvider provider) =>
        new()
        {
            Id = provider.Id,
            OrganizationId = provider.OrganizationId,
            Name = provider.Name,
            ProviderType = provider.ProviderType.ToString(),
            DisplayName = provider.DisplayName,
            Description = provider.Description,
            DefaultRegion = provider.DefaultRegion,
            IsEnabled = provider.IsEnabled,
            IsValidated = provider.IsValidated,
            LastValidatedAt = provider.LastValidatedAt,
            CreatedAt = provider.CreatedAt,
            UpdatedAt = provider.UpdatedAt,
        };
}
