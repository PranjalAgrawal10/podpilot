using FluentValidation.TestHelper;
using PodPilot.Application.Providers.Commands.CreateProvider;
using PodPilot.Application.Providers.Commands.UpdateProvider;
using PodPilot.Application.Providers.Commands.ValidateProvider;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Tests.Providers;

public class CreateProviderCommandValidatorTests
{
    private readonly CreateProviderCommandValidator validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreateProviderCommand
        {
            Name = string.Empty,
            DisplayName = "RunPod Primary",
            ProviderType = ProviderType.RunPod,
            ApiKey = "test-key",
        };

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_ApiKey_Is_Empty()
    {
        var command = new CreateProviderCommand
        {
            Name = "runpod-primary",
            DisplayName = "RunPod Primary",
            ProviderType = ProviderType.RunPod,
            ApiKey = string.Empty,
        };

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ApiKey);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var command = new CreateProviderCommand
        {
            Name = "runpod-primary",
            DisplayName = "RunPod Primary",
            ProviderType = ProviderType.RunPod,
            ApiKey = "rp_test_key",
            Description = "Primary GPU provider",
        };

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class UpdateProviderCommandValidatorTests
{
    private readonly UpdateProviderCommandValidator validator = new();

    [Fact]
    public void Should_Have_Error_When_ProviderId_Is_Empty()
    {
        var command = new UpdateProviderCommand
        {
            ProviderId = Guid.Empty,
            DisplayName = "Updated Name",
        };

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProviderId);
    }

    [Fact]
    public void Should_Have_Error_When_ApiKey_Is_Empty_String()
    {
        var command = new UpdateProviderCommand
        {
            ProviderId = Guid.NewGuid(),
            ApiKey = string.Empty,
        };

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ApiKey);
    }
}

public class ValidateProviderCommandValidatorTests
{
    private readonly ValidateProviderCommandValidator validator = new();

    [Fact]
    public void Should_Have_Error_When_ProviderId_Is_Empty()
    {
        var command = new ValidateProviderCommand { ProviderId = Guid.Empty };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProviderId);
    }
}
