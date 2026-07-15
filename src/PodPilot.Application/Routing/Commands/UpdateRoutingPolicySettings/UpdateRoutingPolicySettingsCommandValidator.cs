using FluentValidation;

namespace PodPilot.Application.Routing.Commands.UpdateRoutingPolicySettings;

/// <summary>Validates <see cref="UpdateRoutingPolicySettingsCommand"/>.</summary>
public sealed class UpdateRoutingPolicySettingsCommandValidator : AbstractValidator<UpdateRoutingPolicySettingsCommand>
{
    /// <summary>Initializes a new instance of the <see cref="UpdateRoutingPolicySettingsCommandValidator"/> class.</summary>
    public UpdateRoutingPolicySettingsCommandValidator()
    {
        RuleFor(x => x.MaxRetries).InclusiveBetween(0, 10);
        RuleFor(x => x.CostWeight).InclusiveBetween(0, 1);
        RuleFor(x => x.LatencyWeight).InclusiveBetween(0, 1);
        RuleFor(x => x.ReliabilityWeight).InclusiveBetween(0, 1);
        RuleFor(x => x.ContextWeight).InclusiveBetween(0, 1);
        RuleFor(x => x.FeaturesWeight).InclusiveBetween(0, 1);
        RuleFor(x => x.AvailabilityWeight).InclusiveBetween(0, 1);
        RuleFor(x => x)
            .Must(x =>
            {
                var sum = x.CostWeight + x.LatencyWeight + x.ReliabilityWeight +
                          x.ContextWeight + x.FeaturesWeight + x.AvailabilityWeight;
                return sum is >= 0.99 and <= 1.01;
            })
            .WithMessage("Scoring weights must sum to 1.0.");
        RuleFor(x => x.CustomRulesJson).MaximumLength(8000);
        RuleFor(x => x)
            .Must(x => x.Strategy != Domain.Enums.RoutingStrategy.ProviderPriority || x.PrimaryProviderId.HasValue)
            .WithMessage("Primary provider is required for ProviderPriority strategy.");
    }
}
