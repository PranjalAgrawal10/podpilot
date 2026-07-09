using FluentValidation;
using PodPilot.Application.Common;

namespace PodPilot.Application.Orchestration.Commands.CreatePodPool;

/// <summary>
/// Validates <see cref="CreatePodPoolCommand"/>.
/// </summary>
public sealed class CreatePodPoolCommandValidator : AbstractValidator<CreatePodPoolCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePodPoolCommandValidator"/> class.
    /// </summary>
    public CreatePodPoolCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ApplicationConstants.PodNameMaxLength);
    }
}
