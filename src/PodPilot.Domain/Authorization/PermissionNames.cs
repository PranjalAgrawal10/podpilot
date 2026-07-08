namespace PodPilot.Domain.Authorization;

/// <summary>
/// Canonical permission identifiers for organization-scoped authorization.
/// </summary>
public static class PermissionNames
{
    /// <summary>Read organization details.</summary>
    public const string OrganizationRead = "Organization.Read";

    /// <summary>Update organization settings.</summary>
    public const string OrganizationUpdate = "Organization.Update";

    /// <summary>Delete an organization.</summary>
    public const string OrganizationDelete = "Organization.Delete";

    /// <summary>Read pods.</summary>
    public const string PodRead = "Pod.Read";

    /// <summary>Create pods.</summary>
    public const string PodCreate = "Pod.Create";

    /// <summary>Update and operate pods.</summary>
    public const string PodUpdate = "Pod.Update";

    /// <summary>Delete pods.</summary>
    public const string PodDelete = "Pod.Delete";

    /// <summary>Read providers.</summary>
    public const string ProviderRead = "Provider.Read";

    /// <summary>Create providers.</summary>
    public const string ProviderCreate = "Provider.Create";

    /// <summary>Update providers.</summary>
    public const string ProviderUpdate = "Provider.Update";

    /// <summary>Delete providers.</summary>
    public const string ProviderDelete = "Provider.Delete";

    /// <summary>Pull models.</summary>
    public const string ModelPull = "Model.Pull";

    /// <summary>Delete models.</summary>
    public const string ModelDelete = "Model.Delete";

    /// <summary>View dashboards.</summary>
    public const string DashboardView = "Dashboard.View";

    /// <summary>View billing.</summary>
    public const string BillingView = "Billing.View";

    /// <summary>Read organization members.</summary>
    public const string MemberRead = "Member.Read";

    /// <summary>Invite and manage members.</summary>
    public const string MemberManage = "Member.Manage";

    /// <summary>Update member roles.</summary>
    public const string MemberRoleUpdate = "Member.RoleUpdate";

    /// <summary>Send invitations.</summary>
    public const string InvitationCreate = "Invitation.Create";

    /// <summary>All defined permissions.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        OrganizationRead,
        OrganizationUpdate,
        OrganizationDelete,
        PodRead,
        PodCreate,
        PodUpdate,
        PodDelete,
        ProviderRead,
        ProviderCreate,
        ProviderUpdate,
        ProviderDelete,
        ModelPull,
        ModelDelete,
        DashboardView,
        BillingView,
        MemberRead,
        MemberManage,
        MemberRoleUpdate,
        InvitationCreate,
    ];
}
