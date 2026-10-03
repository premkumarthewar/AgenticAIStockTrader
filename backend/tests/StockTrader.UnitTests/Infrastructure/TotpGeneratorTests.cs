using StockTrader.Infrastructure.Utilities;

namespace StockTrader.UnitTests.Infrastructure;

/// <summary>
/// TotpGenerator always hashes against DateTimeOffset.UtcNow, so a fixed expected code
/// can't be asserted without changing production code to accept an injectable clock -
/// out of scope here. These tests instead pin down the structural/contract guarantees a
/// caller (SmartApiSessionProvider) actually relies on: digit count, that it's numeric,
/// that it's stable within the same time step, and that invalid input is rejected.
/// </summary>
public class TotpGeneratorTests
{
    // Base32 encoding of the ASCII string "12345678901234567890" - the seed used by RFC
    // 6238's own SHA-1 test vectors (Appendix B), reused here only as a realistic-looking
    // secret, not to assert an exact code (see class remarks).
    private const string SampleBase32Secret = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ";

    [Fact]
    public void GenerateCode_DefaultDigits_ReturnsSixNumericCharacters()
    {
        string code = TotpGenerator.GenerateCode(SampleBase32Secret);

        Assert.Equal(6, code.Length);
        Assert.True(code.All(char.IsDigit));
    }

    [Theory]
    [InlineData(6)]
    [InlineData(8)]
    public void GenerateCode_RespectsRequestedDigitCount(int digits)
    {
        string code = TotpGenerator.GenerateCode(SampleBase32Secret, digits);

        Assert.Equal(digits, code.Length);
    }

    [Fact]
    public void GenerateCode_CalledTwiceWithinTheSameStep_ReturnsTheSameCode()
    {
        // A 30s step is wide enough that two calls made back-to-back land in the same
        // window in practice; this only guards against the code changing on every call
        // (e.g. an accidental use of a per-call random value instead of the time step).
        string first = TotpGenerator.GenerateCode(SampleBase32Secret, stepSeconds: 300);
        string second = TotpGenerator.GenerateCode(SampleBase32Secret, stepSeconds: 300);

        Assert.Equal(first, second);
    }

    [Fact]
    public void GenerateCode_DifferentSecrets_ProduceDifferentCodes()
    {
        string codeA = TotpGenerator.GenerateCode(SampleBase32Secret);
        string codeB = TotpGenerator.GenerateCode("ABCDEFGHIJKLMNOPQRSTUVWXYZ234567");

        Assert.NotEqual(codeA, codeB);
    }

    [Fact]
    public void GenerateCode_InvalidBase32Character_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => TotpGenerator.GenerateCode("this-is-not-base32!!!"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateCode_NullOrWhitespaceSecret_Throws(string? secret)
    {
        Assert.ThrowsAny<ArgumentException>(() => TotpGenerator.GenerateCode(secret!));
    }
}
