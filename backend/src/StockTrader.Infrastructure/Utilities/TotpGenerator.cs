using System.Security.Cryptography;

namespace StockTrader.Infrastructure.Utilities;

/// <summary>
/// A minimal RFC 6238 TOTP generator (the standard "6-digit authenticator app" code),
/// used to log in to Angel One's SmartAPI without a manual step. Written by hand rather
/// than pulling in a NuGet package - the algorithm is a few dozen lines of well-specified
/// HMAC-SHA1 math and this is the only place in the codebase that needs it.
/// </summary>
public static class TotpGenerator
{
    public static string GenerateCode(string base32Secret, int digits = 6, int stepSeconds = 30)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(base32Secret);

        byte[] key = Base32Decode(base32Secret);

        long timeStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / stepSeconds;

        byte[] counter = BitConverter.GetBytes(timeStep);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(counter);

        byte[] hash = HMACSHA1.HashData(key, counter);

        int offset = hash[^1] & 0x0F;

        int binaryCode =
            ((hash[offset] & 0x7F) << 24) |
            ((hash[offset + 1] & 0xFF) << 16) |
            ((hash[offset + 2] & 0xFF) << 8) |
            (hash[offset + 3] & 0xFF);

        int code = binaryCode % (int)Math.Pow(10, digits);

        return code.ToString().PadLeft(digits, '0');
    }

    private static byte[] Base32Decode(string base32)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        string cleaned = base32.Trim().TrimEnd('=').ToUpperInvariant();

        int outputLength = cleaned.Length * 5 / 8;

        byte[] result = new byte[outputLength];

        int bitBuffer = 0;
        int bitsInBuffer = 0;
        int outputIndex = 0;

        foreach (char c in cleaned)
        {
            int value = alphabet.IndexOf(c);

            if (value < 0)
                throw new FormatException($"'{c}' is not a valid base32 character.");

            bitBuffer = (bitBuffer << 5) | value;
            bitsInBuffer += 5;

            if (bitsInBuffer >= 8)
            {
                bitsInBuffer -= 8;
                result[outputIndex++] = (byte)((bitBuffer >> bitsInBuffer) & 0xFF);
            }
        }

        return result;
    }
}
