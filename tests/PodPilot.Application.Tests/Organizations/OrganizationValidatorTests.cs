using FluentValidation.TestHelper;
using PodPilot.Application.Invitations.Commands.InviteMember;
using PodPilot.Application.Organizations.Commands.CreateOrganization;
using PodPilot.Application.Organizations.Commands.UpdateOrganization;

namespace PodPilot.Application.Tests.Organizations;

public class CreateOrganizationCommandValidatorTests
{
    private readonly CreateOrganizationCommandValidator validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreateOrganizationCommand { Name = string.Empty };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var command = new CreateOrganizationCommand
        {
            Name = "Acme Corp",
            Description = "Test organization",
        };

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class UpdateOrganizationCommandValidatorTests
{
    private readonly UpdateOrganizationCommandValidator validator = new();

    [Fact]
    public void Should_Have_Error_When_OrganizationId_Is_Empty()
    {
        var command = new UpdateOrganizationCommand
        {
            OrganizationId = Guid.Empty,
            Name = "Updated Name",
        };

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrganizationId);
    }
}

public class InviteMemberCommandValidatorTests
{
    private readonly InviteMemberCommandValidator validator = new();

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var command = new InviteMemberCommand
        {
            OrganizationId = Guid.NewGuid(),
            Email = "invalid-email",
            Role = "Viewer",
        };

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var command = new InviteMemberCommand
        {
            OrganizationId = Guid.NewGuid(),
            Email = "invitee@example.com",
            Role = "Developer",
        };

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
