using Microsoft.AspNetCore.DataProtection;
using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Encrypts and decrypts sensitive values using ASP.NET Core Data Protection.
/// </summary>
public sealed class EncryptionService : IEncryptionService
{
    private const string ProtectorPurpose = "PodPilot.ProviderCredentials.v1";
    private readonly IDataProtector protector;

    /// <summary>
    /// Initializes a new instance of the <see cref="EncryptionService"/> class.
    /// </summary>
    /// <param name="dataProtectionProvider">The data protection provider.</param>
    public EncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
    }

    /// <inheritdoc />
    public string Encrypt(string plainText) => protector.Protect(plainText);

    /// <inheritdoc />
    public string Decrypt(string cipherText) => protector.Unprotect(cipherText);
}
