using FluentValidation;
using PodPilot.Application.Common;

namespace PodPilot.Application.AiProviders.Commands.CreateRoutingPolicy;

/// <summary>
/// Validator for <see cref="CreateRoutingPolicyCommand"/>.
/// </summary>
public sealed class CreateRoutingPolicyCommandValidator : AbstractValidator<CreateRoutingPolicyCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateRoutingPolicyCommandValidator"/> class.
    /// </summary>
    public CreateRoutingPolicyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ApplicationConstants.ProviderNameMaxLength);
        RuleFor(x => x.ModelName).MaximumLength(ApplicationConstants.AiProviderModelNameMaxLength);
        RuleFor(x => x.PrimaryProviderId).NotEmpty();
        RuleFor(x => x.FailoverStrategy).IsInEnum();
        RuleFor(x => x.MaxRetries).InclusiveBetween(0, 10);
    }
}
