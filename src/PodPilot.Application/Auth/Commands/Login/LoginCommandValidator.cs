using FluentValidation;
using PodPilot.Domain.ValueObjects;

namespace PodPilot.Application.Auth.Commands.Login;

/// <summary>
/// Validator for <see cref="LoginCommand"/>.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandValidator"/> class.
    /// </summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Must(BeValidEmail).WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }

    private static bool BeValidEmail(string email) =>
        Email.TryCreate(email, out _);
}
