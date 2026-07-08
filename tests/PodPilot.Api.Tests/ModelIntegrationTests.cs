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
using PodPilot.Application.Models.Ollama;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Models;
using PodPilot.Contracts.Pods;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Compute;
using PodPilot.Infrastructure.Gateway;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Services;

namespace PodPilot.Api.Tests;

public class ModelIntegrationTests : IClassFixture<ModelWebApplicationFactory>
{
    private readonly HttpClient client;
    private readonly ModelWebApplicationFactory factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ModelIntegrationTests(ModelWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    [Fact]
    public async Task RefreshModels_SyncsFromOllama()
    {
        var auth = await RegisterAndAuthenticateAsync("model-refresh");
        SetBearerToken(auth.AccessToken);

        var providerId = await CreateProviderAsync();
        var podId = await CreateRunningPodAsync(providerId, "http://127.0.0.1:11434");

        var response = await client.PostAsJsonAsync("/api/v1/models/refresh", new RefreshModelsRequest { PodId = podId });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ModelResponse>>>(JsonOptions);
        Assert.NotNull(body?.Data);
        Assert.Contains(body.Data, m => m.Name == "llama3");
    }

    [Fact]
    public async Task PullModel_StartsDownload()
    {
        var auth = await RegisterAndAuthenticateAsync("model-pull");
        SetBearerToken(auth.AccessToken);

        var providerId = await CreateProviderAsync();
        var podId = await CreateRunningPodAsync(providerId, "http://127.0.0.1:11434");

        var response = await client.PostAsJsonAsync(
            "/api/v1/models/pull",
            new PullModelRequest { PodId = podId, Model = "mistral:latest" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ModelDownloadResponse>>(JsonOptions);
        Assert.NotNull(body?.Data);
        Assert.Equal("Queued", body.Data.Status);
    }

    [Fact]
    public async Task PullModel_RejectsDuplicateInstalledModel()
    {
        var auth = await RegisterAndAuthenticateAsync("model-dup");
        SetBearerToken(auth.AccessToken);

        var providerId = await CreateProviderAsync();
        var podId = await CreateRunningPodAsync(providerId, "http://127.0.0.1:11434");

        await client.PostAsJsonAsync("/api/v1/models/refresh", new RefreshModelsRequest { PodId = podId });

        var response = await client.PostAsJsonAsync(
            "/api/v1/models/pull",
            new PullModelRequest { PodId = podId, Model = "llama3:latest" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsRecordsAfterRefresh()
    {
        var auth = await RegisterAndAuthenticateAsync("model-health");
        SetBearerToken(auth.AccessToken);

        var providerId = await CreateProviderAsync();
        var podId = await CreateRunningPodAsync(providerId, "http://127.0.0.1:11434");
        await client.PostAsJsonAsync("/api/v1/models/refresh", new RefreshModelsRequest { PodId = podId });

        using var scope = factory.Services.CreateScope();
        var healthService = scope.ServiceProvider.GetRequiredService<IModelHealthService>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var model = db.AiModels.First(m => m.PodId == podId);
        var pod = db.GpuPods.First(p => p.Id == podId);
        await healthService.CheckModelHealthAsync(model, pod, CancellationToken.None);

        var response = await client.GetAsync("/api/v1/models/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ModelHealthResponse>>>(JsonOptions);
        Assert.NotNull(body?.Data);
        Assert.NotEmpty(body.Data);
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
                FirstName = "Model",
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
                Name = $"runpod-model-{Guid.NewGuid():N}",
                ProviderType = "RunPod",
                DisplayName = "RunPod Models",
                ApiKey = "rp_test_key",
            });

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Contracts.Providers.ProviderResponse>>(JsonOptions);
        Assert.NotNull(created?.Data);
        return created.Data.Id;
    }

    private async Task<Guid> CreateRunningPodAsync(Guid providerId, string endpoint)
    {
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/pods",
            new CreatePodRequest
            {
                ProviderId = providerId,
                Name = "model-pod",
                GpuId = "NVIDIA GeForce RTX 4090",
                GpuType = "RTX4090",
                Region = "US",
                ImageName = "runpod/pytorch:2.1.0-py3.10-cuda11.8.0-devel-ubuntu22.04",
            });

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<PodResponse>>(JsonOptions);
        Assert.NotNull(created?.Data);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var pod = await dbContext.GpuPods.FindAsync(created.Data.Id);
        Assert.NotNull(pod);
        pod.Endpoint = endpoint;
        pod.Status = PodStatus.Running;
        await dbContext.SaveChangesAsync();

        return created.Data.Id;
    }
}

public class ModelWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DatabaseProvider", "InMemory");
        builder.UseSetting("InMemoryDatabaseName", $"PodPilotModelTest_{Guid.NewGuid()}");

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

            services.RemoveAll<IModelNotificationService>();
            services.AddSingleton<IModelNotificationService, NoOpModelNotificationService>();

            services.RemoveAll<IInferenceClient>();
            services.AddSingleton<IInferenceClient>(new HealthyInferenceClient());

            services.RemoveAll<IOllamaClient>();
            services.AddSingleton<IOllamaClient>(new MockOllamaClient());
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
            Task.FromResult("""{"models":[{"name":"llama3:latest"}]}""");
    }

    private sealed class MockOllamaClient : IOllamaClient
    {
        public Task<OllamaVersionResult> GetVersionAsync(string baseUrl, CancellationToken cancellationToken = default) =>
            Task.FromResult(new OllamaVersionResult { Version = "0.5.7" });

        public Task<IReadOnlyList<OllamaModelTag>> ListModelsAsync(string baseUrl, CancellationToken cancellationToken = default) =>
            GetTagsAsync(baseUrl, cancellationToken);

        public Task<IReadOnlyList<OllamaModelTag>> GetTagsAsync(string baseUrl, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<OllamaModelTag>>(
            [
                new OllamaModelTag { Name = "llama3:latest", Size = 4_000_000_000 },
            ]);

        public Task<OllamaModelDetails> ShowModelAsync(string baseUrl, string modelName, CancellationToken cancellationToken = default) =>
            Task.FromResult(new OllamaModelDetails
            {
                Name = modelName,
                Size = 4_000_000_000,
                Family = "llama",
                Parameters = "7B",
                Quantization = "Q4_0",
                ContextLength = 8192,
                License = "LLAMA",
            });

        public async Task PullModelAsync(
            string baseUrl,
            string modelName,
            Func<OllamaPullProgress, Task> onProgress,
            CancellationToken cancellationToken = default)
        {
            await onProgress(new OllamaPullProgress { Status = "downloading", Completed = 50, Total = 100 });
            await onProgress(new OllamaPullProgress { Status = "success", Completed = 100, Total = 100 });
        }

        public Task DeleteModelAsync(string baseUrl, string modelName, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<OllamaGenerateResult> GenerateAsync(
            string baseUrl,
            string modelName,
            string prompt,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new OllamaGenerateResult { Response = "pong", Done = true });

        public Task<OllamaEmbeddingsResult> EmbeddingsAsync(
            string baseUrl,
            string modelName,
            string input,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new OllamaEmbeddingsResult { Embedding = [0.1f, 0.2f] });

        public Task<bool> IsReachableAsync(string baseUrl, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }
}
