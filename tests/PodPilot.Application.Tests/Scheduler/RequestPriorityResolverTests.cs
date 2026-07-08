using PodPilot.Application.Models.Gateway;
using PodPilot.Application.Scheduler;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Tests.Scheduler;

public class RequestPriorityResolverTests
{
    private readonly RequestPriorityResolver resolver = new();

    [Fact]
    public void Resolve_ReturnsHigh_ForStreamingRequests()
    {
        var priority = resolver.Resolve(new GatewayAuthContext { ApiKeyId = Guid.NewGuid(), OrganizationId = Guid.NewGuid() }, "/v1/chat/completions", isStreaming: true);
        Assert.Equal(RequestPriority.High, priority);
    }

    [Fact]
    public void Resolve_ReturnsLow_ForBatchPaths()
    {
        var priority = resolver.Resolve(new GatewayAuthContext { ApiKeyId = Guid.NewGuid(), OrganizationId = Guid.NewGuid() }, "/v1/batch/completions", isStreaming: false);
        Assert.Equal(RequestPriority.Low, priority);
    }

    [Fact]
    public void Resolve_ReturnsNormal_ForOrganizationKeys()
    {
        var priority = resolver.Resolve(new GatewayAuthContext { ApiKeyId = Guid.NewGuid(), OrganizationId = Guid.NewGuid() }, "/v1/responses", isStreaming: false);
        Assert.Equal(RequestPriority.Normal, priority);
    }
}
