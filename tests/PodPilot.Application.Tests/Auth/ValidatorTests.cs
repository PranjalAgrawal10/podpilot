using FluentValidation.TestHelper;
using PodPilot.Application.Auth.Commands.Login;
using PodPilot.Application.Auth.Commands.Register;

namespace PodPilot.Application.Tests.Auth;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator validator = new();

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var command = new RegisterCommand { Email = string.Empty };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var command = new RegisterCommand { Email = "not-an-email" };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Weak()
    {
        var command = new RegisterCommand
        {
            Email = "user@example.com",
            Password = "weak",
            FirstName = "Test",
            LastName = "User",
            OrganizationName = "Test Org",
        };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var command = new RegisterCommand
        {
            Email = "user@example.com",
            Password = "SecureP@ss1",
            FirstName = "Test",
            LastName = "User",
            OrganizationName = "Test Org",
        };
        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator validator = new();

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var command = new LoginCommand { Email = "invalid", Password = "password" };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var command = new LoginCommand { Email = "user@example.com", Password = "password" };
        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
