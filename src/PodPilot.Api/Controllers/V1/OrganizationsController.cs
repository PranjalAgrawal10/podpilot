using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Invitations.Commands.AcceptInvitation;
using PodPilot.Application.Invitations.Commands.InviteMember;
using PodPilot.Application.Members.Commands.AddMember;
using PodPilot.Application.Members.Commands.RemoveMember;
using PodPilot.Application.Members.Commands.UpdateMemberRole;
using PodPilot.Application.Members.Queries.ListMembers;
using PodPilot.Application.Organizations.Commands.CreateOrganization;
using PodPilot.Application.Organizations.Commands.DeleteOrganization;
using PodPilot.Application.Organizations.Commands.SwitchOrganization;
using PodPilot.Application.Organizations.Commands.UpdateOrganization;
using PodPilot.Application.Organizations.Queries.GetOrganization;
using PodPilot.Application.Organizations.Queries.ListOrganizations;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Invitations;
using PodPilot.Contracts.Members;
using PodPilot.Contracts.Organizations;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Organization management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/organizations")]
[Authorize]
[Produces("application/json")]
public sealed class OrganizationsController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizationsController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    public OrganizationsController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Lists organizations for the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrganizationResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListOrganizations(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListOrganizationsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<OrganizationResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets an organization by identifier.
    /// </summary>
    [HttpGet("{organizationId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganization(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetOrganizationQuery { OrganizationId = organizationId },
            cancellationToken);

        return Ok(ApiResponse<OrganizationResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Creates a new organization.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrganizationResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateOrganization(
        [FromBody] CreateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateOrganizationCommand
            {
                Name = request.Name,
                Description = request.Description,
                Logo = request.Logo,
            },
            cancellationToken);

        return CreatedAtAction(
            nameof(GetOrganization),
            new { organizationId = result.Id },
            ApiResponse<OrganizationResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Updates an organization.
    /// </summary>
    [HttpPut("{organizationId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateOrganization(
        Guid organizationId,
        [FromBody] UpdateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateOrganizationCommand
            {
                OrganizationId = organizationId,
                Name = request.Name,
                Description = request.Description,
                Logo = request.Logo,
            },
            cancellationToken);

        return Ok(ApiResponse<OrganizationResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Deletes an organization.
    /// </summary>
    [HttpDelete("{organizationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteOrganization(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new DeleteOrganizationCommand { OrganizationId = organizationId },
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Switches the current organization context and re-issues tokens.
    /// </summary>
    [HttpPost("switch")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SwitchOrganization(
        [FromBody] SwitchOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SwitchOrganizationCommand { OrganizationId = request.OrganizationId },
            cancellationToken);

        return Ok(ApiResponse<AuthResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Lists members of an organization.
    /// </summary>
    [HttpGet("{organizationId:guid}/members")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MemberResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListMembers(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ListMembersQuery { OrganizationId = organizationId },
            cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<MemberResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Adds an existing user to an organization.
    /// </summary>
    [HttpPost("{organizationId:guid}/members")]
    [ProducesResponseType(typeof(ApiResponse<MemberResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddMember(
        Guid organizationId,
        [FromBody] AddMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AddMemberCommand
            {
                OrganizationId = organizationId,
                Email = request.Email,
                Role = request.Role,
            },
            cancellationToken);

        return CreatedAtAction(
            nameof(ListMembers),
            new { organizationId },
            ApiResponse<MemberResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Removes a member from an organization.
    /// </summary>
    [HttpDelete("{organizationId:guid}/members/{memberId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveMember(
        Guid organizationId,
        Guid memberId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new RemoveMemberCommand
            {
                OrganizationId = organizationId,
                MemberId = memberId,
            },
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates a member's organization role.
    /// </summary>
    [HttpPut("{organizationId:guid}/members/{memberId:guid}/role")]
    [ProducesResponseType(typeof(ApiResponse<MemberResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMemberRole(
        Guid organizationId,
        Guid memberId,
        [FromBody] UpdateMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateMemberRoleCommand
            {
                OrganizationId = organizationId,
                MemberId = memberId,
                Role = request.Role,
            },
            cancellationToken);

        return Ok(ApiResponse<MemberResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Invites a user to join an organization.
    /// </summary>
    [HttpPost("{organizationId:guid}/invite")]
    [ProducesResponseType(typeof(ApiResponse<InvitationResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> InviteMember(
        Guid organizationId,
        [FromBody] InviteMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new InviteMemberCommand
            {
                OrganizationId = organizationId,
                Email = request.Email,
                Role = request.Role,
            },
            cancellationToken);

        return CreatedAtAction(
            nameof(InviteMember),
            new { organizationId },
            ApiResponse<InvitationResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Accepts an organization invitation.
    /// </summary>
    [HttpPost("accept")]
    [ProducesResponseType(typeof(ApiResponse<MemberResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AcceptInvitation(
        [FromBody] AcceptInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AcceptInvitationCommand { Token = request.Token },
            cancellationToken);

        return Ok(ApiResponse<MemberResponse>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString();
}
