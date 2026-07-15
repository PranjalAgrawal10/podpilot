using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace PodPilot.Infrastructure.Security;

/// <summary>
/// RFC 6238 TOTP helper using HMAC-SHA1.
/// </summary>
public static class TotpService
{
    private const int DefaultDigits = 6;
    private const int DefaultPeriodSeconds = 30;
    private const int DefaultWindow = 1;

    /// <summary>Generates a new base32 shared secret.</summary>
    public static string GenerateSecret(int byteLength = 20)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Base32Encode(bytes);
    }

    /// <summary>Builds an otpauth URI.</summary>
    public static string BuildOtpAuthUri(string issuer, string accountName, string secret) =>
        $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountName)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&digits={DefaultDigits}&period={DefaultPeriodSeconds}";

    /// <summary>Generates the current TOTP code for a shared secret.</summary>
    public static string GenerateCode(string base32Secret)
    {
        var key = Base32Decode(base32Secret);
        var timestep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / DefaultPeriodSeconds;
        return ComputeTotp(key, timestep);
    }

    /// <summary>Validates a TOTP code within a small time window.</summary>
    public static bool ValidateCode(string base32Secret, string code, int window = DefaultWindow)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != DefaultDigits)
        {
            return false;
        }

        var key = Base32Decode(base32Secret);
        var timestep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / DefaultPeriodSeconds;
        for (var i = -window; i <= window; i++)
        {
            var expected = ComputeTotp(key, timestep + i);
            if (CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(expected),
                    Encoding.ASCII.GetBytes(code)))
            {
                return true;
            }
        }

        return false;
    }

    private static string ComputeTotp(byte[] key, long counter)
    {
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(counterBytes);
        }

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counterBytes);
        var offset = hash[^1] & 0x0F;
        var binary =
            ((hash[offset] & 0x7F) << 24) |
            ((hash[offset + 1] & 0xFF) << 16) |
            ((hash[offset + 2] & 0xFF) << 8) |
            (hash[offset + 3] & 0xFF);
        var otp = binary % (int)Math.Pow(10, DefaultDigits);
        return otp.ToString(CultureInfo.InvariantCulture).PadLeft(DefaultDigits, '0');
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new StringBuilder(((data.Length * 8) + 4) / 5);
        var buffer = 0;
        var bitsLeft = 0;
        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bitsLeft += 8;
            while (bitsLeft >= 5)
            {
                output.Append(alphabet[(buffer >> (bitsLeft - 5)) & 0x1F]);
                bitsLeft -= 5;
            }
        }

        if (bitsLeft > 0)
        {
            output.Append(alphabet[(buffer << (5 - bitsLeft)) & 0x1F]);
        }

        return output.ToString();
    }

    private static byte[] Base32Decode(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var cleaned = input.Trim().Replace("=", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        var output = new List<byte>((cleaned.Length * 5) / 8);
        var buffer = 0;
        var bitsLeft = 0;
        foreach (var c in cleaned)
        {
            var val = alphabet.IndexOf(c);
            if (val < 0)
            {
                continue;
            }

            buffer = (buffer << 5) | val;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                output.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
                bitsLeft -= 8;
            }
        }

        return output.ToArray();
    }
}
