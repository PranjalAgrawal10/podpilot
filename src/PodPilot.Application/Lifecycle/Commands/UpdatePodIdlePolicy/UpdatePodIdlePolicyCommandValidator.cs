using FluentValidation;
using PodPilot.Application.Common;

namespace PodPilot.Application.Lifecycle.Commands.UpdatePodIdlePolicy;

/// <summary>
/// Validates update pod idle policy commands.
/// </summary>
public sealed class UpdatePodIdlePolicyCommandValidator : AbstractValidator<UpdatePodIdlePolicyCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePodIdlePolicyCommandValidator"/> class.
    /// </summary>
    public UpdatePodIdlePolicyCommandValidator()
    {
        RuleFor(x => x.PodId).NotEmpty();
        RuleFor(x => x.IdleTimeoutMinutes).InclusiveBetween(1, 24 * 60);
        RuleFor(x => x.GracePeriodMinutes).InclusiveBetween(0, 60);
        RuleFor(x => x.MinimumRunningTimeMinutes).InclusiveBetween(0, 24 * 60);
    }
}
