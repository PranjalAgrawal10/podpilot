using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Moq;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Security;

namespace PodPilot.Application.Tests.Security;

public class TotpAndMfaTests
{
    [Fact]
    public void TotpService_Generates_And_Validates_Code()
    {
        var secret = TotpService.GenerateSecret();
        var code = TotpService.GenerateCode(secret);

        Assert.True(TotpService.ValidateCode(secret, code));
        Assert.False(TotpService.ValidateCode(secret, "000000"));
    }

    [Fact]
    public async Task MfaService_Enroll_Confirm_And_Verify()
    {
        await using var db = CreateDb();
        var encryption = new Mock<IEncryptionService>();
        encryption.Setup(e => e.Encrypt(It.IsAny<string>())).Returns<string>(v => "enc:" + v);
        encryption.Setup(e => e.Decrypt(It.IsAny<string>())).Returns<string>(v => v.Replace("enc:", string.Empty));

        var userId = Guid.NewGuid();
        var identity = new Mock<IIdentityService>();
        identity.Setup(i => i.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = userId,
                Email = "mfa@podpilot.test",
                FirstName = "Mfa",
                LastName = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            });

        var mfa = new MfaService(
            db,
            encryption.Object,
            identity.Object,
            Mock.Of<IDateTimeService>(d => d.UtcNow == DateTime.UtcNow));

        var enrollment = await mfa.BeginEnrollmentAsync(userId);
        Assert.False(string.IsNullOrWhiteSpace(enrollment.OtpAuthUri));

        var code = TotpService.GenerateCode(enrollment.SharedSecret);
        await mfa.ConfirmEnrollmentAsync(userId, code);

        Assert.True(await mfa.IsEnabledAsync(userId));
        Assert.True(await mfa.VerifyAsync(userId, TotpService.GenerateCode(enrollment.SharedSecret)));
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"mfa-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
