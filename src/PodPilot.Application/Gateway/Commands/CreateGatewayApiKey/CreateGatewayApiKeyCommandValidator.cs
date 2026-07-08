using FluentValidation;
using PodPilot.Application.Common;

namespace PodPilot.Application.Gateway.Commands.CreateGatewayApiKey;

/// <summary>
/// Validates <see cref="CreateGatewayApiKeyCommand"/>.
/// </summary>
public sealed class CreateGatewayApiKeyCommandValidator : AbstractValidator<CreateGatewayApiKeyCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateGatewayApiKeyCommandValidator"/> class.
    /// </summary>
    public CreateGatewayApiKeyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RateLimitPerMinute)
            .GreaterThan(0)
            .When(x => x.RateLimitPerMinute.HasValue);
        RuleFor(x => x.RateLimitPerDay)
            .GreaterThan(0)
            .When(x => x.RateLimitPerDay.HasValue);
    }
}
