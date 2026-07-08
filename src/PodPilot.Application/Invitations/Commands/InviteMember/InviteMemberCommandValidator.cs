using FluentValidation;
using PodPilot.Domain.ValueObjects;

namespace PodPilot.Application.Invitations.Commands.InviteMember;

/// <summary>
/// Validator for <see cref="InviteMemberCommand"/>.
/// </summary>
public sealed class InviteMemberCommandValidator : AbstractValidator<InviteMemberCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InviteMemberCommandValidator"/> class.
    /// </summary>
    public InviteMemberCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Must(BeValidEmail).WithMessage("Email format is invalid.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.");
    }

    private static bool BeValidEmail(string email) =>
        Email.TryCreate(email, out _);
}
