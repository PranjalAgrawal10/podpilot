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
