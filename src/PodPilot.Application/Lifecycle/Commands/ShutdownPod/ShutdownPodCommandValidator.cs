using FluentValidation;

namespace PodPilot.Application.Lifecycle.Commands.ShutdownPod;

/// <summary>
/// Validates shutdown pod commands.
/// </summary>
public sealed class ShutdownPodCommandValidator : AbstractValidator<ShutdownPodCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShutdownPodCommandValidator"/> class.
    /// </summary>
    public ShutdownPodCommandValidator()
    {
        RuleFor(x => x.PodId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
