using FluentValidation;

namespace PodPilot.Application.Models.Commands.RefreshModels;

/// <summary>
/// Validates <see cref="RefreshModelsCommand"/>.
/// </summary>
public sealed class RefreshModelsCommandValidator : AbstractValidator<RefreshModelsCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshModelsCommandValidator"/> class.
    /// </summary>
    public RefreshModelsCommandValidator()
    {
        RuleFor(x => x.PodId).NotEmpty();
    }
}
