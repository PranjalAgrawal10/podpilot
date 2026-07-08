using FluentValidation;

namespace PodPilot.Application.Providers.Commands.ValidateCredentials;

/// <summary>
/// Validator for <see cref="ValidateCredentialsCommand"/>.
/// </summary>
public sealed class ValidateCredentialsCommandValidator : AbstractValidator<ValidateCredentialsCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateCredentialsCommandValidator"/> class.
    /// </summary>
    public ValidateCredentialsCommandValidator()
    {
        RuleFor(x => x.ApiKey)
            .NotEmpty()
            .WithMessage("API key is required.");
    }
}
