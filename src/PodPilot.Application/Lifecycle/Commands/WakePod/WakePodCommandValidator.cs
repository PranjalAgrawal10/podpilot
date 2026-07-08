using FluentValidation;

namespace PodPilot.Application.Lifecycle.Commands.WakePod;

/// <summary>
/// Validates wake pod commands.
/// </summary>
public sealed class WakePodCommandValidator : AbstractValidator<WakePodCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WakePodCommandValidator"/> class.
    /// </summary>
    public WakePodCommandValidator()
    {
        RuleFor(x => x.PodId).NotEmpty();
    }
}
