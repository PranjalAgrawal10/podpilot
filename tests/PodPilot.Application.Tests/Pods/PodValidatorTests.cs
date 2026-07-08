using FluentValidation.TestHelper;
using PodPilot.Application.Pods.Commands.CreatePod;
using PodPilot.Application.Pods.Commands.DeletePod;
using PodPilot.Application.Pods.Commands.StartPod;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Tests.Pods;

public class CreatePodCommandValidatorTests
{
    private readonly CreatePodCommandValidator validator = new();

    [Fact]
    public void Should_Pass_For_Valid_Command()
    {
        var command = new CreatePodCommand
        {
            ProviderId = Guid.NewGuid(),
            Name = "training-pod",
            GpuId = "NVIDIA GeForce RTX 4090",
            GpuType = GpuType.RTX4090,
            Region = "US",
            ImageName = "runpod/pytorch:2.1.0-py3.10-cuda11.8.0-devel-ubuntu22.04",
            ContainerDiskGb = 50,
            VolumeDiskGb = 20,
        };

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreatePodCommand
        {
            ProviderId = Guid.NewGuid(),
            Name = string.Empty,
            GpuId = "gpu",
            GpuType = GpuType.RTX4090,
            Region = "US",
            ImageName = "image",
        };

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Region_Is_Empty()
    {
        var command = new CreatePodCommand
        {
            ProviderId = Guid.NewGuid(),
            Name = "pod",
            GpuId = "gpu",
            GpuType = GpuType.RTX4090,
            Region = string.Empty,
            ImageName = "image",
        };

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Region);
    }
}

public class StartPodCommandValidatorTests
{
    private readonly StartPodCommandValidator validator = new();

    [Fact]
    public void Should_Have_Error_When_PodId_Is_Empty()
    {
        var command = new StartPodCommand { PodId = Guid.Empty };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PodId);
    }
}

public class DeletePodCommandValidatorTests
{
    private readonly DeletePodCommandValidator validator = new();

    [Fact]
    public void Should_Have_Error_When_PodId_Is_Empty()
    {
        var command = new DeletePodCommand { PodId = Guid.Empty };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PodId);
    }
}
