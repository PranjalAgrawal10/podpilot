using FluentValidation;

namespace PodPilot.Application.Pods.Commands.SyncPod;

/// <summary>
/// Validator for <see cref="SyncPodCommand"/>.
/// </summary>
public sealed class SyncPodCommandValidator : AbstractValidator<SyncPodCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SyncPodCommandValidator"/> class.
    /// </summary>
    public SyncPodCommandValidator()
    {
        RuleFor(x => x.PodId).NotEmpty();
    }
}
