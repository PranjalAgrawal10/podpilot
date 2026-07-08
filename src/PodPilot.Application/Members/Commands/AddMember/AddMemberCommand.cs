using MediatR;
using PodPilot.Contracts.Members;

namespace PodPilot.Application.Members.Commands.AddMember;

/// <summary>
/// Command to add an existing user to an organization.
/// </summary>
public sealed class AddMemberCommand : IRequest<MemberResponse>
{
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role to assign.
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
