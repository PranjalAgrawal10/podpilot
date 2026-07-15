using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Authorization;

/// <summary>
/// Maps organization roles to their granted permissions.
/// </summary>
public static class RolePermissionMatrix
{
    private static readonly IReadOnlyDictionary<OrganizationRole, IReadOnlySet<string>> Matrix =
        new Dictionary<OrganizationRole, IReadOnlySet<string>>
        {
            [OrganizationRole.Owner] = new HashSet<string>(PermissionNames.All),
            [OrganizationRole.Admin] =
                new HashSet<string>
                {
                    PermissionNames.OrganizationRead,
                    PermissionNames.OrganizationUpdate,
                    PermissionNames.PodRead,
                    PermissionNames.PodCreate,
                    PermissionNames.PodUpdate,
                    PermissionNames.PodDelete,
                    PermissionNames.ProviderRead,
                    PermissionNames.ProviderCreate,
                    PermissionNames.ProviderUpdate,
                    PermissionNames.ProviderDelete,
                    PermissionNames.ModelRead,
                    PermissionNames.ModelPull,
                    PermissionNames.ModelDelete,
                    PermissionNames.ModelManage,
                    PermissionNames.GatewayRead,
                    PermissionNames.GatewayManage,
                    PermissionNames.DashboardView,
                    PermissionNames.BillingView,
                    PermissionNames.MemberRead,
                    PermissionNames.MemberManage,
                    PermissionNames.MemberRoleUpdate,
                    PermissionNames.InvitationCreate,
                    PermissionNames.OrchestratorRead,
                    PermissionNames.OrchestratorManage,
                    PermissionNames.ObservabilityRead,
                    PermissionNames.ObservabilityExport,
                    PermissionNames.AiProviderRead,
                    PermissionNames.AiProviderCreate,
                    PermissionNames.AiProviderUpdate,
                    PermissionNames.AiProviderDelete,
                    PermissionNames.RoutingRead,
                    PermissionNames.RoutingManage,
                },
            [OrganizationRole.Developer] =
                new HashSet<string>
                {
                    PermissionNames.OrganizationRead,
                    PermissionNames.PodRead,
                    PermissionNames.PodCreate,
                    PermissionNames.PodUpdate,
                    PermissionNames.PodDelete,
                    PermissionNames.ProviderRead,
                    PermissionNames.ProviderCreate,
                    PermissionNames.ProviderUpdate,
                    PermissionNames.ProviderDelete,
                    PermissionNames.ModelRead,
                    PermissionNames.ModelPull,
                    PermissionNames.ModelDelete,
                    PermissionNames.ModelManage,
                    PermissionNames.GatewayRead,
                    PermissionNames.GatewayManage,
                    PermissionNames.DashboardView,
                    PermissionNames.MemberRead,
                    PermissionNames.OrchestratorRead,
                    PermissionNames.OrchestratorManage,
                    PermissionNames.ObservabilityRead,
                    PermissionNames.AiProviderRead,
                    PermissionNames.AiProviderCreate,
                    PermissionNames.AiProviderUpdate,
                    PermissionNames.AiProviderDelete,
                    PermissionNames.RoutingRead,
                    PermissionNames.RoutingManage,
                },
            [OrganizationRole.Viewer] =
                new HashSet<string>
                {
                    PermissionNames.OrganizationRead,
                    PermissionNames.PodRead,
                    PermissionNames.ProviderRead,
                    PermissionNames.ModelRead,
                    PermissionNames.GatewayRead,
                    PermissionNames.DashboardView,
                    PermissionNames.BillingView,
                    PermissionNames.MemberRead,
                    PermissionNames.OrchestratorRead,
                    PermissionNames.ObservabilityRead,
                    PermissionNames.AiProviderRead,
                    PermissionNames.RoutingRead,
                },
        };

    /// <summary>
    /// Gets permissions granted to the specified role.
    /// </summary>
    /// <param name="role">The organization role.</param>
    /// <returns>Permission names.</returns>
    public static IReadOnlySet<string> GetPermissions(OrganizationRole role) =>
        Matrix.TryGetValue(role, out var permissions)
            ? permissions
            : new HashSet<string>();

    /// <summary>
    /// Determines whether a role has the specified permission.
    /// </summary>
    /// <param name="role">The organization role.</param>
    /// <param name="permission">The permission name.</param>
    /// <returns><c>true</c> if granted; otherwise <c>false</c>.</returns>
    public static bool HasPermission(OrganizationRole role, string permission) =>
        GetPermissions(role).Contains(permission);
}
