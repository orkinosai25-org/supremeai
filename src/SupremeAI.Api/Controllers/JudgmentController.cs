using Microsoft.AspNetCore.Mvc;
using SupremeAI.Api.Models;
using SupremeAI.Api.Services;

namespace SupremeAI.Api.Controllers;

/// <summary>
/// SupremeAI Judgment Engine endpoints.
///
///   POST /supreme/judge          — Run the full judgment pipeline.
///   GET  /supreme/history?n=20   — Retrieve the n most-recent judgments.
/// </summary>
[ApiController]
[Route("supreme")]
[Produces("application/json")]
public sealed class JudgmentController : ControllerBase
{
    private readonly JudgmentEngine _engine;
    private readonly JudgmentStore  _store;
    private readonly ILogger<JudgmentController> _logger;

    public JudgmentController(
        JudgmentEngine engine,
        JudgmentStore store,
        ILogger<JudgmentController> logger)
    {
        _engine = engine;
        _store  = store;
        _logger = logger;
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
            request.ModelIds.Count > 0 ? string.Join(',', request.ModelIds) : "(default)",
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
        if (n < 1 || n > 1000)
            return BadRequest(new ErrorResponse { Error = "n must be between 1 and 1000." });

        var judgments = await _store.GetRecentAsync(n, ct);
        var total     = await _store.CountAsync(ct);

        return Ok(new JudgmentHistoryResponse
        {
            Judgments = judgments,
            Total     = total,
        });
    }
}
