using FluentValidation;

namespace PodPilot.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Validator for <see cref="RefreshTokenCommand"/>.
/// </summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenCommandValidator"/> class.
    /// </summary>
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
