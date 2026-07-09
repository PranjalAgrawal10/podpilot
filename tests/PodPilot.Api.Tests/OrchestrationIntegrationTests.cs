using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Orchestration;
using PodPilot.Contracts.Pods;
using PodPilot.Contracts.Providers;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Compute;
using PodPilot.Infrastructure.Services;

namespace PodPilot.Api.Tests;

public class OrchestrationIntegrationTests : IClassFixture<OrchestrationWebApplicationFactory>
{
    private readonly HttpClient client;
    private readonly OrchestrationWebApplicationFactory factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public OrchestrationIntegrationTests(OrchestrationWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Pool_Get_Status_And_Scale_Endpoints_Work_End_To_End()
    {
        var auth = await RegisterAndAuthenticateAsync("orchestrator-owner");
        SetBearerToken(auth.AccessToken);

        var providerId = await CreateProviderAsync();
        var firstPodId = await CreatePodAsync(providerId, "pool-pod-1");
        var secondPodId = await CreatePodAsync(providerId, "pool-pod-2");

        var createPoolResponse = await client.PostAsJsonAsync(
            "/api/v1/pod-pools",
            new CreatePodPoolRequest
            {
                Name = "integration-pool",
                Description = "Pool for orchestration integration tests",
                PoolType = "Custom",
                IsDefault = true,
                ProviderId = providerId,
                GpuId = "NVIDIA GeForce RTX 4090",
                GpuType = "RTX4090",
                Region = "US",
                ImageName = "runpod/pytorch:2.1.0-py3.10-cuda11.8.0-devel-ubuntu22.04",
                Models = ["llama3:latest"],
                PodIds = [firstPodId, secondPodId],
                ScalingPolicy = new ScalingPolicyRequest
                {
                    Name = "integration-policy",
                    MinPods = 1,
                    MaxPods = 5,
                    AutoScaleUpEnabled = true,
                    AutoScaleDownEnabled = true,
                },
            });

        Assert.Equal(HttpStatusCode.Created, createPoolResponse.StatusCode);

        var createdPool = await createPoolResponse.Content.ReadFromJsonAsync<ApiResponse<PodPoolResponse>>(JsonOptions);
        Assert.NotNull(createdPool?.Data);
        Assert.Equal("integration-pool", createdPool.Data.Name);
        Assert.Equal(2, createdPool.Data.Members.Count);

        var statusResponse = await client.GetAsync("/api/v1/orchestrator");
        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);

        var status = await statusResponse.Content.ReadFromJsonAsync<ApiResponse<OrchestratorStatusResponse>>(JsonOptions);
        Assert.NotNull(status?.Data);
        Assert.True(status.Data.PoolCount >= 1);
        Assert.True(status.Data.HealthyPods >= 2);
        Assert.Equal(0, status.Data.QueueLength);

        var scaleUpResponse = await client.PostAsJsonAsync(
            "/api/v1/autoscaler/scale-up",
            new ManualScaleRequest
            {
                PoolId = createdPool.Data.Id,
                Reason = "Integration test scale-up",
            });

        Assert.Equal(HttpStatusCode.OK, scaleUpResponse.StatusCode);

        var scaleUpResult = await scaleUpResponse.Content.ReadFromJsonAsync<ApiResponse<ScalingActionResult>>(JsonOptions);
        Assert.NotNull(scaleUpResult?.Data);
        Assert.Equal(ScalingDirection.ScaleUp, scaleUpResult.Data.Direction);
        Assert.True(scaleUpResult.Data.Success);
        Assert.NotNull(scaleUpResult.Data.PodId);

        var scaleDownResponse = await client.PostAsJsonAsync(
            "/api/v1/autoscaler/scale-down",
            new ManualScaleRequest
            {
                PoolId = createdPool.Data.Id,
                Reason = "Integration test scale-down",
            });

        Assert.Equal(HttpStatusCode.OK, scaleDownResponse.StatusCode);

        var scaleDownResult = await scaleDownResponse.Content.ReadFromJsonAsync<ApiResponse<ScalingActionResult>>(JsonOptions);
        Assert.NotNull(scaleDownResult?.Data);
        Assert.Equal(ScalingDirection.ScaleDown, scaleDownResult.Data.Direction);
        Assert.True(scaleDownResult.Data.Success);
    }

    private async Task<Guid> CreateProviderAsync()
    {
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/providers",
            new CreateProviderRequest
            {
                Name = "orchestrator-provider",
                ProviderType = "RunPod",
                DisplayName = "Orchestrator RunPod",
                ApiKey = "rp_test_key",
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProviderResponse>>(JsonOptions);
        Assert.NotNull(created?.Data);
        return created.Data.Id;
    }

    private async Task<Guid> CreatePodAsync(Guid providerId, string name)
    {
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/pods",
            new CreatePodRequest
            {
                ProviderId = providerId,
                Name = name,
                GpuId = "NVIDIA GeForce RTX 4090",
                GpuType = "RTX4090",
                Region = "US",
                ImageName = "runpod/pytorch:2.1.0-py3.10-cuda11.8.0-devel-ubuntu22.04",
                ContainerDiskGb = 50,
                VolumeDiskGb = 20,
                EnablePublicIp = true,
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<PodResponse>>(JsonOptions);
        Assert.NotNull(created?.Data);
        return created.Data.Id;
    }

    private async Task<AuthResponse> RegisterAndAuthenticateAsync(string prefix)
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@podpilot.test";
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

public sealed class OrchestrationWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DatabaseProvider", "InMemory");
        builder.UseSetting("InMemoryDatabaseName", $"PodPilotOrchestrationTest_{Guid.NewGuid()}");

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

            services.RemoveAll<IPodNotificationService>();
            services.AddSingleton<IPodNotificationService, NoOpPodNotificationService>();

            services.RemoveAll<IInferenceClient>();
            services.AddSingleton<IInferenceClient, HealthyInferenceClient>();
        });
    }

    private sealed class HealthyInferenceClient : IInferenceClient
    {
        public Task<bool> IsHealthyAsync(
            string baseUrl,
            CancellationToken cancellationToken = default,
            TimeSpan? requestTimeout = null) =>
            Task.FromResult(true);

        public Task<bool> WaitForHealthyAsync(
            string baseUrl,
            CancellationToken cancellationToken = default,
            int? maxAttempts = null,
            TimeSpan? checkInterval = null,
            TimeSpan? requestTimeout = null) =>
            Task.FromResult(true);

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
