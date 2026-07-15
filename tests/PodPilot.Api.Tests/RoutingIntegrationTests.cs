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
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Routing;
using PodPilot.Domain.Enums;

namespace PodPilot.Api.Tests;

public class RoutingIntegrationTests : IClassFixture<RoutingWebApplicationFactory>
{
    private readonly HttpClient client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RoutingIntegrationTests(RoutingWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Routing_Dashboard_Policy_And_Simulate_Work()
    {
        var auth = await RegisterAndAuthenticateAsync("routing");
        SetBearerToken(auth.AccessToken);

        var dashboardResponse = await client.GetAsync("/api/v1/routing");
        Assert.Equal(HttpStatusCode.OK, dashboardResponse.StatusCode);
        var dashboard = await dashboardResponse.Content.ReadFromJsonAsync<ApiResponse<RoutingDashboardResponse>>(JsonOptions);
        Assert.NotNull(dashboard?.Data);
        Assert.False(string.IsNullOrWhiteSpace(dashboard.Data.Strategy));

        var policyResponse = await client.GetAsync("/api/v1/routing/policy");
        Assert.Equal(HttpStatusCode.OK, policyResponse.StatusCode);

        var updateResponse = await client.PutAsJsonAsync(
            "/api/v1/routing/policy",
            new UpdateRoutingPolicySettingsRequest
            {
                Strategy = "LowestCost",
                CostWeight = 0.55,
                LatencyWeight = 0.10,
                ReliabilityWeight = 0.15,
                ContextWeight = 0.05,
                FeaturesWeight = 0.05,
                AvailabilityWeight = 0.10,
                MaxRetries = 2,
                FailoverStrategy = "RetryThenFailover",
            });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<RoutingPolicySettingsResponse>>(JsonOptions);
        Assert.NotNull(updated?.Data);
        Assert.Equal("LowestCost", updated.Data.Strategy);

        var simulateResponse = await client.PostAsJsonAsync(
            "/api/v1/routing/simulate",
            new SimulateRoutingRequest
            {
                Prompt = "Summarize this article in three bullets",
                Strategy = "Balanced",
            });

        Assert.Equal(HttpStatusCode.OK, simulateResponse.StatusCode);
        var simulation = await simulateResponse.Content.ReadFromJsonAsync<ApiResponse<SimulateRoutingResponse>>(JsonOptions);
        Assert.NotNull(simulation?.Data);
        Assert.Equal("Summarization", simulation.Data.TaskType);
        Assert.False(string.IsNullOrWhiteSpace(simulation.Data.DecisionReason));

        var historyResponse = await client.GetAsync("/api/v1/routing/history");
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);
        var history = await historyResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<RoutingHistoryItemResponse>>>(JsonOptions);
        Assert.NotNull(history?.Data);
        Assert.NotEmpty(history.Data);

        var modelsResponse = await client.GetAsync("/api/v1/routing/models");
        Assert.Equal(HttpStatusCode.OK, modelsResponse.StatusCode);
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

public sealed class RoutingWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DatabaseProvider", "InMemory");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"InMemory-routing-{Guid.NewGuid():N}");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAiProvider>();
            services.AddSingleton<IAiProvider>(new FakeRoutingAiProvider());
        });
    }
}

internal sealed class FakeRoutingAiProvider : IAiProvider
{
    public AiProviderKind ProviderKind => AiProviderKind.OpenAi;

    public Task<IReadOnlyList<AiModelInfo>> ListModelsAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AiModelInfo>>(
        [
            new AiModelInfo
            {
                ModelName = "gpt-4o-mini",
                DisplayName = "GPT-4o mini",
                ContextLength = 128000,
                SupportsStreaming = true,
                InputCostPerMillionTokens = 0.15m,
                OutputCostPerMillionTokens = 0.60m,
            },
        ]);

    public Task<AiChatResponse> ChatAsync(
        AiProviderConnection connection,
        AiChatRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new AiChatResponse
        {
            Id = "chat-1",
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
        Task.FromResult(new AiEmbeddingResponse
        {
            Model = request.Model,
            Embeddings = [new float[] { 0.1f, 0.2f }],
        });

    public Task<AiProviderHealthResult> HealthAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new AiProviderHealthResult { IsHealthy = true, LatencyMs = 40 });

    public Task<AiCredentialValidationResult> ValidateCredentialsAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new AiCredentialValidationResult { IsValid = true });

    public Task<int?> CountTokensAsync(
        AiProviderConnection connection,
        string model,
        string text,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<int?>(text.Length / 4);
}
