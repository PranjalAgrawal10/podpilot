using FluentValidation;
using PodPilot.Application.Common;

namespace PodPilot.Application.Pods.Commands.UpdatePod;

/// <summary>
/// Validator for <see cref="UpdatePodCommand"/>.
/// </summary>
public sealed class UpdatePodCommandValidator : AbstractValidator<UpdatePodCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePodCommandValidator"/> class.
    /// </summary>
    public UpdatePodCommandValidator()
    {
        RuleFor(x => x.PodId).NotEmpty();
        RuleFor(x => x.Name)
            .MaximumLength(ApplicationConstants.PodNameMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Name));
        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.PodDescriptionMaxLength)
            .When(x => x.Description is not null);
    }
}
