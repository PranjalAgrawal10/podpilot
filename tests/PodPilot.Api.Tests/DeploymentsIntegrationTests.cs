using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Deployments;

namespace PodPilot.Api.Tests;

public class DeploymentsIntegrationTests : IClassFixture<DeploymentsWebApplicationFactory>
{
    private readonly HttpClient client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public DeploymentsIntegrationTests(DeploymentsWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Catalog_And_Deployments_Endpoints_Work()
    {
        var auth = await RegisterAndAuthenticateAsync("deploy");
        SetBearerToken(auth.AccessToken);

        var gpus = await client.GetAsync("/api/v1/gpus");
        Assert.Equal(HttpStatusCode.OK, gpus.StatusCode);
        var gpuPayload = await gpus.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<GpuCatalogResponse>>>(JsonOptions);
        Assert.NotNull(gpuPayload?.Data);
        Assert.Contains(gpuPayload.Data, g => g.Code == "RTX4090");

        var models = await client.GetAsync("/api/v1/models/catalog");
        Assert.Equal(HttpStatusCode.OK, models.StatusCode);

        var templates = await client.GetAsync("/api/v1/templates");
        Assert.Equal(HttpStatusCode.OK, templates.StatusCode);

        var recommend = await client.PostAsJsonAsync(
            "/api/v1/gpus/recommend",
            new RecommendGpuRequest { Models = ["qwen-coder-7b"] });
        Assert.Equal(HttpStatusCode.OK, recommend.StatusCode);
        var recommendation = await recommend.Content.ReadFromJsonAsync<ApiResponse<GpuRecommendationResponse>>(JsonOptions);
        Assert.Equal("RTX4090", recommendation?.Data?.RecommendedGpuCode);

        var list = await client.GetAsync("/api/v1/deployments");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);

        var dashboard = await client.GetAsync("/api/v1/deployments/dashboard");
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
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

public sealed class DeploymentsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DatabaseProvider", "InMemory");
        builder.UseSetting("InMemoryDatabaseName", $"PodPilotDeployTest_{Guid.NewGuid()}");
    }
}
