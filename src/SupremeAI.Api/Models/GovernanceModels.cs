namespace SupremeAI.Api.Models;

/// <summary>Response returned by <c>GET /health</c>.</summary>
public sealed class HealthResponse
{
    /// <summary>"healthy" | "degraded" | "unhealthy"</summary>
    public string Status { get; set; } = "healthy";

    /// <summary>API release tag (e.g. "v0.3.2-api-governance").</summary>
    public string Version { get; set; } = "";

    /// <summary>Human-readable uptime since the process started.</summary>
    public string Uptime { get; set; } = "";

    /// <summary>UTC timestamp of the health check.</summary>
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>Response returned by <c>GET /version</c>.</summary>
public sealed class VersionResponse
{
    /// <summary>API release tag (e.g. "v0.3.2-api-governance").</summary>
    public string Version { get; set; } = "";

    /// <summary>Friendly name of the API.</summary>
    public string Api { get; set; } = "";

    /// <summary>Short description of the API's purpose.</summary>
    public string Description { get; set; } = "";
}
