using FluentValidation;
using PodPilot.Application.Common;

namespace PodPilot.Application.AiProviders.Commands.UpdateAiProvider;

/// <summary>
/// Validator for <see cref="UpdateAiProviderCommand"/>.
/// </summary>
public sealed class UpdateAiProviderCommandValidator : AbstractValidator<UpdateAiProviderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAiProviderCommandValidator"/> class.
    /// </summary>
    public UpdateAiProviderCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Provider name is required.")
            .MaximumLength(ApplicationConstants.ProviderNameMaxLength);
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(ApplicationConstants.ProviderDisplayNameMaxLength);
        RuleFor(x => x.Description).MaximumLength(ApplicationConstants.ProviderDescriptionMaxLength);
        RuleFor(x => x.BaseUrl).MaximumLength(ApplicationConstants.AiProviderBaseUrlMaxLength);
        RuleFor(x => x.DeploymentName).MaximumLength(ApplicationConstants.AiProviderDeploymentNameMaxLength);
        RuleFor(x => x.ApiVersion).MaximumLength(ApplicationConstants.AiProviderApiVersionMaxLength);
    }
}
