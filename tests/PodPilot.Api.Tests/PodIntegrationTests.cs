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
using PodPilot.Application.Models.Pods;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Pods;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Compute;
using PodPilot.Infrastructure.Services;

namespace PodPilot.Api.Tests;

public class PodIntegrationTests : IClassFixture<PodWebApplicationFactory>
{
    private readonly HttpClient client;
    private readonly PodWebApplicationFactory factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PodIntegrationTests(PodWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Start_Stop_And_Delete_Pod_Work_End_To_End()
    {
        var auth = await RegisterAndAuthenticateAsync("pod-owner");
        SetBearerToken(auth.AccessToken);

        var providerId = await CreateProviderAsync();

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/pods",
            new CreatePodRequest
            {
                ProviderId = providerId,
                Name = "training-pod",
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
        Assert.Equal("training-pod", created.Data.Name);
        Assert.Equal("Running", created.Data.Status);

        var startResponse = await client.PostAsync($"/api/v1/pods/{created.Data.Id}/start", null);
        Assert.Equal(HttpStatusCode.OK, startResponse.StatusCode);

        var stopResponse = await client.PostAsync($"/api/v1/pods/{created.Data.Id}/stop", null);
        Assert.Equal(HttpStatusCode.OK, stopResponse.StatusCode);

        var stopped = await stopResponse.Content.ReadFromJsonAsync<ApiResponse<PodResponse>>(JsonOptions);
        Assert.NotNull(stopped?.Data);
        Assert.Equal("Stopped", stopped.Data.Status);

        var deleteResponse = await client.DeleteAsJsonAsync(
            $"/api/v1/pods/{created.Data.Id}",
            new DeletePodRequest { Force = true });
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await client.GetAsync("/api/v1/pods");
        var pods = await listResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<PodResponse>>>(JsonOptions);
        Assert.NotNull(pods?.Data);
        Assert.DoesNotContain(pods.Data, p => p.Id == created.Data.Id);
    }

    [Fact]
    public async Task StartPod_Replaces_Provider_Instance_When_Start_Fails()
    {
        var auth = await RegisterAndAuthenticateAsync("pod-recovery");
        SetBearerToken(auth.AccessToken);

        var providerId = await CreateProviderAsync();
        var podProvider = factory.Services.GetRequiredService<TestRunPodPodProvider>();

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/pods",
            new CreatePodRequest
            {
                ProviderId = providerId,
                Name = "recovery-pod",
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

        var originalProviderPodId = created.Data.ProviderPodId;
        Assert.False(string.IsNullOrWhiteSpace(originalProviderPodId));

        var stopResponse = await client.PostAsync($"/api/v1/pods/{created.Data.Id}/stop", null);
        Assert.Equal(HttpStatusCode.OK, stopResponse.StatusCode);

        podProvider.StartFailureProviderPodIds.Add(originalProviderPodId!);

        var startResponse = await client.PostAsync($"/api/v1/pods/{created.Data.Id}/start", null);
        Assert.Equal(HttpStatusCode.OK, startResponse.StatusCode);

        var started = await startResponse.Content.ReadFromJsonAsync<ApiResponse<PodResponse>>(JsonOptions);
        Assert.NotNull(started?.Data);
        Assert.Equal(created.Data.Id, started.Data.Id);
        Assert.Equal("Running", started.Data.Status);
        Assert.False(string.IsNullOrWhiteSpace(started.Data.ProviderPodId));
        Assert.NotEqual(originalProviderPodId, started.Data.ProviderPodId);
        Assert.DoesNotContain(started.Data.ProviderPodId!, podProvider.StartFailureProviderPodIds);

        var providerPods = await podProvider.ListPodsAsync("test", CancellationToken.None);
        Assert.DoesNotContain(providerPods, p => p.ProviderPodId == originalProviderPodId);
    }

    [Fact]
    public async Task ListPods_Imports_PreExisting_Stopped_Pods_From_Provider()
    {
        var auth = await RegisterAndAuthenticateAsync("pod-import");
        SetBearerToken(auth.AccessToken);

        var providerId = await CreateProviderAsync();

        var podProvider = factory.Services.GetRequiredService<TestRunPodPodProvider>();
        podProvider.SeedPod(new PodInfo
        {
            ProviderPodId = "existing-pod-1",
            Name = "legacy-training-pod",
            Status = PodStatus.Stopped,
            GpuId = "NVIDIA GeForce RTX 4090",
            GpuType = GpuType.RTX4090,
            Region = "US",
            ImageName = "runpod/pytorch:2.1.0-py3.10-cuda11.8.0-devel-ubuntu22.04",
            ContainerDiskGb = 50,
            VolumeDiskGb = 20,
            HourlyCost = 0.74m,
            LastStoppedAt = DateTime.UtcNow,
        });

        var listResponse = await client.GetAsync("/api/v1/pods");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var pods = await listResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<PodResponse>>>(JsonOptions);
        Assert.NotNull(pods?.Data);
        Assert.Contains(pods.Data, p => p.Name == "legacy-training-pod" && p.Status == "Stopped");
        Assert.Contains(pods.Data, p => p.ProviderPodId == "existing-pod-1");
    }

    private async Task<Guid> CreateProviderAsync()
    {
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/providers",
            new Contracts.Providers.CreateProviderRequest
            {
                Name = "runpod-pods",
                ProviderType = "RunPod",
                DisplayName = "RunPod Pods",
                ApiKey = "rp_test_key",
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Contracts.Providers.ProviderResponse>>(JsonOptions);
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

public sealed class PodWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DatabaseProvider", "InMemory");
        builder.UseSetting("InMemoryDatabaseName", $"PodPilotPodTest_{Guid.NewGuid()}");

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

internal sealed class TestRunPodPodProvider : IPodProvider
{
    private readonly Dictionary<string, PodInfo> pods = new();

    public ProviderType ProviderType => ProviderType.RunPod;

    public HashSet<string> StartFailureProviderPodIds { get; } = new(StringComparer.Ordinal);

    public void SeedPod(PodInfo info) => pods[info.ProviderPodId] = info;

    public Task<PodInfo> CreatePodAsync(string apiKey, PodCreateOptions options, CancellationToken cancellationToken = default)
    {
        var id = $"pod-{Guid.NewGuid():N}"[..12];
        var info = new PodInfo
        {
            ProviderPodId = id,
            Name = options.Name,
            Status = PodStatus.Running,
            GpuId = options.GpuId,
            GpuType = options.GpuType,
            Region = options.Region,
            TemplateId = options.TemplateId,
            ImageName = options.ImageName,
            ContainerDiskGb = options.ContainerDiskGb,
            VolumeDiskGb = options.VolumeDiskGb,
            HourlyCost = 0.74m,
            LastStartedAt = DateTime.UtcNow,
            Endpoint = "http://100.65.0.119:8888",
            PublicIp = "100.65.0.119",
            Endpoints =
            [
                new PodEndpointInfo { Port = 8888, Protocol = "http", PublicPort = 8888, Url = "http://100.65.0.119:8888" },
            ],
        };

        pods[id] = info;
        return Task.FromResult(info);
    }

    public Task<PodOperationResult> DeletePodAsync(string apiKey, string providerPodId, CancellationToken cancellationToken = default)
    {
        pods.Remove(providerPodId);
        return Task.FromResult(new PodOperationResult { Success = true, Status = PodStatus.Deleted });
    }

    public Task<PodOperationResult> StartPodAsync(string apiKey, string providerPodId, CancellationToken cancellationToken = default)
    {
        if (StartFailureProviderPodIds.Contains(providerPodId))
        {
            return Task.FromResult(new PodOperationResult
            {
                Success = false,
                Status = PodStatus.Failed,
                ErrorMessage = "Simulated provider start failure.",
            });
        }

        return UpdateStatus(providerPodId, PodStatus.Running);
    }

    public Task<PodOperationResult> StopPodAsync(string apiKey, string providerPodId, CancellationToken cancellationToken = default) =>
        UpdateStatus(providerPodId, PodStatus.Stopped);

    public Task<PodOperationResult> RestartPodAsync(string apiKey, string providerPodId, CancellationToken cancellationToken = default) =>
        UpdateStatus(providerPodId, PodStatus.Running);

    public Task<PodInfo> GetPodAsync(string apiKey, string providerPodId, CancellationToken cancellationToken = default) =>
        Task.FromResult(pods[providerPodId]);

    public Task<IReadOnlyList<PodInfo>> ListPodsAsync(string apiKey, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PodInfo>>(pods.Values.ToList());

    public Task<PodInfo> SyncPodStatusAsync(string apiKey, string providerPodId, CancellationToken cancellationToken = default) =>
        GetPodAsync(apiKey, providerPodId, cancellationToken);

    private Task<PodOperationResult> UpdateStatus(string providerPodId, PodStatus status)
    {
        var pod = pods[providerPodId];
        var updated = new PodInfo
        {
            ProviderPodId = pod.ProviderPodId,
            Name = pod.Name,
            Status = status,
            GpuId = pod.GpuId,
            GpuType = pod.GpuType,
            Region = pod.Region,
            ImageName = pod.ImageName,
            HourlyCost = pod.HourlyCost,
            Endpoint = pod.Endpoint,
            PublicIp = pod.PublicIp,
            Endpoints = pod.Endpoints,
            LastStartedAt = status == PodStatus.Running ? DateTime.UtcNow : pod.LastStartedAt,
            LastStoppedAt = status == PodStatus.Stopped ? DateTime.UtcNow : pod.LastStoppedAt,
        };

        pods[providerPodId] = updated;
        return Task.FromResult(new PodOperationResult
        {
            Success = true,
            Status = status,
            Pod = updated,
        });
    }
}

internal static class HttpClientJsonExtensions
{
    public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, requestUri)
        {
            Content = JsonContent.Create(value),
        };
        return client.SendAsync(request);
    }
}
