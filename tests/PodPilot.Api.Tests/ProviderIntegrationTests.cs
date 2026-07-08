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
using PodPilot.Application.Models.Compute;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Providers;
using PodPilot.Domain.Enums;

namespace PodPilot.Api.Tests;

public class ProviderIntegrationTests : IClassFixture<ProviderWebApplicationFactory>
{
    private readonly HttpClient client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ProviderIntegrationTests(ProviderWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Validate_New_Provider_Credentials_Before_Creation()
    {
        var auth = await RegisterAndAuthenticateAsync("provider-validate");
        SetBearerToken(auth.AccessToken);

        var validateResponse = await client.PostAsJsonAsync(
            "/api/v1/providers/validate",
            new ValidateCredentialsRequest
            {
                ProviderType = "RunPod",
                ApiKey = "rp_test_key",
            });

        Assert.Equal(HttpStatusCode.OK, validateResponse.StatusCode);

        var validation = await validateResponse.Content.ReadFromJsonAsync<ApiResponse<ProviderValidationResponse>>(JsonOptions);
        Assert.NotNull(validation?.Data);
        Assert.True(validation.Data.IsValid);
        Assert.Equal("Connected", validation.Data.ConnectionStatus);
        Assert.Equal("user-123", validation.Data.AccountInfo?.AccountId);
        Assert.NotEmpty(validation.Data.Regions);
        Assert.NotEmpty(validation.Data.Gpus);

        var body = await validateResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("ApiKey", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_Validate_And_Delete_Provider_Work_End_To_End()
    {
        var auth = await RegisterAndAuthenticateAsync("provider-owner");
        SetBearerToken(auth.AccessToken);

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/providers",
            new CreateProviderRequest
            {
                Name = "runpod-primary",
                ProviderType = "RunPod",
                DisplayName = "RunPod Primary",
                ApiKey = "rp_test_key",
                Description = "Primary GPU provider",
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProviderResponse>>(JsonOptions);
        Assert.NotNull(created?.Data);
        Assert.True(created.Data.IsValidated);
        Assert.Equal("RunPod", created.Data.ProviderType);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("ApiKey", createBody, StringComparison.OrdinalIgnoreCase);

        var validateResponse = await client.PostAsJsonAsync(
            $"/api/v1/providers/{created.Data.Id}/validate",
            new ValidateProviderRequest());

        Assert.Equal(HttpStatusCode.OK, validateResponse.StatusCode);

        var validation = await validateResponse.Content.ReadFromJsonAsync<ApiResponse<ProviderValidationResponse>>(JsonOptions);
        Assert.NotNull(validation?.Data);
        Assert.True(validation.Data.IsValid);
        Assert.Equal("Connected", validation.Data.ConnectionStatus);
        Assert.Equal("user-123", validation.Data.AccountInfo?.AccountId);
        Assert.NotEmpty(validation.Data.Regions);

        var deleteResponse = await client.DeleteAsync($"/api/v1/providers/{created.Data.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await client.GetAsync("/api/v1/providers");
        var providers = await listResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ProviderResponse>>>(JsonOptions);
        Assert.NotNull(providers?.Data);
        Assert.DoesNotContain(providers.Data, p => p.Id == created.Data.Id);
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

public sealed class ProviderWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DatabaseProvider", "InMemory");
        builder.UseSetting("InMemoryDatabaseName", $"PodPilotProviderTest_{Guid.NewGuid()}");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IComputeProvider>();
            services.AddSingleton<IComputeProvider, TestRunPodComputeProvider>();
            services.RemoveAll<IComputeProviderFactory>();
            services.AddSingleton<IComputeProviderFactory, PodPilot.Infrastructure.Compute.ComputeProviderFactory>();
        });
    }
}

internal sealed class TestRunPodComputeProvider : IComputeProvider
{
    public ProviderType ProviderType => ProviderType.RunPod;

    public Task<ProviderValidationResult> ValidateCredentialsAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var regions = new List<ProviderRegionInfo>
        {
            new() { RegionId = "US", Name = "United States", IsAvailable = true },
        };
        var gpus = new List<ProviderGpuInfo>
        {
            new()
            {
                GpuId = "NVIDIA GeForce RTX 4090",
                Name = "NVIDIA GeForce RTX 4090",
                GpuType = GpuType.RTX4090,
                MemoryGb = 24,
            },
        };

        return Task.FromResult(new ProviderValidationResult
        {
            IsValid = true,
            ConnectionStatus = ProviderConnectionStatus.Connected,
            AccountInfo = CreateAccountInfo(),
            Regions = regions,
            Gpus = gpus,
            Templates = [],
        });
    }

    public Task<ProviderAccountInfo> GetAccountInfoAsync(
        string apiKey,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(CreateAccountInfo());

    public Task<IReadOnlyList<ProviderRegionInfo>> ListRegionsAsync(
        string apiKey,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ProviderRegionInfo>>(
        [
            new() { RegionId = "US", Name = "United States", IsAvailable = true },
        ]);

    public Task<IReadOnlyList<ProviderGpuInfo>> ListGpusAsync(
        string apiKey,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ProviderGpuInfo>>(
        [
            new()
            {
                GpuId = "NVIDIA GeForce RTX 4090",
                Name = "NVIDIA GeForce RTX 4090",
                GpuType = GpuType.RTX4090,
                MemoryGb = 24,
            },
        ]);

    public Task<IReadOnlyList<ProviderTemplateInfo>> ListTemplatesAsync(
        string apiKey,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ProviderTemplateInfo>>([]);

    public Task<ProviderHealthResult> CheckHealthAsync(
        string apiKey,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new ProviderHealthResult
        {
            Status = ProviderConnectionStatus.Connected,
            ResponseTimeMs = 25,
            CheckedAt = DateTime.UtcNow,
        });

    private static ProviderAccountInfo CreateAccountInfo() =>
        new()
        {
            AccountId = "user-123",
            Email = "owner@runpod.test",
            DisplayName = "owner@runpod.test",
            Balance = 42.5m,
            Currency = "USD",
        };
}
