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
