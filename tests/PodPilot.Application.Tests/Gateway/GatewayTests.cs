using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Gateway;

namespace PodPilot.Application.Tests.Gateway;

public class GatewayRouterTests
{
    [Fact]
    public async Task Should_Route_To_Configured_Model_Pod()
    {
        var organizationId = Guid.NewGuid();
        var podId = Guid.NewGuid();
        var dbContext = CreateContext(organizationId, podId, "llama3", "http://10.0.0.1:11434");

        var router = new GatewayRouter(dbContext, new NoOpPodOrchestrator());
        var result = await router.ResolveAsync(organizationId, "llama3");

        Assert.Equal(podId, result.Pod.Id);
        Assert.Equal("llama3", result.Model);
        Assert.Equal("http://10.0.0.1:11434", result.BaseUrl);
    }

    [Fact]
    public async Task Should_Fallback_To_Default_Route_When_Model_Missing()
    {
        var organizationId = Guid.NewGuid();
        var podId = Guid.NewGuid();
        var dbContext = CreateContext(organizationId, podId, "default-model", "http://10.0.0.2:11434", isDefault: true);

        var router = new GatewayRouter(dbContext, new NoOpPodOrchestrator());
        var result = await router.ResolveAsync(organizationId, "unknown-model");

        Assert.Equal(podId, result.Pod.Id);
    }

    private static IApplicationDbContext CreateContext(
        Guid organizationId,
        Guid podId,
        string modelName,
        string endpoint,
        bool isDefault = false)
    {
        var options = new DbContextOptionsBuilder<PodPilot.Infrastructure.Persistence.ApplicationDbContext>()
            .UseInMemoryDatabase($"gateway-router-{Guid.NewGuid()}")
            .Options;

        var context = new PodPilot.Infrastructure.Persistence.ApplicationDbContext(options);
        context.GpuPods.Add(new GpuPod
        {
            Id = podId,
            OrganizationId = organizationId,
            Name = "test-pod",
            Endpoint = endpoint,
            Status = PodStatus.Running,
            GpuId = "gpu-1",
            Region = "US",
            ProviderId = Guid.NewGuid(),
        });
        context.GatewayRoutes.Add(new GatewayRoute
        {
            OrganizationId = organizationId,
            GpuPodId = podId,
            ModelName = modelName,
            IsDefault = isDefault,
        });
        context.SaveChanges();
        return context;
    }

    private sealed class NoOpPodOrchestrator : IPodOrchestrator
    {
        public Task<OrchestratorRouteResult?> ResolvePodAsync(
            OrchestratorRouteRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<OrchestratorRouteResult?>(null);

        public Task<FailoverResult> HandleFailoverAsync(
            Guid organizationId,
            Guid failedPodId,
            Guid? requestId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new FailoverResult { Success = false });

        public Task<OrchestratorStatus> GetStatusAsync(
            Guid organizationId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new OrchestratorStatus());
    }
}

public class GatewayErrorMapperTests
{
    [Fact]
    public async Task Should_Write_OpenAi_Compatible_Error()
    {
        var context = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() },
        };
        await GatewayErrorMapper.WriteErrorAsync(
            context,
            GatewayErrorFormat.OpenAi,
            (int)HttpStatusCode.BadGateway,
            "gateway_error",
            "Request failed",
            CancellationToken.None);

        Assert.Equal((int)HttpStatusCode.BadGateway, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal("Request failed", document.RootElement.GetProperty("error").GetProperty("message").GetString());
    }

    [Fact]
    public async Task Should_Write_Anthropic_Compatible_Error()
    {
        var context = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() },
        };
        await GatewayErrorMapper.WriteErrorAsync(
            context,
            GatewayErrorFormat.Anthropic,
            (int)HttpStatusCode.BadGateway,
            "gateway_error",
            "Request failed",
            CancellationToken.None);

        Assert.Equal((int)HttpStatusCode.BadGateway, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal("error", document.RootElement.GetProperty("type").GetString());
    }
}

public class GatewayModelsFormatterTests
{
    [Fact]
    public void Should_Format_OpenAi_Model_List()
    {
        const string tags = """
            {
              "models": [
                { "name": "llama3:latest" }
              ]
            }
            """;

        var payload = GatewayModelsFormatter.Format(tags, GatewayErrorFormat.OpenAi);
        using var document = JsonDocument.Parse(payload);

        Assert.Equal("list", document.RootElement.GetProperty("object").GetString());
        Assert.Equal("llama3:latest", document.RootElement.GetProperty("data")[0].GetProperty("id").GetString());
    }
}
