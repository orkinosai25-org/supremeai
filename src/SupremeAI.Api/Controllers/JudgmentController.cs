using Microsoft.AspNetCore.Mvc;
using SupremeAI.Api.Models;
using SupremeAI.Api.Services;

namespace SupremeAI.Api.Controllers;

/// <summary>
/// SupremeAI Judgment Engine endpoints.
///
///   POST /supreme/judge              — Run the full judgment pipeline.
///   GET  /supreme/history?n=20       — Retrieve the n most-recent judgments.
///   GET  /supreme/models             — List all model profiles with analytics.
///   GET  /supreme/models/{id}        — Get the profile for a single model.
///   GET  /supreme/metrics            — Get aggregated system-level metrics.
///   GET  /supreme/domains            — List all domain authority profiles.
///   GET  /supreme/domains/{domain}   — Get the authority profile for a single domain.
/// </summary>
[ApiController]
[Route("supreme")]
[Produces("application/json")]
public sealed class JudgmentController : ControllerBase
{
    private readonly JudgmentEngine _engine;
    private readonly JudgmentStore  _store;
    private readonly JudgmentAnalyticsService _analytics;
    private readonly DomainProfileRegistry _domainProfiles;
    private readonly ILogger<JudgmentController> _logger;

    public JudgmentController(
        JudgmentEngine engine,
        JudgmentStore store,
        JudgmentAnalyticsService analytics,
        DomainProfileRegistry domainProfiles,
        ILogger<JudgmentController> logger)
    {
        _engine         = engine;
        _store          = store;
        _analytics      = analytics;
        _domainProfiles = domainProfiles;
        _logger         = logger;
    }

    // ── POST /supreme/judge ───────────────────────────────────────────────────

    /// <summary>
    /// Runs the Judgment Engine:
    /// fans the prompt out to the panel models, conducts a reasoning interview,
    /// scores and ranks each response, produces a rationale, and persists the
    /// judgment for later audit/replay.
    /// </summary>
    [HttpPost("judge")]
    public async Task<IActionResult> Judge([FromBody] JudgeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest(new ErrorResponse { Error = "Prompt is required." });

        _logger.LogInformation(
            "JudgmentController: judge request — models={Models}, prompt length={Len}",
            request.ModelIds.Count > 0
                ? string.Join(',', request.ModelIds).Replace('\r', ' ').Replace('\n', ' ')
                : "(default)",
            request.Prompt.Length);

        var record = await _engine.JudgeAsync(request, ct);
        return Ok(new JudgmentResponse { Judgment = record });
    }

    // ── GET /supreme/history ──────────────────────────────────────────────────

    /// <summary>
    /// Returns the <paramref name="n"/> most-recent judgment records
    /// ordered by descending timestamp.
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> History(
        [FromQuery] int n = 20,
        CancellationToken ct = default)
    {
        if (n < 1 || n > JudgmentEngine.MaxHistoryLimit)
            return BadRequest(new ErrorResponse { Error = $"n must be between 1 and {JudgmentEngine.MaxHistoryLimit}." });

        var judgments = await _store.GetRecentAsync(n, ct);
        var total     = await _store.CountAsync(ct);

        return Ok(new JudgmentHistoryResponse
        {
            Judgments = judgments,
            Total     = total,
        });
    }

    // ── GET /supreme/models ───────────────────────────────────────────────────

    /// <summary>
    /// Returns profiles for all models that have participated in at least one
    /// judgment, ordered by descending win-rate.
    /// Each profile includes rolling statistics, strengths, weaknesses, typical
    /// failure modes, and a confidence estimate.
    /// </summary>
    [HttpGet("models")]
    public async Task<IActionResult> GetModels(CancellationToken ct = default)
    {
        _logger.LogInformation("JudgmentController: GET /supreme/models");

        var profiles = await _analytics.GetAllProfilesAsync(ct);
        return Ok(new ModelsResponse
        {
            Models = profiles,
            Total  = profiles.Count,
        });
    }

    // ── GET /supreme/models/{id} ──────────────────────────────────────────────

    /// <summary>
    /// Returns the analytics profile for the model identified by <paramref name="id"/>.
    /// Returns 404 if the model has no history.
    /// </summary>
    [HttpGet("models/{id}")]
    public async Task<IActionResult> GetModel(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new ErrorResponse { Error = "Model id must not be empty." });

        _logger.LogInformation(
            "JudgmentController: GET /supreme/models/{ModelId}",
            id.Replace('\r', ' ').Replace('\n', ' '));

        var profile = await _analytics.GetProfileAsync(id, ct);
        if (profile is null)
            return NotFound(new ErrorResponse { Error = $"No history found for model '{id}'." });

        return Ok(profile);
    }

    // ── GET /supreme/metrics ──────────────────────────────────────────────────

    /// <summary>
    /// Returns aggregated system-level metrics computed from the full judgment
    /// history: total judgments, model count, overall disagreement rate, average
    /// confidence, per-model rolling statistics, and a global average score.
    /// </summary>
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics(CancellationToken ct = default)
    {
        _logger.LogInformation("JudgmentController: GET /supreme/metrics");

        var metrics = await _analytics.GetMetricsAsync(ct);
        return Ok(new MetricsResponse { Metrics = metrics });
    }

    // ── GET /supreme/domains ──────────────────────────────────────────────────

    /// <summary>
    /// Returns all domain authority profiles that govern how SupremeAI judges
    /// acceptability per task domain (accepted source types, hallucination
    /// tolerance, creativity tolerance, evidence expectations).
    ///
    /// These profiles are the basis for T-101 confidence modulation, domain-
    /// specific reasons, and source-verification caveats.
    /// </summary>
    [HttpGet("domains")]
    public IActionResult GetDomains()
    {
        _logger.LogInformation("JudgmentController: GET /supreme/domains");

        var profiles = _domainProfiles.GetAll();
        return Ok(new DomainProfilesResponse
        {
            Profiles = [.. profiles],
            Total    = profiles.Count,
        });
    }

    // ── GET /supreme/domains/{domain} ─────────────────────────────────────────

    /// <summary>
    /// Returns the authority profile for the domain identified by
    /// <paramref name="domain"/> (e.g. "code", "research", "creative").
    /// Returns 404 if no profile is registered for that domain.
    /// </summary>
    [HttpGet("domains/{domain}")]
    public IActionResult GetDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return BadRequest(new ErrorResponse { Error = "Domain must not be empty." });

        _logger.LogInformation(
            "JudgmentController: GET /supreme/domains/{Domain}",
            domain.Replace('\r', ' ').Replace('\n', ' '));

        var profile = _domainProfiles.GetProfile(domain.ToLowerInvariant());
        if (profile is null)
            return NotFound(new ErrorResponse { Error = $"No domain authority profile found for '{domain}'." });

        return Ok(profile);
    }
}
