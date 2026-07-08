using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Lifecycle;
using PodPilot.Contracts.Pods;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Compute;
using PodPilot.Infrastructure.Services;

namespace PodPilot.Api.Tests;

public class PodLifecycleIntegrationTests : IClassFixture<PodLifecycleWebApplicationFactory>
{
    private readonly HttpClient client;
    private readonly PodLifecycleWebApplicationFactory factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PodLifecycleIntegrationTests(PodLifecycleWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Wake_Shutdown_And_Update_Policy_Work_End_To_End()
    {
        var auth = await RegisterAndAuthenticateAsync("lifecycle-user");
        SetBearerToken(auth.AccessToken);

        var providerId = await CreateProviderAsync();
        var podId = await CreatePodAsync(providerId);

        var policyResponse = await client.PutAsJsonAsync(
            $"/api/v1/pods/{podId}/idle-policy",
            new UpdatePodIdlePolicyRequest
            {
                IdleTimeoutMinutes = 15,
                GracePeriodMinutes = 2,
                AutoShutdownEnabled = true,
                AutoWakeEnabled = true,
                MinimumRunningTimeMinutes = 0,
            });
        Assert.Equal(HttpStatusCode.OK, policyResponse.StatusCode);

        var policy = await policyResponse.Content.ReadFromJsonAsync<ApiResponse<PodIdlePolicyResponse>>(JsonOptions);
        Assert.NotNull(policy?.Data);
        Assert.Equal(15, policy.Data.IdleTimeoutMinutes);

        var stopResponse = await client.PostAsync($"/api/v1/pods/{podId}/stop", null);
        Assert.Equal(HttpStatusCode.OK, stopResponse.StatusCode);

        var wakeResponse = await client.PostAsync($"/api/v1/pods/{podId}/wake", null);
        Assert.Equal(HttpStatusCode.OK, wakeResponse.StatusCode);

        var wake = await wakeResponse.Content.ReadFromJsonAsync<ApiResponse<PodWakeResponse>>(JsonOptions);
        Assert.NotNull(wake?.Data);
        Assert.True(wake.Data.Success);
        Assert.True(wake.Data.Queued);

        using (var scope = factory.Services.CreateScope())
        {
            var lifecycleService = scope.ServiceProvider.GetRequiredService<IPodLifecycleService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var request = await dbContext.PodWakeRequests.FirstAsync();
            var result = await lifecycleService.ProcessWakeRequestAsync(request);
            Assert.True(result.Success);
        }

        var lifecycleSummaryResponse = await client.GetAsync($"/api/v1/pods/{podId}/lifecycle");
        var lifecycle = await lifecycleSummaryResponse.Content.ReadFromJsonAsync<ApiResponse<PodLifecycleSummaryResponse>>(JsonOptions);
        Assert.NotNull(lifecycle?.Data);
        Assert.True(lifecycle.Data.AutoWakeEnabled);

        var shutdownResponse = await client.PostAsync($"/api/v1/pods/{podId}/shutdown", null);
        Assert.Equal(HttpStatusCode.OK, shutdownResponse.StatusCode);

        var shutdown = await shutdownResponse.Content.ReadFromJsonAsync<ApiResponse<PodShutdownResponse>>(JsonOptions);
        Assert.NotNull(shutdown?.Data);
        Assert.True(shutdown.Data.Success);
    }

    private async Task<Guid> CreateProviderAsync()
    {
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/providers",
            new Contracts.Providers.CreateProviderRequest
            {
                Name = "runpod-lifecycle",
                ProviderType = "RunPod",
                DisplayName = "RunPod Lifecycle",
                ApiKey = "rp_test_key",
            });

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Contracts.Providers.ProviderResponse>>(JsonOptions);
        Assert.NotNull(created?.Data);
        return created.Data.Id;
    }

    private async Task<Guid> CreatePodAsync(Guid providerId)
    {
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/pods",
            new CreatePodRequest
            {
                ProviderId = providerId,
                Name = "lifecycle-pod",
                GpuId = "NVIDIA GeForce RTX 4090",
                GpuType = "RTX4090",
                Region = "US",
                ImageName = "runpod/pytorch:2.1.0-py3.10-cuda11.8.0-devel-ubuntu22.04",
                ContainerDiskGb = 50,
                VolumeDiskGb = 20,
            });

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<PodResponse>>(JsonOptions);
        Assert.NotNull(created?.Data);
        return created.Data.Id;
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

        var registerContent = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions);
        Assert.NotNull(registerContent?.Data);
        return registerContent.Data;
    }

    private void SetBearerToken(string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}

public sealed class PodLifecycleWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DatabaseProvider", "InMemory");
        builder.UseSetting("InMemoryDatabaseName", $"PodPilotLifecycleTest_{Guid.NewGuid()}");

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
        });
    }
}
