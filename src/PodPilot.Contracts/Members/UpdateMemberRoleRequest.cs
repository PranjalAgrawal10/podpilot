namespace PodPilot.Contracts.Members;

/// <summary>
/// Request to update a member's organization role.
/// </summary>
public sealed class UpdateMemberRoleRequest
{
    /// <summary>
    /// Gets or sets the new role.
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
