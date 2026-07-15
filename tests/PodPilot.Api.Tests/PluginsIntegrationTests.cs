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
using PodPilot.Application.Models.Plugins;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Plugins;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Api.Tests;

public class PluginsIntegrationTests : IClassFixture<PluginWebApplicationFactory>
{
    private readonly HttpClient client;
    private readonly PluginWebApplicationFactory factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PluginsIntegrationTests(PluginWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Install_Plugin_And_Register_Mcp_Server_Work_End_To_End()
    {
        var auth = await RegisterAndAuthenticateAsync("plugins");
        SetBearerToken(auth.AccessToken);

        var listResponse = await client.GetAsync("/api/v1/plugins");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<PluginResponse>>>(JsonOptions);
        Assert.NotNull(list?.Data);
        Assert.Contains(list.Data, p => p.PackageId == "com.podpilot.utility.health");

        var installResponse = await client.PostAsJsonAsync(
            "/api/v1/plugins",
            new InstallPluginRequest
            {
                PackageId = "com.podpilot.utility.health",
                GrantedPermissions = ["Plugin.Read"],
            });
        Assert.Equal(HttpStatusCode.Created, installResponse.StatusCode);
        var installed = await installResponse.Content.ReadFromJsonAsync<ApiResponse<PluginResponse>>(JsonOptions);
        Assert.NotNull(installed?.Data?.InstallationId);

        var dashboardResponse = await client.GetAsync("/api/v1/plugins/dashboard");
        Assert.Equal(HttpStatusCode.OK, dashboardResponse.StatusCode);

        var createMcp = await client.PostAsJsonAsync(
            "/api/v1/mcp/servers",
            new CreateMcpServerRequest
            {
                Name = "filesystem-local",
                Version = "1.0.0",
                ServerKind = "Filesystem",
                Endpoint = "http://127.0.0.1:7101/mcp",
                AuthScheme = "None",
                DiscoverOnCreate = false,
            });
        Assert.Equal(HttpStatusCode.Created, createMcp.StatusCode);
        var mcp = await createMcp.Content.ReadFromJsonAsync<ApiResponse<McpServerResponse>>(JsonOptions);
        Assert.NotNull(mcp?.Data);
        Assert.False(mcp.Data.HasCredential);
        var createBody = await createMcp.Content.ReadAsStringAsync();
        Assert.DoesNotContain("EncryptedCredential", createBody, StringComparison.OrdinalIgnoreCase);

        var kinds = await client.GetAsync("/api/v1/mcp/kinds");
        Assert.Equal(HttpStatusCode.OK, kinds.StatusCode);

        await SeedToolAsync(mcp.Data.Id, auth);

        var execute = await client.PostAsJsonAsync(
            "/api/v1/mcp/tools/execute",
            new ExecuteMcpToolRequest
            {
                ServerId = mcp.Data.Id,
                ToolName = "echo",
                ArgumentsJson = "{\"text\":\"hi\"}",
            });
        Assert.Equal(HttpStatusCode.OK, execute.StatusCode);
        var executeResult = await execute.Content.ReadFromJsonAsync<ApiResponse<ExecuteMcpToolResponse>>(JsonOptions);
        Assert.NotNull(executeResult?.Data);
        Assert.True(executeResult.Data.Succeeded);

        var deleteMcp = await client.DeleteAsync($"/api/v1/mcp/servers/{mcp.Data.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteMcp.StatusCode);

        var uninstall = await client.DeleteAsync($"/api/v1/plugins/{installed.Data.InstallationId}");
        Assert.Equal(HttpStatusCode.NoContent, uninstall.StatusCode);
    }

    private async Task SeedToolAsync(Guid serverId, AuthResponse auth)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var server = await db.McpServers.FirstAsync(s => s.Id == serverId);
        await db.AddMcpToolAsync(new McpTool
        {
            OrganizationId = server.OrganizationId,
            McpServerId = server.Id,
            Name = "echo",
            Description = "Echo tool",
            IsEnabled = true,
            DiscoveredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        _ = auth;
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

public sealed class PluginWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DatabaseProvider", "InMemory");
        builder.UseSetting("InMemoryDatabaseName", $"PodPilotPluginsTest_{Guid.NewGuid()}");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IMcpConnectionFactory>();
            services.AddSingleton<IMcpConnectionFactory, FakeMcpConnectionFactory>();
        });
    }
}

internal sealed class FakeMcpConnectionFactory : IMcpConnectionFactory
{
    public Task<IMcpConnection> CreateAsync(
        McpServer server,
        string? decryptedCredential,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IMcpConnection>(new FakeMcpConnection());
}

internal sealed class FakeMcpConnection : IMcpConnection
{
    public bool IsConnected { get; private set; }

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = true;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<McpToolInfo>> ListToolsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<McpToolInfo>>(
        [
            new McpToolInfo { Name = "echo", Description = "Echo" },
        ]);

    public Task<IReadOnlyList<McpResourceInfo>> ListResourcesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<McpResourceInfo>>([]);

    public Task<IReadOnlyList<McpPromptInfo>> ListPromptsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<McpPromptInfo>>([]);

    public Task<McpToolCallResult> CallToolAsync(
        string toolName,
        string argumentsJson,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new McpToolCallResult
        {
            Succeeded = true,
            ContentJson = argumentsJson,
            DurationMs = 5,
        });

    public Task<bool> PingAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
