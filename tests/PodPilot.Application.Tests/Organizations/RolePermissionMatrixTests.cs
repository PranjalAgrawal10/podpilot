using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Tests.Organizations;

public class RolePermissionMatrixTests
{
    [Theory]
    [InlineData(OrganizationRole.Owner, PermissionNames.OrganizationDelete, true)]
    [InlineData(OrganizationRole.Admin, PermissionNames.OrganizationDelete, false)]
    [InlineData(OrganizationRole.Admin, PermissionNames.InvitationCreate, true)]
    [InlineData(OrganizationRole.Developer, PermissionNames.MemberManage, false)]
    [InlineData(OrganizationRole.Developer, PermissionNames.PodCreate, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.OrganizationRead, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.OrganizationUpdate, false)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.PodRead, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.PodCreate, false)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.PodUpdate, false)]
    [InlineData(OrganizationRole.Developer, PermissionNames.PodUpdate, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.ProviderRead, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.ProviderCreate, false)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.ProviderUpdate, false)]
    [InlineData(OrganizationRole.Developer, PermissionNames.ProviderUpdate, true)]
    [InlineData(OrganizationRole.Admin, PermissionNames.ProviderRead, true)]
    [InlineData(OrganizationRole.Admin, PermissionNames.OrchestratorRead, true)]
    [InlineData(OrganizationRole.Admin, PermissionNames.OrchestratorManage, true)]
    [InlineData(OrganizationRole.Developer, PermissionNames.OrchestratorRead, true)]
    [InlineData(OrganizationRole.Developer, PermissionNames.OrchestratorManage, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.OrchestratorRead, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.OrchestratorManage, false)]
    [InlineData(OrganizationRole.Admin, PermissionNames.ObservabilityRead, true)]
    [InlineData(OrganizationRole.Admin, PermissionNames.ObservabilityExport, true)]
    [InlineData(OrganizationRole.Developer, PermissionNames.ObservabilityRead, true)]
    [InlineData(OrganizationRole.Developer, PermissionNames.ObservabilityExport, false)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.ObservabilityRead, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.ObservabilityExport, false)]
    [InlineData(OrganizationRole.Admin, PermissionNames.AiProviderRead, true)]
    [InlineData(OrganizationRole.Admin, PermissionNames.AiProviderCreate, true)]
    [InlineData(OrganizationRole.Developer, PermissionNames.AiProviderUpdate, true)]
    [InlineData(OrganizationRole.Developer, PermissionNames.AiProviderDelete, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.AiProviderRead, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.AiProviderCreate, false)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.AiProviderUpdate, false)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.AiProviderDelete, false)]
    [InlineData(OrganizationRole.Admin, PermissionNames.RoutingRead, true)]
    [InlineData(OrganizationRole.Admin, PermissionNames.RoutingManage, true)]
    [InlineData(OrganizationRole.Developer, PermissionNames.RoutingManage, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.RoutingRead, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.RoutingManage, false)]
    [InlineData(OrganizationRole.Admin, PermissionNames.PluginRead, true)]
    [InlineData(OrganizationRole.Admin, PermissionNames.PluginManage, true)]
    [InlineData(OrganizationRole.Admin, PermissionNames.McpRead, true)]
    [InlineData(OrganizationRole.Admin, PermissionNames.McpManage, true)]
    [InlineData(OrganizationRole.Developer, PermissionNames.PluginManage, true)]
    [InlineData(OrganizationRole.Developer, PermissionNames.McpManage, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.PluginRead, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.PluginManage, false)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.McpRead, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.McpManage, false)]
    [InlineData(OrganizationRole.Admin, PermissionNames.SecurityManage, true)]
    [InlineData(OrganizationRole.Admin, PermissionNames.SecretsManage, true)]
    [InlineData(OrganizationRole.Admin, PermissionNames.ComplianceManage, true)]
    [InlineData(OrganizationRole.Developer, PermissionNames.AuditRead, true)]
    [InlineData(OrganizationRole.Developer, PermissionNames.SecurityManage, false)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.SecurityRead, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.SecretsRead, false)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.PolicyRead, true)]
    [InlineData(OrganizationRole.Admin, PermissionNames.BillingManage, true)]
    [InlineData(OrganizationRole.Admin, PermissionNames.LicenseManage, true)]
    [InlineData(OrganizationRole.Developer, PermissionNames.BillingRead, true)]
    [InlineData(OrganizationRole.Developer, PermissionNames.BillingManage, false)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.BillingRead, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.LicenseRead, true)]
    [InlineData(OrganizationRole.Viewer, PermissionNames.BackupManage, false)]
    public void HasPermission_ReturnsExpectedResult(
        OrganizationRole role,
        string permission,
        bool expected)
    {
        var result = RolePermissionMatrix.HasPermission(role, permission);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Owner_HasAllDefinedPermissions()
    {
        foreach (var permission in PermissionNames.All)
        {
            Assert.True(RolePermissionMatrix.HasPermission(OrganizationRole.Owner, permission));
        }
    }
}
