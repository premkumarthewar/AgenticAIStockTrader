using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockTrader.Infrastructure.Clients.Kite;
using StockTrader.Infrastructure.Clients.SmartApi;
using StockTrader.Shared.Results;

namespace StockTrader.Api.Controllers;

/// <summary>
/// Broker login/session endpoints. Deliberately separate from IBrokerClient (used only
/// for placing orders via BrokerClientResolver, after an approval) - logging in isn't
/// part of that broker-agnostic contract because Alpaca doesn't need one, so this talks
/// to the Zerodha/Angel One session types directly instead of going through Application.
/// </summary>
[ApiController]
[Route("api/brokers")]
[Authorize]
public class BrokersController(
    IKiteSessionProvider kiteSessionProvider,
    ISmartApiSessionProvider smartApiSessionProvider) : ControllerBase
{
    /// <summary>
    /// The URL to open in a browser to start Zerodha's daily interactive login. Zerodha
    /// redirects back to whatever redirect_url is registered against this app's API key
    /// in the Kite Connect developer console, with a request_token query parameter -
    /// point that redirect_url at GET api/brokers/zerodha/callback.
    /// </summary>
    [HttpGet("zerodha/login-url")]
    public IActionResult GetZerodhaLoginUrl()
    {
        return Ok(new { loginUrl = kiteSessionProvider.GetLoginUrl() });
    }

    [HttpGet("zerodha/callback")]
    public async Task<IActionResult> ZerodhaCallback([FromQuery(Name = "request_token")] string? requestToken, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("success", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new Error("BrokerError", $"Zerodha login did not succeed (status: {status})."));

        if (string.IsNullOrWhiteSpace(requestToken))
            return BadRequest(new Error("BadRequest", "No request_token was supplied by Zerodha's redirect."));

        Result result = await kiteSessionProvider.ExchangeRequestTokenAsync(requestToken, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { message = "Zerodha session established for today." });
    }

    [HttpGet("zerodha/status")]
    public IActionResult GetZerodhaStatus()
    {
        bool connected = kiteSessionProvider.TryGetAccessToken(out _);

        return Ok(new { connected });
    }

    /// <summary>
    /// Triggers (or confirms) an Angel One login. Unlike Zerodha this needs no manual
    /// step - GetJwtTokenAsync logs in with the configured TOTP secret if it doesn't
    /// already hold a valid session - so this is mainly useful to verify credentials are
    /// configured correctly.
    /// </summary>
    [HttpPost("angelone/connect")]
    public async Task<IActionResult> ConnectAngelOne(CancellationToken cancellationToken)
    {
        Result<string> result = await smartApiSessionProvider.GetJwtTokenAsync(cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { connected = true });
    }

    [HttpGet("angelone/status")]
    public async Task<IActionResult> GetAngelOneStatus(CancellationToken cancellationToken)
    {
        Result<string> result = await smartApiSessionProvider.GetJwtTokenAsync(cancellationToken);

        return Ok(new { connected = result.IsSuccess });
    }
}
