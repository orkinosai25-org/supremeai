using Microsoft.AspNetCore.Mvc;
using SupremeAI.Api.Models;
using SupremeAI.Api.Services;

namespace SupremeAI.Api.Controllers;

/// <summary>
/// SupremeAI Benchmark &amp; Publishing endpoints.
///
///   GET  /supreme/benchmarks               — List all available benchmark packs.
///   GET  /supreme/benchmarks/{id}          — Get a specific benchmark pack.
///   GET  /supreme/benchmarks/{id}/results  — Get results for the latest completed run.
///   POST /supreme/benchmarks/{id}/run      — Execute a benchmark run.
/// </summary>
[ApiController]
[Route("supreme/benchmarks")]
[Produces("application/json")]
public sealed class BenchmarkController : ControllerBase
{
    private readonly BenchmarkService _benchmarkService;
    private readonly ILogger<BenchmarkController> _logger;

    public BenchmarkController(
        BenchmarkService benchmarkService,
        ILogger<BenchmarkController> logger)
    {
        _benchmarkService = benchmarkService;
        _logger           = logger;
    }

    // ── GET /supreme/benchmarks ───────────────────────────────────────────────

    /// <summary>
    /// Returns all available benchmark packs with their metadata and question sets.
    /// </summary>
    [HttpGet]
    public IActionResult GetBenchmarks()
    {
        _logger.LogInformation("BenchmarkController: GET /supreme/benchmarks");

        var packs = _benchmarkService.GetAllPacks();
        return Ok(new BenchmarkListResponse
        {
            Benchmarks = [.. packs],
            Total      = packs.Count,
        });
    }

    // ── GET /supreme/benchmarks/{id} ─────────────────────────────────────────

    /// <summary>
    /// Returns the benchmark pack identified by <paramref name="id"/>.
    /// Returns 404 when the pack does not exist.
    /// </summary>
    [HttpGet("{id}")]
    public IActionResult GetBenchmark(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new ErrorResponse { Error = "Benchmark id must not be empty." });

        _logger.LogInformation(
            "BenchmarkController: GET /supreme/benchmarks/{BenchmarkId}",
            id.Replace('\r', ' ').Replace('\n', ' '));

        var pack = _benchmarkService.GetPack(id);
        if (pack is null)
            return NotFound(new ErrorResponse { Error = $"No benchmark found with id '{id}'." });

        return Ok(pack);
    }

    // ── GET /supreme/benchmarks/{id}/results ─────────────────────────────────

    /// <summary>
    /// Returns the results for the most recent completed run of the benchmark
    /// identified by <paramref name="id"/>.
    ///
    /// Use <c>?format=markdown</c> to receive a Markdown summary instead of JSON.
    ///
    /// Returns 404 when the benchmark does not exist or has no completed run.
    /// </summary>
    [HttpGet("{id}/results")]
    public async Task<IActionResult> GetResults(
        string id,
        [FromQuery] string? format = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new ErrorResponse { Error = "Benchmark id must not be empty." });

        _logger.LogInformation(
            "BenchmarkController: GET /supreme/benchmarks/{BenchmarkId}/results",
            id.Replace('\r', ' ').Replace('\n', ' '));

        var pack = _benchmarkService.GetPack(id);
        if (pack is null)
            return NotFound(new ErrorResponse { Error = $"No benchmark found with id '{id}'." });

        var results = await _benchmarkService.GetResultsAsync(id, ct);
        if (results is null)
            return NotFound(new ErrorResponse
            {
                Error = $"No completed benchmark run found for '{id}'. " +
                        $"Use POST /supreme/benchmarks/{id}/run to execute the benchmark first.",
            });

        // ── Markdown export ───────────────────────────────────────────────────
        if (string.Equals(format, "markdown", StringComparison.OrdinalIgnoreCase))
        {
            var markdown = _benchmarkService.ToMarkdown(results, pack);
            return Content(markdown, "text/markdown");
        }

        // ── JSON export (default) ─────────────────────────────────────────────
        return Ok(new BenchmarkResultsResponse { Results = results });
    }

    // ── POST /supreme/benchmarks/{id}/run ────────────────────────────────────

    /// <summary>
    /// Executes a benchmark run for the pack identified by <paramref name="id"/>.
    ///
    /// This fans each question through the Judgment Engine and stores the
    /// resulting judgments.  The <c>modelIds</c> body parameter overrides the
    /// default model panel.
    ///
    /// <b>Warning:</b> this can take several minutes for large packs.
    /// </summary>
    [HttpPost("{id}/run")]
    public async Task<IActionResult> RunBenchmark(
        string id,
        [FromBody] BenchmarkRunRequest? request = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new ErrorResponse { Error = "Benchmark id must not be empty." });

        _logger.LogInformation(
            "BenchmarkController: POST /supreme/benchmarks/{BenchmarkId}/run",
            id.Replace('\r', ' ').Replace('\n', ' '));

        if (_benchmarkService.GetPack(id) is null)
            return NotFound(new ErrorResponse { Error = $"No benchmark found with id '{id}'." });

        var modelIds = request?.ModelIds ?? [];

        BenchmarkRunRecord run;
        try
        {
            run = await _benchmarkService.RunAsync(id, modelIds, ct);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }

        return Ok(new BenchmarkRunResponse { Run = run });
    }
}
