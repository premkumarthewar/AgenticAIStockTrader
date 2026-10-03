using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StockTrader.Api.HealthChecks;

/// <summary>
/// Renders a HealthReport as JSON (status + per-check name/status/description/duration)
/// instead of ASP.NET Core's default plain-text "Healthy"/"Unhealthy" body, so a caller
/// (or a human hitting /health/ready directly) can see which dependency actually failed.
/// </summary>
public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = entry.Value.Duration.TotalMilliseconds
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, SerializerOptions));
    }
}
