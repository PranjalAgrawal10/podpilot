using FluentValidation;

namespace PodPilot.Application.Pods.Commands.RestartPod;

/// <summary>
/// Validator for <see cref="RestartPodCommand"/>.
/// </summary>
public sealed class RestartPodCommandValidator : AbstractValidator<RestartPodCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestartPodCommandValidator"/> class.
    /// </summary>
    public RestartPodCommandValidator()
    {
        RuleFor(x => x.PodId).NotEmpty();
    }
}
