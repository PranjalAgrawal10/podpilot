using Microsoft.EntityFrameworkCore;
using Moq;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Security;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Security;

namespace PodPilot.Application.Tests.Security;

public class EnterpriseAuditTests
{
    [Fact]
    public async Task AppendAsync_Is_Immutable_Against_Updates()
    {
        await using var db = CreateDb();
        var service = new EnterpriseAuditService(
            db,
            Mock.Of<IDateTimeService>(d => d.UtcNow == DateTime.UtcNow),
            Mock.Of<ISecurityNotificationService>());

        var orgId = Guid.NewGuid();
        await service.AppendAsync(new EnterpriseAuditEntry
        {
            OrganizationId = orgId,
            Category = AuditEventCategory.Authentication,
            EventType = "Login",
            Summary = "User logged in",
        });

        var entity = await db.AuditEvents.SingleAsync();
        entity.Summary = "tampered";
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task QueryAsync_Filters_By_Organization()
    {
        await using var db = CreateDb();
        var now = DateTime.UtcNow;
        await db.AddAuditEventAsync(new AuditEvent
        {
            OrganizationId = Guid.NewGuid(),
            Category = AuditEventCategory.Secret,
            EventType = "SecretAccessed",
            Summary = "other",
            OccurredAt = now,
            IsImmutable = true,
        });
        var orgId = Guid.NewGuid();
        await db.AddAuditEventAsync(new AuditEvent
        {
            OrganizationId = orgId,
            Category = AuditEventCategory.Secret,
            EventType = "SecretAccessed",
            Summary = "mine",
            OccurredAt = now,
            IsImmutable = true,
        });
        await db.SaveChangesAsync();

        var service = new EnterpriseAuditService(
            db,
            Mock.Of<IDateTimeService>(d => d.UtcNow == now),
            Mock.Of<ISecurityNotificationService>());

        var results = await service.QueryAsync(orgId, new AuditQueryRequest { Take = 10 });
        Assert.Single(results);
        Assert.Equal("mine", results[0].Summary);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"audit-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
