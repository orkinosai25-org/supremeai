using Microsoft.AspNetCore.Mvc;
using SupremeAI.Api.Middleware;
using SupremeAI.Api.Models;

namespace SupremeAI.Api.Controllers;

/// <summary>
/// API governance endpoints — health, liveness, and version metadata.
///
///   GET /health   — Liveness probe; returns status, version, and uptime.
///   GET /version  — Returns the current API release tag and build description.
/// </summary>
[ApiController]
[Produces("application/json")]
public sealed class GovernanceController : ControllerBase
{
    private static readonly DateTimeOffset _startTime = DateTimeOffset.UtcNow;

    private readonly ILogger<GovernanceController> _logger;

    public GovernanceController(ILogger<GovernanceController> logger)
    {
        _logger = logger;
    }

    // ── GET /health ───────────────────────────────────────────────────────────

    /// <summary>
    /// Liveness probe.
    /// Returns the API health status, current release version, and process uptime.
    /// Suitable for use as a Kubernetes liveness or readiness probe target.
    /// </summary>
    [HttpGet("/health")]
    public IActionResult Health()
    {
        var uptime = DateTimeOffset.UtcNow - _startTime;

        _logger.LogInformation("Health check requested.");

        return Ok(new HealthResponse
        {
            Status    = "healthy",
            Version   = GovernanceMiddleware.ApiVersion,
            Uptime    = uptime.ToString(@"d\.hh\:mm\:ss"),
            Timestamp = DateTimeOffset.UtcNow,
        });
    }

    // ── GET /version ──────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the current API release tag and a short description of the platform.
    /// Clients can use this to confirm which API governance release they are talking to.
    /// </summary>
    [HttpGet("/version")]
    public IActionResult Version()
    {
        return Ok(new VersionResponse
        {
            Version     = GovernanceMiddleware.ApiVersion,
            Api         = "SupremeAI API",
            Description =
                "Judgment, Benchmarking, API Governance, and Blazor WebAssembly UI layer for SupremeAI. " +
                "Evaluates multiple AI models, estimates confidence, and provides " +
                "explainable, auditable decisions surfaced through the first-generation UI.",
        });
    }
}
