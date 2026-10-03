namespace StockTrader.Infrastructure.Options;

public sealed class AngelOneOptions
{
    public const string SectionName = "Broker:AngelOne";

    public string BaseUrl { get; init; } = "https://apiconnect.angelone.in";

    public string ScripMasterUrl { get; init; } = "https://margincalculator.angelone.in/OpenAPI_File/files/OpenAPIScripMaster.json";

    /// <summary>
    /// API key issued by Angel One's SmartAPI developer console, sent as the
    /// X-PrivateKey header on every request.
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;

    public string ClientCode { get; init; } = string.Empty;

    /// <summary>
    /// The trading PIN/password used with loginByPassword - not the account login
    /// password.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Base32 TOTP secret from linking an authenticator app to the Angel One account.
    /// Used to compute the 6-digit code loginByPassword requires, so login can be fully
    /// automated (unlike Zerodha's interactive redirect flow).
    /// </summary>
    public string TotpSecret { get; init; } = string.Empty;

    /// <summary>
    /// A stable local MAC address string reported in the X-MACAddress header. Angel One
    /// does not appear to validate this beyond requiring the header be present.
    /// </summary>
    public string LocalMacAddress { get; init; } = "00:00:00:00:00:00";
}
