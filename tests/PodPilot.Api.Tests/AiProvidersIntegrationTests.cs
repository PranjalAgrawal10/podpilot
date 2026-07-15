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
using PodPilot.Application.Models.AiProviders;
using PodPilot.Contracts.AiProviders;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Domain.Enums;

namespace PodPilot.Api.Tests;

public class AiProvidersIntegrationTests : IClassFixture<AiProviderWebApplicationFactory>
{
    private readonly HttpClient client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AiProvidersIntegrationTests(AiProviderWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_List_And_Delete_Ai_Provider_Work_End_To_End()
    {
        var auth = await RegisterAndAuthenticateAsync("ai-provider");
        SetBearerToken(auth.AccessToken);

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/ai/providers",
            new CreateAiProviderRequest
            {
                Name = "openai-primary",
                DisplayName = "OpenAI Primary",
                ProviderKind = "OpenAi",
                ApiKey = "sk-test-key",
                IsEnabled = true,
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<AiProviderResponse>>(JsonOptions);
        Assert.NotNull(created?.Data);
        Assert.True(created.Data.IsValidated);
        Assert.Equal("OpenAi", created.Data.ProviderKind);

        var createBody = await createResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("EncryptedApiKey", createBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sk-test-key", createBody, StringComparison.OrdinalIgnoreCase);

        var listResponse = await client.GetAsync("/api/v1/ai/providers");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<AiProviderResponse>>>(JsonOptions);
        Assert.NotNull(list?.Data);
        Assert.Contains(list.Data, p => p.Id == created.Data.Id);

        var kindsResponse = await client.GetAsync("/api/v1/ai/provider-kinds");
        Assert.Equal(HttpStatusCode.OK, kindsResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/v1/ai/providers/{created.Data.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
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

public sealed class AiProviderWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DatabaseProvider", "InMemory");
        builder.UseSetting("InMemoryDatabaseName", $"PodPilotAiProviderTest_{Guid.NewGuid()}");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAiProvider>();
            services.AddSingleton<IAiProvider, TestOpenAiProvider>();
            services.RemoveAll<IAiProviderFactory>();
            services.AddSingleton<IAiProviderFactory, PodPilot.Infrastructure.AiProviders.AiProviderFactory>();
        });
    }
}

internal sealed class TestOpenAiProvider : IAiProvider
{
    public AiProviderKind ProviderKind => AiProviderKind.OpenAi;

    public Task<IReadOnlyList<AiModelInfo>> ListModelsAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AiModelInfo>>(
        [
            new AiModelInfo { ModelName = "gpt-4o", DisplayName = "GPT-4o", SupportsStreaming = true },
        ]);

    public Task<AiChatResponse> ChatAsync(
        AiProviderConnection connection,
        AiChatRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new AiChatResponse
        {
            Id = "chatcmpl-test",
            Model = request.Model,
            Content = "ok",
            ProviderKind = ProviderKind,
        });

    public Task StreamChatAsync(
        AiProviderConnection connection,
        AiChatRequest request,
        Stream responseStream,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<AiEmbeddingResponse> EmbeddingsAsync(
        AiProviderConnection connection,
        AiEmbeddingRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new AiEmbeddingResponse { Model = request.Model, Embeddings = [] });

    public Task<AiProviderHealthResult> HealthAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new AiProviderHealthResult { IsHealthy = true, LatencyMs = 5, Message = "OK" });

    public Task<AiCredentialValidationResult> ValidateCredentialsAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new AiCredentialValidationResult { IsValid = true, Message = "OK" });

    public Task<int?> CountTokensAsync(
        AiProviderConnection connection,
        string model,
        string text,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<int?>(text.Length / 4);
}
