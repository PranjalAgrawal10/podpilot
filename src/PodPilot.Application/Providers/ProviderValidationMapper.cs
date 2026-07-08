using PodPilot.Application.Models.Compute;
using PodPilot.Contracts.Providers;

namespace PodPilot.Application.Providers;

/// <summary>
/// Maps provider validation results to API responses.
/// </summary>
internal static class ProviderValidationMapper
{
    /// <summary>
    /// Maps a validation result to a response DTO.
    /// </summary>
    /// <param name="validation">The validation result.</param>
    /// <returns>The API response.</returns>
    public static ProviderValidationResponse ToResponse(ProviderValidationResult validation) =>
        new()
        {
            IsValid = validation.IsValid,
            ErrorMessage = validation.ErrorMessage,
            ConnectionStatus = validation.ConnectionStatus.ToString(),
            AccountInfo = validation.AccountInfo is null
                ? null
                : new ProviderAccountResponse
                {
                    AccountId = validation.AccountInfo.AccountId,
                    Email = validation.AccountInfo.Email,
                    DisplayName = validation.AccountInfo.DisplayName,
                    Balance = validation.AccountInfo.Balance,
                    Currency = validation.AccountInfo.Currency,
                },
            Regions = validation.Regions
                .Select(r => new ProviderRegionResponse
                {
                    RegionId = r.RegionId,
                    Name = r.Name,
                    IsAvailable = r.IsAvailable,
                })
                .ToList(),
            Gpus = validation.Gpus
                .Select(g => new ProviderGpuResponse
                {
                    GpuId = g.GpuId,
                    Name = g.Name,
                    GpuType = g.GpuType.ToString(),
                    MemoryGb = g.MemoryGb,
                    IsAvailable = g.IsAvailable,
                })
                .ToList(),
            Templates = validation.Templates
                .Select(t => new ProviderTemplateResponse
                {
                    TemplateId = t.TemplateId,
                    Name = t.Name,
                    Description = t.Description,
                    ImageName = t.ImageName,
                })
                .ToList(),
        };
}
