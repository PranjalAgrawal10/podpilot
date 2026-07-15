using FluentValidation;
using PodPilot.Application.Common;

namespace PodPilot.Application.AiProviders.Commands.ValidateAiProvider;

/// <summary>
/// Validator for <see cref="ValidateAiProviderCommand"/>.
/// </summary>
public sealed class ValidateAiProviderCommandValidator : AbstractValidator<ValidateAiProviderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateAiProviderCommandValidator"/> class.
    /// </summary>
    public ValidateAiProviderCommandValidator()
    {
        RuleFor(x => x.ProviderKind).IsInEnum();
        RuleFor(x => x.BaseUrl).MaximumLength(ApplicationConstants.AiProviderBaseUrlMaxLength);
        RuleFor(x => x.DeploymentName).MaximumLength(ApplicationConstants.AiProviderDeploymentNameMaxLength);
        RuleFor(x => x.ApiVersion).MaximumLength(ApplicationConstants.AiProviderApiVersionMaxLength);
    }
}
