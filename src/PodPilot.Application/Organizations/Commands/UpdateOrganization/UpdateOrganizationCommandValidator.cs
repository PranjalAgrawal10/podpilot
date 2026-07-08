using FluentValidation;

namespace PodPilot.Application.Organizations.Commands.UpdateOrganization;

/// <summary>
/// Validator for <see cref="UpdateOrganizationCommand"/>.
/// </summary>
public sealed class UpdateOrganizationCommandValidator : AbstractValidator<UpdateOrganizationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateOrganizationCommandValidator"/> class.
    /// </summary>
    public UpdateOrganizationCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Logo)
            .MaximumLength(2048);
    }
}
