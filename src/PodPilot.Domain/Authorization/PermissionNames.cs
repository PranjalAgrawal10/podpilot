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

    /// <summary>Read models.</summary>
    public const string ModelRead = "Model.Read";

    /// <summary>Pull models.</summary>
    public const string ModelPull = "Model.Pull";

    /// <summary>Delete models.</summary>
    public const string ModelDelete = "Model.Delete";

    /// <summary>Manage models (refresh, set default).</summary>
    public const string ModelManage = "Model.Manage";

    /// <summary>Read gateway configuration and metrics.</summary>
    public const string GatewayRead = "Gateway.Read";

    /// <summary>Manage gateway API keys and routes.</summary>
    public const string GatewayManage = "Gateway.Manage";

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

    /// <summary>Read orchestration, pools, and capacity.</summary>
    public const string OrchestratorRead = "Orchestrator.Read";

    /// <summary>Manage pod pools and auto-scaling.</summary>
    public const string OrchestratorManage = "Orchestrator.Manage";

    /// <summary>Read observability metrics, health, and analytics.</summary>
    public const string ObservabilityRead = "Observability.Read";

    /// <summary>Export observability data.</summary>
    public const string ObservabilityExport = "Observability.Export";

    /// <summary>Read AI inference providers and model catalog.</summary>
    public const string AiProviderRead = "AiProvider.Read";

    /// <summary>Create AI inference providers and routing policies.</summary>
    public const string AiProviderCreate = "AiProvider.Create";

    /// <summary>Update AI inference providers and routing policies.</summary>
    public const string AiProviderUpdate = "AiProvider.Update";

    /// <summary>Delete AI inference providers and routing policies.</summary>
    public const string AiProviderDelete = "AiProvider.Delete";

    /// <summary>Read intelligent routing dashboard, rankings, and history.</summary>
    public const string RoutingRead = "Routing.Read";

    /// <summary>Manage intelligent routing policy settings and simulations.</summary>
    public const string RoutingManage = "Routing.Manage";

    /// <summary>Read plugins and local marketplace catalog.</summary>
    public const string PluginRead = "Plugin.Read";

    /// <summary>Install and configure plugins.</summary>
    public const string PluginManage = "Plugin.Manage";

    /// <summary>Read MCP servers, tools, and resources.</summary>
    public const string McpRead = "Mcp.Read";

    /// <summary>Register and manage MCP servers; execute tools.</summary>
    public const string McpManage = "Mcp.Manage";

    /// <summary>Read security dashboard, sessions, and identity providers.</summary>
    public const string SecurityRead = "Security.Read";

    /// <summary>Manage SSO, MFA policies, sessions, and trusted devices.</summary>
    public const string SecurityManage = "Security.Manage";

    /// <summary>Read enterprise audit events.</summary>
    public const string AuditRead = "Audit.Read";

    /// <summary>Read secret metadata (never plaintext).</summary>
    public const string SecretsRead = "Secrets.Read";

    /// <summary>Create, rotate, and delete secrets.</summary>
    public const string SecretsManage = "Secrets.Manage";

    /// <summary>Read organization governance and security policies.</summary>
    public const string PolicyRead = "Policy.Read";

    /// <summary>Update organization governance and security policies.</summary>
    public const string PolicyManage = "Policy.Manage";

    /// <summary>Read compliance status and exports.</summary>
    public const string ComplianceRead = "Compliance.Read";

    /// <summary>Perform compliance actions (export, erasure, retention).</summary>
    public const string ComplianceManage = "Compliance.Manage";

    /// <summary>Read billing, subscriptions, and usage.</summary>
    public const string BillingRead = "Billing.Read";

    /// <summary>Manage subscriptions, invoices, and payment methods.</summary>
    public const string BillingManage = "Billing.Manage";

    /// <summary>Manage product licenses and activation.</summary>
    public const string LicenseManage = "License.Manage";

    /// <summary>Read license status.</summary>
    public const string LicenseRead = "License.Read";

    /// <summary>Manage backups and restores.</summary>
    public const string BackupManage = "Backup.Manage";

    /// <summary>Read backups.</summary>
    public const string BackupRead = "Backup.Read";

    /// <summary>Read one-click AI deployments and catalogs.</summary>
    public const string DeploymentRead = "Deployment.Read";

    /// <summary>Create and manage one-click AI deployments.</summary>
    public const string DeploymentManage = "Deployment.Manage";

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
        ModelRead,
        ModelPull,
        ModelDelete,
        ModelManage,
        GatewayRead,
        GatewayManage,
        DashboardView,
        BillingView,
        MemberRead,
        MemberManage,
        MemberRoleUpdate,
        InvitationCreate,
        OrchestratorRead,
        OrchestratorManage,
        ObservabilityRead,
        ObservabilityExport,
        AiProviderRead,
        AiProviderCreate,
        AiProviderUpdate,
        AiProviderDelete,
        RoutingRead,
        RoutingManage,
        PluginRead,
        PluginManage,
        McpRead,
        McpManage,
        SecurityRead,
        SecurityManage,
        AuditRead,
        SecretsRead,
        SecretsManage,
        PolicyRead,
        PolicyManage,
        ComplianceRead,
        ComplianceManage,
        BillingRead,
        BillingManage,
        LicenseRead,
        LicenseManage,
        BackupRead,
        BackupManage,
        DeploymentRead,
        DeploymentManage,
    ];
}
