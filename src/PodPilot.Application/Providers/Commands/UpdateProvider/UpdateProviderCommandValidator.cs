using FluentValidation;
using PodPilot.Application.Common;

namespace PodPilot.Application.Providers.Commands.UpdateProvider;

/// <summary>
/// Validator for <see cref="UpdateProviderCommand"/>.
/// </summary>
public sealed class UpdateProviderCommandValidator : AbstractValidator<UpdateProviderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProviderCommandValidator"/> class.
    /// </summary>
    public UpdateProviderCommandValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .MaximumLength(ApplicationConstants.ProviderNameMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.DisplayName)
            .MaximumLength(ApplicationConstants.ProviderDisplayNameMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.ProviderDescriptionMaxLength)
            .When(x => x.Description is not null);

        RuleFor(x => x.DefaultRegion)
            .MaximumLength(ApplicationConstants.ProviderRegionMaxLength)
            .When(x => x.DefaultRegion is not null);

        RuleFor(x => x.ApiKey)
            .NotEmpty()
            .When(x => x.ApiKey is not null);
    }
}
