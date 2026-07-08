using FluentValidation;

namespace PodPilot.Application.Providers.Commands.ValidateProvider;

/// <summary>
/// Validator for <see cref="ValidateProviderCommand"/>.
/// </summary>
public sealed class ValidateProviderCommandValidator : AbstractValidator<ValidateProviderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateProviderCommandValidator"/> class.
    /// </summary>
    public ValidateProviderCommandValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty();

        RuleFor(x => x.ApiKey)
            .NotEmpty()
            .When(x => x.ApiKey is not null);
    }
}
