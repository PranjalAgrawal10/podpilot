using FluentValidation;

namespace PodPilot.Application.Auth.Commands.Logout;

/// <summary>
/// Validator for <see cref="LogoutCommand"/>.
/// </summary>
public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogoutCommandValidator"/> class.
    /// </summary>
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
