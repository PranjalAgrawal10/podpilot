using FluentValidation;

namespace PodPilot.Application.Pods.Commands.DeletePod;

/// <summary>
/// Validator for <see cref="DeletePodCommand"/>.
/// </summary>
public sealed class DeletePodCommandValidator : AbstractValidator<DeletePodCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePodCommandValidator"/> class.
    /// </summary>
    public DeletePodCommandValidator()
    {
        RuleFor(x => x.PodId).NotEmpty();
    }
}
