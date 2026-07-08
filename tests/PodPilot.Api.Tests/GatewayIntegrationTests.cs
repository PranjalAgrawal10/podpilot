using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Gateway;
using PodPilot.Contracts.Pods;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Compute;
using PodPilot.Infrastructure.Gateway;
using PodPilot.Infrastructure.Services;

namespace PodPilot.Api.Tests;

public class GatewayIntegrationTests : IClassFixture<GatewayWebApplicationFactory>
{
    private readonly HttpClient client;
    private readonly GatewayWebApplicationFactory factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GatewayIntegrationTests(GatewayWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    [Fact]
    public async Task OpenAI_Models_Endpoint_Returns_Compatible_List()
    {
        var auth = await RegisterAndAuthenticateAsync("gateway-openai-user");
        SetBearerToken(auth.AccessToken);

        var providerId = await CreateProviderAsync();
        var podId = await CreatePodAsync(providerId, "http://127.0.0.1:11434");

        var keyResponse = await client.PostAsJsonAsync(
            "/api/v1/gateway/api-keys",
            new CreateGatewayApiKeyRequest { Name = "ide-key", IsPersonal = true });
        var keyBody = await keyResponse.Content.ReadAsStringAsync();
        Assert.True(keyResponse.IsSuccessStatusCode, keyBody);
        var createdKey = JsonSerializer.Deserialize<ApiResponse<GatewayApiKeyResponse>>(keyBody, JsonOptions);
        Assert.NotNull(createdKey?.Data?.PlaintextKey);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", createdKey.Data.PlaintextKey);

        var modelsResponse = await client.GetAsync("/v1/models");
        Assert.Equal(HttpStatusCode.OK, modelsResponse.StatusCode);

        var payload = await modelsResponse.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        Assert.Equal("list", document.RootElement.GetProperty("object").GetString());
        Assert.Equal("llama3:latest", document.RootElement.GetProperty("data")[0].GetProperty("id").GetString());
    }

    [Fact]
    public async Task Anthropic_Messages_Endpoint_Returns_Gateway_Error_When_Pod_Unavailable()
    {
        var auth = await RegisterAndAuthenticateAsync("gateway-anthropic-user");
        SetBearerToken(auth.AccessToken);

        var providerId = await CreateProviderAsync();
        await CreatePodAsync(providerId, "http://127.0.0.1:11434");

        var keyResponse = await client.PostAsJsonAsync(
            "/api/v1/gateway/api-keys",
            new CreateGatewayApiKeyRequest { Name = "claude-key", IsPersonal = false });
        var keyBody = await keyResponse.Content.ReadAsStringAsync();
        Assert.True(keyResponse.IsSuccessStatusCode, keyBody);
        var createdKey = JsonSerializer.Deserialize<ApiResponse<GatewayApiKeyResponse>>(keyBody, JsonOptions);
        Assert.NotNull(createdKey?.Data?.PlaintextKey);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", createdKey.Data.PlaintextKey);

        var body = new StringContent(
            JsonSerializer.Serialize(new
            {
                model = "llama3:latest",
                max_tokens = 32,
                messages = new[] { new { role = "user", content = "Hello" } },
            }),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("/v1/messages", body);
        Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);

        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        Assert.Equal("error", document.RootElement.GetProperty("type").GetString());
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
                FirstName = "Gateway",
                LastName = "Tester",
                OrganizationName = $"{prefix} Organization",
            });

        var registerContent = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions);
        Assert.NotNull(registerContent?.Data);
        return registerContent.Data;
    }

    private void SetBearerToken(string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private async Task<Guid> CreateProviderAsync()
    {
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/providers",
            new Contracts.Providers.CreateProviderRequest
            {
                Name = $"runpod-gateway-{Guid.NewGuid():N}",
                ProviderType = "RunPod",
                DisplayName = "RunPod Gateway",
                ApiKey = "rp_test_key",
            });

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Contracts.Providers.ProviderResponse>>(JsonOptions);
        Assert.NotNull(created?.Data);
        return created.Data.Id;
    }

    private async Task<Guid> CreatePodAsync(Guid providerId, string endpoint)
    {
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/pods",
            new CreatePodRequest
            {
                ProviderId = providerId,
                Name = "gateway-pod",
                GpuId = "NVIDIA GeForce RTX 4090",
                GpuType = "RTX4090",
                Region = "US",
                ImageName = "runpod/pytorch:2.1.0-py3.10-cuda11.8.0-devel-ubuntu22.04",
            });

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<PodResponse>>(JsonOptions);
        Assert.NotNull(created?.Data);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PodPilot.Infrastructure.Persistence.ApplicationDbContext>();
        var pod = await dbContext.GpuPods.FindAsync(created.Data.Id);
        Assert.NotNull(pod);
        pod.Endpoint = endpoint;
        pod.Status = PodStatus.Stopped;
        await dbContext.SaveChangesAsync();

        return created.Data.Id;
    }
}

public class GatewayWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DatabaseProvider", "InMemory");
        builder.UseSetting("InMemoryDatabaseName", $"PodPilotGatewayTest_{Guid.NewGuid()}");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IComputeProvider>();
            services.AddSingleton<IComputeProvider, TestRunPodComputeProvider>();
            services.RemoveAll<IComputeProviderFactory>();
            services.AddSingleton<IComputeProviderFactory, ComputeProviderFactory>();

            services.RemoveAll<IPodProvider>();
            var podProvider = new TestRunPodPodProvider();
            services.AddSingleton<IPodProvider>(podProvider);
            services.AddSingleton(podProvider);
            services.RemoveAll<IPodProviderFactory>();
            services.AddSingleton<IPodProviderFactory, PodProviderFactory>();

            services.RemoveAll<IGatewayNotificationService>();
            services.AddSingleton<IGatewayNotificationService, NoOpGatewayNotificationService>();

            services.RemoveAll<IInferenceClient>();
            services.AddSingleton<IInferenceClient>(new MockInferenceClient());
        });
    }

    private sealed class MockInferenceClient : IInferenceClient
    {
        public Task<bool> IsHealthyAsync(
            string baseUrl,
            CancellationToken cancellationToken = default,
            TimeSpan? requestTimeout = null) =>
            Task.FromResult(false);

        public Task<bool> WaitForHealthyAsync(
            string baseUrl,
            CancellationToken cancellationToken = default,
            int? maxAttempts = null,
            TimeSpan? checkInterval = null,
            TimeSpan? requestTimeout = null) =>
            Task.FromResult(false);

        public Task<string> GetModelsAsync(string baseUrl, CancellationToken cancellationToken = default) =>
            Task.FromResult("""
                {
                  "models": [
                    { "name": "llama3:latest" }
                  ]
                }
                """);
    }
}
