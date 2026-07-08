using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Invitations;
using PodPilot.Contracts.Members;
using PodPilot.Contracts.Organizations;

namespace PodPilot.Api.Tests;

public class OrganizationEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public OrganizationEndpointTests(CustomWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrganization_ListMembers_And_SwitchOrganization_Work_End_To_End()
    {
        var auth = await RegisterAndAuthenticateAsync("owner");
        SetBearerToken(auth.AccessToken);

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/organizations",
            new CreateOrganizationRequest
            {
                Name = "Second Organization",
                Description = "Additional workspace",
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<OrganizationResponse>>(JsonOptions);
        Assert.NotNull(created?.Data);
        Assert.Equal("Second Organization", created.Data.Name);
        Assert.Equal("Owner", created.Data.CurrentUserRole);

        var listResponse = await client.GetAsync("/api/v1/organizations");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var organizations = await listResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<OrganizationResponse>>>(JsonOptions);
        Assert.NotNull(organizations?.Data);
        Assert.True(organizations.Data.Count >= 2);

        var membersResponse = await client.GetAsync($"/api/v1/organizations/{created.Data.Id}/members");
        Assert.Equal(HttpStatusCode.OK, membersResponse.StatusCode);

        var members = await membersResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<MemberResponse>>>(JsonOptions);
        Assert.NotNull(members?.Data);
        Assert.Contains(members.Data, m => m.Role == "Owner");

        var switchResponse = await client.PostAsJsonAsync(
            "/api/v1/organizations/switch",
            new SwitchOrganizationRequest { OrganizationId = created.Data.Id });

        Assert.Equal(HttpStatusCode.OK, switchResponse.StatusCode);

        var switched = await switchResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions);
        Assert.NotNull(switched?.Data);
        Assert.Equal(created.Data.Id, switched.Data.User.CurrentOrganizationId);
        Assert.Equal("Owner", switched.Data.User.CurrentOrganizationRole);
    }

    [Fact]
    public async Task Invite_And_AcceptInvitation_AddsMember()
    {
        var ownerAuth = await RegisterAndAuthenticateAsync("owner");
        SetBearerToken(ownerAuth.AccessToken);

        var organizations = await client.GetFromJsonAsync<ApiResponse<IReadOnlyList<OrganizationResponse>>>(
            "/api/v1/organizations",
            JsonOptions);

        var organizationId = organizations!.Data!.First(o => o.IsDefault).Id;

        var inviteeEmail = $"invitee_{Guid.NewGuid():N}@podpilot.test";
        var inviteResponse = await client.PostAsJsonAsync(
            $"/api/v1/organizations/{organizationId}/invite",
            new InviteMemberRequest
            {
                Email = inviteeEmail,
                Role = "Developer",
            });

        Assert.Equal(HttpStatusCode.Created, inviteResponse.StatusCode);

        var invitation = await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InvitationResponse>>(JsonOptions);
        Assert.NotNull(invitation?.Data);
        Assert.False(string.IsNullOrWhiteSpace(invitation.Data.Token));

        var inviteeAuth = await RegisterAndAuthenticateAsync("invitee", inviteeEmail);
        SetBearerToken(inviteeAuth.AccessToken);

        var acceptResponse = await client.PostAsJsonAsync(
            "/api/v1/organizations/accept",
            new AcceptInvitationRequest { Token = invitation.Data.Token });

        Assert.Equal(HttpStatusCode.OK, acceptResponse.StatusCode);

        var member = await acceptResponse.Content.ReadFromJsonAsync<ApiResponse<MemberResponse>>(JsonOptions);
        Assert.NotNull(member?.Data);
        Assert.Equal("Developer", member.Data.Role);
        Assert.Equal(inviteeEmail, member.Data.Email);
    }

    [Fact]
    public async Task UpdateMemberRole_ChangesRole_ForOwner()
    {
        var ownerAuth = await RegisterAndAuthenticateAsync("owner");
        SetBearerToken(ownerAuth.AccessToken);

        var organizations = await client.GetFromJsonAsync<ApiResponse<IReadOnlyList<OrganizationResponse>>>(
            "/api/v1/organizations",
            JsonOptions);

        var organizationId = organizations!.Data!.First().Id;

        var inviteeEmail = $"member_{Guid.NewGuid():N}@podpilot.test";
        var inviteResponse = await client.PostAsJsonAsync(
            $"/api/v1/organizations/{organizationId}/invite",
            new InviteMemberRequest { Email = inviteeEmail, Role = "Viewer" });

        var invitation = await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InvitationResponse>>(JsonOptions);

        var inviteeAuth = await RegisterAndAuthenticateAsync("member", inviteeEmail);
        SetBearerToken(inviteeAuth.AccessToken);

        await client.PostAsJsonAsync(
            "/api/v1/organizations/accept",
            new AcceptInvitationRequest { Token = invitation!.Data!.Token });

        SetBearerToken(ownerAuth.AccessToken);

        var members = await client.GetFromJsonAsync<ApiResponse<IReadOnlyList<MemberResponse>>>(
            $"/api/v1/organizations/{organizationId}/members",
            JsonOptions);

        var targetMember = members!.Data!.First(m => m.Email == inviteeEmail);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/v1/organizations/{organizationId}/members/{targetMember.Id}/role",
            new UpdateMemberRoleRequest { Role = "Developer" });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<MemberResponse>>(JsonOptions);
        Assert.NotNull(updated?.Data);
        Assert.Equal("Developer", updated.Data.Role);
    }

    private async Task<AuthResponse> RegisterAndAuthenticateAsync(string prefix, string? email = null)
    {
        email ??= $"{prefix}_{Guid.NewGuid():N}@podpilot.test";

        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = "SecureP@ss1",
            FirstName = "Test",
            LastName = "User",
            OrganizationName = $"{prefix} Organization",
        };

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var registerContent = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions);
        Assert.NotNull(registerContent?.Data);
        return registerContent.Data;
    }

    private void SetBearerToken(string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}
