using Microsoft.EntityFrameworkCore;
using Moq;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Infrastructure.AiProviders;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Application.Tests.AiProviders;

public class AiFailoverServiceTests
{
    [Fact]
    public async Task RecordFailoverAsync_Persists_Event()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"failover-{Guid.NewGuid():N}")
            .Options;
        await using var db = new ApplicationDbContext(options);
        var dateTime = new Mock<IDateTimeService>();
        dateTime.SetupGet(d => d.UtcNow).Returns(new DateTime(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc));

        var service = new AiFailoverService(db, dateTime.Object);
        var orgId = Guid.NewGuid();
        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();

        await service.RecordFailoverAsync(orgId, fromId, toId, "gpt-4o", "timeout", succeeded: true);

        var stored = await db.AiFailoverEvents.SingleAsync();
        Assert.Equal(orgId, stored.OrganizationId);
        Assert.Equal(fromId, stored.FromProviderId);
        Assert.Equal(toId, stored.ToProviderId);
        Assert.Equal("gpt-4o", stored.ModelName);
        Assert.True(stored.Succeeded);
    }
}
