using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Security;
using PodPilot.Infrastructure.Security;

namespace PodPilot.Api.Tests;

public class SecurityIntegrationTests : IClassFixture<SecurityWebApplicationFactory>
{
    private readonly HttpClient client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SecurityIntegrationTests(SecurityWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Secrets_Policies_Audit_And_Mfa_Flow_Work_End_To_End()
    {
        var auth = await RegisterAndAuthenticateAsync("security");
        SetBearerToken(auth.AccessToken);

        var createSecret = await client.PostAsJsonAsync(
            "/api/v1/secrets",
            new CreateSecretRequest
            {
                Name = "gateway-jwt",
                SecretKind = "JwtSigningKey",
                BackendKind = "LocalEncrypted",
                Value = "super-secret-value",
            });
        Assert.Equal(HttpStatusCode.Created, createSecret.StatusCode);
        var secretBody = await createSecret.Content.ReadAsStringAsync();
        Assert.DoesNotContain("super-secret-value", secretBody, StringComparison.OrdinalIgnoreCase);

        var listSecrets = await client.GetAsync("/api/v1/secrets");
        Assert.Equal(HttpStatusCode.OK, listSecrets.StatusCode);

        var policies = await client.GetAsync("/api/v1/policies");
        Assert.Equal(HttpStatusCode.OK, policies.StatusCode);

        var updatePolicies = await client.PutAsJsonAsync(
            "/api/v1/policies",
            new UpdatePoliciesRequest
            {
                Governance = new GovernancePolicyResponse
                {
                    AllowedProviders = ["OpenAi"],
                    AllowedModels = [],
                    AllowedPlugins = [],
                    AllowedMcpServers = [],
                    EmptyAllowListMeansAllowAll = true,
                },
            });
        Assert.Equal(HttpStatusCode.OK, updatePolicies.StatusCode);

        var audit = await client.GetAsync("/api/v1/audit");
        Assert.Equal(HttpStatusCode.OK, audit.StatusCode);

        var compliance = await client.GetAsync("/api/v1/compliance");
        Assert.Equal(HttpStatusCode.OK, compliance.StatusCode);

        var dashboard = await client.GetAsync("/api/v1/security/dashboard");
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);

        var enroll = await client.PostAsJsonAsync(
            "/api/v1/auth/mfa",
            new MfaRequest { Action = "enroll" });
        Assert.Equal(HttpStatusCode.OK, enroll.StatusCode);
        var enrollment = await enroll.Content.ReadFromJsonAsync<ApiResponse<MfaEnrollmentResponse>>(JsonOptions);
        Assert.NotNull(enrollment?.Data?.SharedSecret);

        var code = TotpService.GenerateCode(enrollment.Data.SharedSecret);
        var confirm = await client.PostAsJsonAsync(
            "/api/v1/auth/mfa",
            new MfaRequest { Action = "confirm", Code = code });
        Assert.True(
            confirm.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent,
            $"Unexpected MFA confirm status: {confirm.StatusCode}");

        var createIdp = await client.PostAsJsonAsync(
            "/api/v1/security/identity-providers",
            new CreateIdentityProviderRequest
            {
                Name = "entra-test",
                ProviderKind = "EntraId",
                Protocol = "Oidc",
                ClientId = "client",
                ClientSecret = "idp-client-secret-value",
                Issuer = "https://login.microsoftonline.com/common/v2.0",
                AuthorizationEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize",
                TokenEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/token",
            });
        Assert.Equal(HttpStatusCode.Created, createIdp.StatusCode);
        var idpBody = await createIdp.Content.ReadAsStringAsync();
        Assert.DoesNotContain("idp-client-secret-value", idpBody, StringComparison.Ordinal);
    }

    private async Task<AuthResponse> RegisterAndAuthenticateAsync(string prefix)
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@podpilot.test";
        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest
            {
                Email = email,
                Password = "SecureP@ss1",
                FirstName = "Test",
                LastName = "User",
                OrganizationName = $"{prefix} Organization",
            });

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

public sealed class SecurityWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DatabaseProvider", "InMemory");
        builder.UseSetting("InMemoryDatabaseName", $"PodPilotSecurityTest_{Guid.NewGuid()}");
    }
}
