using FluentValidation;

namespace PodPilot.Application.Invitations.Commands.AcceptInvitation;

/// <summary>
/// Validator for <see cref="AcceptInvitationCommand"/>.
/// </summary>
public sealed class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AcceptInvitationCommandValidator"/> class.
    /// </summary>
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Invitation token is required.");
    }
}
