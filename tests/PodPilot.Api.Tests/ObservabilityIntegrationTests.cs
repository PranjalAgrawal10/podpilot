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
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Observability;
using PodPilot.Contracts.Pods;
using PodPilot.Contracts.Providers;
using PodPilot.Infrastructure.Compute;
using PodPilot.Infrastructure.Services;

namespace PodPilot.Api.Tests;

public class ObservabilityIntegrationTests : IClassFixture<ObservabilityWebApplicationFactory>
{
  private readonly HttpClient client;
  private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

  public ObservabilityIntegrationTests(ObservabilityWebApplicationFactory factory)
  {
    client = factory.CreateClient();
  }

  [Fact]
  public async Task Observability_Endpoints_Return_Expected_Data()
  {
    var auth = await RegisterAndAuthenticateAsync("observability-owner");
    SetBearerToken(auth.AccessToken);

    var providerId = await CreateProviderAsync();
    await CreatePodAsync(providerId, "observability-pod");

    var liveMetricsResponse = await client.GetAsync("/api/v1/metrics/live");
    Assert.Equal(HttpStatusCode.OK, liveMetricsResponse.StatusCode);

    var liveMetrics = await liveMetricsResponse.Content.ReadFromJsonAsync<ApiResponse<LiveMetricsResponse>>(JsonOptions);
    Assert.NotNull(liveMetrics?.Data);
    Assert.True(liveMetrics.Data.CapturedAt > DateTime.MinValue);

    var costResponse = await client.GetAsync("/api/v1/cost?period=Hourly");
    Assert.Equal(HttpStatusCode.OK, costResponse.StatusCode);

    var cost = await costResponse.Content.ReadFromJsonAsync<ApiResponse<CostResponse>>(JsonOptions);
    Assert.NotNull(cost?.Data);
    Assert.True(cost.Data.HourlyCost > 0);
    Assert.Equal("Hourly", cost.Data.Period);
    Assert.NotEmpty(cost.Data.PodBreakdowns);

    var systemHealthResponse = await client.GetAsync("/api/v1/health/system");
    Assert.Equal(HttpStatusCode.OK, systemHealthResponse.StatusCode);

    var systemHealth = await systemHealthResponse.Content.ReadFromJsonAsync<ApiResponse<SystemHealthResponse>>(JsonOptions);
    Assert.NotNull(systemHealth?.Data);
    Assert.NotEmpty(systemHealth.Data.Components);
    Assert.Contains(systemHealth.Data.Components, c => c.Component == "Database");
    Assert.Contains(systemHealth.Data.Components, c => c.Component == "Gateway");

    var alertsResponse = await client.GetAsync("/api/v1/alerts");
    Assert.Equal(HttpStatusCode.OK, alertsResponse.StatusCode);

    var alerts = await alertsResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<AlertResponse>>>(JsonOptions);
    Assert.NotNull(alerts?.Data);
  }

  private async Task<Guid> CreateProviderAsync()
  {
    var createResponse = await client.PostAsJsonAsync(
      "/api/v1/providers",
      new CreateProviderRequest
      {
        Name = "observability-provider",
        ProviderType = "RunPod",
        DisplayName = "Observability RunPod",
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

public sealed class ObservabilityWebApplicationFactory : WebApplicationFactory<Program>
{
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseEnvironment("Testing");
    builder.UseSetting("DatabaseProvider", "InMemory");
    builder.UseSetting("InMemoryDatabaseName", $"PodPilotObservabilityTest_{Guid.NewGuid()}");

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
