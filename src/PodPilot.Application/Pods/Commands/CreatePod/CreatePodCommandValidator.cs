using FluentValidation;
using PodPilot.Application.Common;

namespace PodPilot.Application.Pods.Commands.CreatePod;

/// <summary>
/// Validator for <see cref="CreatePodCommand"/>.
/// </summary>
public sealed class CreatePodCommandValidator : AbstractValidator<CreatePodCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePodCommandValidator"/> class.
    /// </summary>
    public CreatePodCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ApplicationConstants.PodNameMaxLength);
        RuleFor(x => x.Description).MaximumLength(ApplicationConstants.PodDescriptionMaxLength);
        RuleFor(x => x.GpuId).NotEmpty();
        RuleFor(x => x.Region).NotEmpty().MaximumLength(ApplicationConstants.PodRegionMaxLength);
        RuleFor(x => x.ImageName)
            .NotEmpty()
            .When(x => string.IsNullOrWhiteSpace(x.TemplateId))
            .WithMessage("Image name is required when no template is selected.");
        RuleFor(x => x.TemplateId)
            .NotEmpty()
            .When(x => string.IsNullOrWhiteSpace(x.ImageName))
            .WithMessage("Template is required when no image name is provided.");
        RuleFor(x => x.ImageName).MaximumLength(ApplicationConstants.PodImageNameMaxLength);
        RuleFor(x => x.ContainerDiskGb)
            .InclusiveBetween(ApplicationConstants.PodMinContainerDiskGb, ApplicationConstants.PodMaxContainerDiskGb);
        RuleFor(x => x.VolumeDiskGb)
            .InclusiveBetween(ApplicationConstants.PodMinVolumeDiskGb, ApplicationConstants.PodMaxVolumeDiskGb);
        RuleFor(x => x.GpuCount).GreaterThan(0);
    }
}
