namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Encrypts and decrypts sensitive values such as provider API keys.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts a plain-text value.
    /// </summary>
    /// <param name="plainText">The value to encrypt.</param>
    /// <returns>The encrypted payload.</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts an encrypted payload.
    /// </summary>
    /// <param name="cipherText">The encrypted payload.</param>
    /// <returns>The decrypted plain-text value.</returns>
    string Decrypt(string cipherText);
}
