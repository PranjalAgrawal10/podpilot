using System.Text.RegularExpressions;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common;

/// <summary>
/// Shared application constants and helpers.
/// </summary>
public static class ApplicationConstants
{
    /// <summary>
    /// The ASP.NET Identity Admin role name.
    /// </summary>
    public const string AdminRole = "Admin";

    /// <summary>
    /// The ASP.NET Identity Member role name.
    /// </summary>
    public const string MemberRole = "Member";

    /// <summary>
    /// JWT claim for current organization identifier.
    /// </summary>
    public const string OrganizationIdClaim = "organization_id";

    /// <summary>
    /// JWT claim for current organization role.
    /// </summary>
    public const string OrganizationRoleClaim = "organization_role";

    /// <summary>
    /// Default invitation validity in days.
    /// </summary>
    public const int InvitationExpirationDays = 7;

    /// <summary>
    /// All ASP.NET Identity roles.
    /// </summary>
    public static readonly string[] AllRoles = [AdminRole, MemberRole];

    /// <summary>
    /// Creates a URL-friendly slug from an organization name.
    /// </summary>
    /// <param name="name">The organization name.</param>
    /// <returns>A URL-friendly slug.</returns>
    public static string CreateSlug(string name)
    {
        var slug = name.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }

    /// <summary>
    /// Maps an <see cref="OrganizationRole"/> to its string representation.
    /// </summary>
    /// <param name="role">The role enum value.</param>
    /// <returns>The role name.</returns>
    public static string ToRoleName(OrganizationRole role) => role.ToString();

    /// <summary>
    /// Parses a role name into an <see cref="OrganizationRole"/>.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <returns>The parsed role.</returns>
    public static OrganizationRole ParseRoleName(string roleName) =>
        Enum.TryParse<OrganizationRole>(roleName, ignoreCase: true, out var role)
            ? role
            : OrganizationRole.Viewer;
}
