using FluentValidation;

namespace PodPilot.Application.Models.Commands.PullModel;

/// <summary>
/// Validates <see cref="PullModelCommand"/>.
/// </summary>
public sealed class PullModelCommandValidator : AbstractValidator<PullModelCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PullModelCommandValidator"/> class.
    /// </summary>
    public PullModelCommandValidator()
    {
        RuleFor(x => x.PodId).NotEmpty();
        RuleFor(x => x.Model).NotEmpty().MaximumLength(200);
    }
}
