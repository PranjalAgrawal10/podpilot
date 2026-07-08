using Microsoft.AspNetCore.DataProtection;
using PodPilot.Infrastructure.Services;

namespace PodPilot.Application.Tests.Providers;

public class EncryptionServiceTests
{
    [Fact]
    public void Encrypt_And_Decrypt_RoundTrip_ReturnsOriginalValue()
    {
        var provider = new EphemeralDataProtectionProvider();
        var service = new EncryptionService(provider);

        const string secret = "runpod-api-key-12345";
        var encrypted = service.Encrypt(secret);
        var decrypted = service.Decrypt(encrypted);

        Assert.NotEqual(secret, encrypted);
        Assert.Equal(secret, decrypted);
    }

    [Fact]
    public void Encrypt_ProducesDifferentCiphertext_ForSameInput()
    {
        var provider = new EphemeralDataProtectionProvider();
        var service = new EncryptionService(provider);

        const string secret = "runpod-api-key-12345";
        var first = service.Encrypt(secret);
        var second = service.Encrypt(secret);

        Assert.NotEqual(first, second);
        Assert.Equal(secret, service.Decrypt(first));
        Assert.Equal(secret, service.Decrypt(second));
    }
}
