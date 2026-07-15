using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Moq;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Security;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Persistence;
using PodPilot.Infrastructure.Security.Secrets;

namespace PodPilot.Application.Tests.Security;

public class SecretManagerTests
{
    [Fact]
    public async Task Create_And_Resolve_Does_Not_Expose_Plaintext_In_Entity()
    {
        await using var db = CreateDb();
        var encryption = new Mock<IEncryptionService>();
        encryption.Setup(e => e.Encrypt(It.IsAny<string>())).Returns<string>(v => "enc:" + v);
        encryption.Setup(e => e.Decrypt(It.IsAny<string>())).Returns<string>(v => v["enc:".Length..]);

        var local = new LocalEncryptedSecretProvider(encryption.Object);
        var factory = new SecretProviderFactory([local]);
        var env = new FakeHostEnvironment { EnvironmentName = "Testing" };

        var manager = new SecretManager(
            db,
            factory,
            Mock.Of<IDateTimeService>(d => d.UtcNow == DateTime.UtcNow),
            local,
            env);

        var orgId = Guid.NewGuid();
        var id = await manager.CreateAsync(
            orgId,
            new CreateSecretRequest
            {
                Name = "openai-key",
                SecretKind = SecretKind.AiProviderCredential,
                BackendKind = SecretBackendKind.LocalEncrypted,
                Plaintext = "sk-secret",
            });

        var stored = await db.SecretReferences.SingleAsync(s => s.Id == id);
        Assert.Equal("enc:sk-secret", stored.EncryptedValue);
        Assert.NotEqual("sk-secret", stored.EncryptedValue);

        var resolved = await manager.ResolveAsync(orgId, id);
        Assert.Equal("sk-secret", resolved);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"secrets-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "PodPilot.Tests";

        public string ContentRootPath { get; set; } = Path.GetTempPath();

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
