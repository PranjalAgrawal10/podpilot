using FluentValidation;
using PodPilot.Application.Common;

namespace PodPilot.Application.AiProviders.Commands.CreateAiProvider;

/// <summary>
/// Validator for <see cref="CreateAiProviderCommand"/>.
/// </summary>
public sealed class CreateAiProviderCommandValidator : AbstractValidator<CreateAiProviderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateAiProviderCommandValidator"/> class.
    /// </summary>
    public CreateAiProviderCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Provider name is required.")
            .MaximumLength(ApplicationConstants.ProviderNameMaxLength);

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(ApplicationConstants.ProviderDisplayNameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.ProviderDescriptionMaxLength);

        RuleFor(x => x.BaseUrl)
            .MaximumLength(ApplicationConstants.AiProviderBaseUrlMaxLength);

        RuleFor(x => x.DeploymentName)
            .MaximumLength(ApplicationConstants.AiProviderDeploymentNameMaxLength);

        RuleFor(x => x.ApiVersion)
            .MaximumLength(ApplicationConstants.AiProviderApiVersionMaxLength);

        RuleFor(x => x.ProviderKind)
            .IsInEnum().WithMessage("Provider kind is invalid.");
    }
}
