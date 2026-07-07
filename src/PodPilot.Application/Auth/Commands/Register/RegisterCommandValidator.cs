using FluentValidation;
using PodPilot.Domain.ValueObjects;

namespace PodPilot.Application.Auth.Commands.Register;

/// <summary>
/// Validator for <see cref="RegisterCommand"/>.
/// </summary>
public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterCommandValidator"/> class.
    /// </summary>
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Must(BeValidEmail).WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.OrganizationName)
            .NotEmpty().WithMessage("Organization name is required.")
            .MaximumLength(200);
    }

    private static bool BeValidEmail(string email) =>
        Email.TryCreate(email, out _);
}
