using FluentValidation;

namespace PodPilot.Application.Organizations.Commands.CreateOrganization;

/// <summary>
/// Validator for <see cref="CreateOrganizationCommand"/>.
/// </summary>
public sealed class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateOrganizationCommandValidator"/> class.
    /// </summary>
    public CreateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Organization name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Logo)
            .MaximumLength(2048);
    }
}
