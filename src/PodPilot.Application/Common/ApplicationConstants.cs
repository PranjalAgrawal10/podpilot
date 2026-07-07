using System.Text.RegularExpressions;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common;

/// <summary>
/// Shared application constants and helpers.
/// </summary>
public static class ApplicationConstants
{
    /// <summary>
    /// The Admin role name.
    /// </summary>
    public const string AdminRole = "Admin";

    /// <summary>
    /// The Member role name.
    /// </summary>
    public const string MemberRole = "Member";

    /// <summary>
    /// All application roles.
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
    /// Maps a <see cref="UserRole"/> enum to its string representation.
    /// </summary>
    /// <param name="role">The role enum value.</param>
    /// <returns>The role name.</returns>
    public static string ToRoleName(UserRole role) =>
        role switch
        {
            UserRole.Admin => AdminRole,
            _ => MemberRole,
        };
}
