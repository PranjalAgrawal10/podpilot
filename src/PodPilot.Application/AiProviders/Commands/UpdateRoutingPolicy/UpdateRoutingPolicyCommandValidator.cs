using FluentValidation;
using PodPilot.Application.Common;

namespace PodPilot.Application.AiProviders.Commands.UpdateRoutingPolicy;

/// <summary>
/// Validator for <see cref="UpdateRoutingPolicyCommand"/>.
/// </summary>
public sealed class UpdateRoutingPolicyCommandValidator : AbstractValidator<UpdateRoutingPolicyCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoutingPolicyCommandValidator"/> class.
    /// </summary>
    public UpdateRoutingPolicyCommandValidator()
    {
        RuleFor(x => x.PolicyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ApplicationConstants.ProviderNameMaxLength);
        RuleFor(x => x.ModelName).MaximumLength(ApplicationConstants.AiProviderModelNameMaxLength);
        RuleFor(x => x.PrimaryProviderId).NotEmpty();
        RuleFor(x => x.FailoverStrategy).IsInEnum();
        RuleFor(x => x.MaxRetries).InclusiveBetween(0, 10);
    }
}
