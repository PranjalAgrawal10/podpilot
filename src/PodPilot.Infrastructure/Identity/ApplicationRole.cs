using Microsoft.AspNetCore.Identity;

namespace PodPilot.Infrastructure.Identity;

/// <summary>
/// Application role entity integrated with ASP.NET Identity.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationRole"/> class.
    /// </summary>
    public ApplicationRole()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationRole"/> class.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    public ApplicationRole(string roleName)
        : base(roleName)
    {
    }
}
