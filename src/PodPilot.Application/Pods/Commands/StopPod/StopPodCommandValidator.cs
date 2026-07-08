using FluentValidation;

namespace PodPilot.Application.Pods.Commands.StopPod;

/// <summary>
/// Validator for <see cref="StopPodCommand"/>.
/// </summary>
public sealed class StopPodCommandValidator : AbstractValidator<StopPodCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StopPodCommandValidator"/> class.
    /// </summary>
    public StopPodCommandValidator()
    {
        RuleFor(x => x.PodId).NotEmpty();
    }
}
