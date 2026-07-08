using FluentValidation;
using PodPilot.Application.Common;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Providers.Commands.CreateProvider;

/// <summary>
/// Validator for <see cref="CreateProviderCommand"/>.
/// </summary>
public sealed class CreateProviderCommandValidator : AbstractValidator<CreateProviderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateProviderCommandValidator"/> class.
    /// </summary>
    public CreateProviderCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Provider name is required.")
            .MaximumLength(ApplicationConstants.ProviderNameMaxLength);

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(ApplicationConstants.ProviderDisplayNameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.ProviderDescriptionMaxLength);

        RuleFor(x => x.DefaultRegion)
            .MaximumLength(ApplicationConstants.ProviderRegionMaxLength);

        RuleFor(x => x.ApiKey)
            .NotEmpty().WithMessage("API key is required.");

        RuleFor(x => x.ProviderType)
            .IsInEnum().WithMessage("Provider type is invalid.");
    }
}
