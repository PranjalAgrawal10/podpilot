namespace PodPilot.Contracts.Members;

/// <summary>
/// Request to add an existing user to an organization.
/// </summary>
public sealed class AddMemberRequest
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role to assign.
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
