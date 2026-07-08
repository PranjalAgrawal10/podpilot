using FluentValidation;

namespace PodPilot.Application.Pods.Commands.StartPod;

/// <summary>
/// Validator for <see cref="StartPodCommand"/>.
/// </summary>
public sealed class StartPodCommandValidator : AbstractValidator<StartPodCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartPodCommandValidator"/> class.
    /// </summary>
    public StartPodCommandValidator()
    {
        RuleFor(x => x.PodId).NotEmpty();
    }
}
