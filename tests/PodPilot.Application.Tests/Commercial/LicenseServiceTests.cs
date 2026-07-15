using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Models.Commercial;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Commercial;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Application.Tests.Commercial;

public class LicenseServiceTests
{
    [Fact]
    public async Task Issue_Activate_And_Validate_License()
    {
        await using var db = CreateDb();
        var service = new LicenseService(db);

        var issued = await service.IssueAsync(new IssueLicenseRequest
        {
            Edition = LicenseEdition.Professional,
            DeploymentMode = LicenseDeploymentMode.Online,
            MaxSeats = 10,
        });

        Assert.StartsWith("PP-", issued.LicenseKey);
        Assert.Equal(LicenseEdition.Professional, issued.Info.Edition);

        var orgId = Guid.NewGuid();
        var activated = await service.ActivateAsync(orgId, issued.LicenseKey);
        Assert.True(activated.IsActivated);
        Assert.True(activated.IsValid);

        var validated = await service.ValidateAsync(orgId);
        Assert.True(validated.IsValid);
        Assert.Equal(LicenseEdition.Professional, validated.Edition);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"license-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
