using FluentValidation;

namespace PodPilot.Application.Routing.Commands.SimulateRouting;

/// <summary>Validates <see cref="SimulateRoutingCommand"/>.</summary>
public sealed class SimulateRoutingCommandValidator : AbstractValidator<SimulateRoutingCommand>
{
    /// <summary>Initializes a new instance of the <see cref="SimulateRoutingCommandValidator"/> class.</summary>
    public SimulateRoutingCommandValidator()
    {
        RuleFor(x => x.Prompt).NotEmpty().MaximumLength(20000);
        RuleFor(x => x.ModelHint).MaximumLength(200);
        RuleFor(x => x.Path).MaximumLength(500);
    }
}
