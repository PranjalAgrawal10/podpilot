using FluentValidation;

namespace PodPilot.Application.Gateway.Commands.CreateGatewayRoute;

/// <summary>
/// Validates <see cref="CreateGatewayRouteCommand"/>.
/// </summary>
public sealed class CreateGatewayRouteCommandValidator : AbstractValidator<CreateGatewayRouteCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateGatewayRouteCommandValidator"/> class.
    /// </summary>
    public CreateGatewayRouteCommandValidator()
    {
        RuleFor(x => x.GpuPodId).NotEmpty();
        RuleFor(x => x.ModelName).NotEmpty().MaximumLength(200);
    }
}
