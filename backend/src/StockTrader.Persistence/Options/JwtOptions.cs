using System.ComponentModel.DataAnnotations;

namespace StockTrader.Persistence.Options;

/// <summary>
/// Signing/validation settings for the JWT bearer tokens issued by AuthService and
/// validated by the JwtBearer authentication handler configured in the Api project's
/// Program.cs (via AddJwtAuthentication). Both sides bind this same "Jwt" section so the
/// signing key can never drift between token issuance and validation. Lives in
/// Persistence (rather than Infrastructure, alongside the other *Options.cs files)
/// because AuthService - the token issuer - lives here too, and Api already references
/// Persistence directly.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// Symmetric signing key. Must be at least 32 characters (256 bits) for HS256. In
    /// production this should come from an environment variable or a secret store, never
    /// committed to appsettings.json.
    /// </summary>
    [Required]
    [MinLength(32)]
    public string Secret { get; init; } = string.Empty;

    public string Issuer { get; init; } = "StockTrader.Api";

    public string Audience { get; init; } = "StockTrader.Client";

    public int AccessTokenMinutes { get; init; } = 60;
}
