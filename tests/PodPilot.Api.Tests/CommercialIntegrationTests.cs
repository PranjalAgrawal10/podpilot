using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Commercial;
using PodPilot.Contracts.Common;

namespace PodPilot.Api.Tests;

public class CommercialIntegrationTests : IClassFixture<CommercialWebApplicationFactory>
{
    private readonly HttpClient client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CommercialIntegrationTests(CommercialWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Billing_Onboarding_And_License_Flow_Work()
    {
        var auth = await RegisterAndAuthenticateAsync("commercial");
        SetBearerToken(auth.AccessToken);

        var plans = await client.GetAsync("/api/v1/billing/plans");
        Assert.Equal(HttpStatusCode.OK, plans.StatusCode);
        var planPayload = await plans.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<PlanResponse>>>(JsonOptions);
        Assert.NotNull(planPayload?.Data);
        Assert.Contains(planPayload.Data, p => p.Code == "pro");

        var subscription = await client.GetAsync("/api/v1/billing/subscription");
        Assert.Equal(HttpStatusCode.OK, subscription.StatusCode);

        var usage = await client.GetAsync("/api/v1/billing/usage");
        Assert.Equal(HttpStatusCode.OK, usage.StatusCode);

        var dashboard = await client.GetAsync("/api/v1/commercial/dashboard");
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);

        var onboarding = await client.GetAsync("/api/v1/onboarding");
        Assert.Equal(HttpStatusCode.OK, onboarding.StatusCode);

        var completeStep = await client.PostAsJsonAsync(
            "/api/v1/onboarding/steps/complete",
            new CompleteOnboardingStepRequest { Step = "CreateOrganization" });
        Assert.Equal(HttpStatusCode.OK, completeStep.StatusCode);

        var issue = await client.PostAsJsonAsync(
            "/api/v1/licenses/issue",
            new IssueLicenseRequest
            {
                Edition = "Professional",
                DeploymentMode = "Online",
                MaxSeats = 5,
            });
        Assert.True(
            issue.StatusCode is HttpStatusCode.OK or HttpStatusCode.Created,
            $"Unexpected issue status {issue.StatusCode}");
        var issued = await issue.Content.ReadFromJsonAsync<ApiResponse<IssuedLicenseResponse>>(JsonOptions);
        Assert.NotNull(issued?.Data?.LicenseKey);

        var activate = await client.PostAsJsonAsync(
            "/api/v1/licenses/activate",
            new ActivateLicenseRequest { LicenseKey = issued.Data.LicenseKey });
        Assert.Equal(HttpStatusCode.OK, activate.StatusCode);

        var status = await client.GetAsync("/api/v1/status");
        Assert.Equal(HttpStatusCode.OK, status.StatusCode);

        var release = await client.GetAsync("/api/v1/releases/status");
        Assert.Equal(HttpStatusCode.OK, release.StatusCode);
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

public sealed class CommercialWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DatabaseProvider", "InMemory");
        builder.UseSetting("InMemoryDatabaseName", $"PodPilotCommercialTest_{Guid.NewGuid()}");
    }
}
