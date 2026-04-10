namespace SupremeAI.Api.Models;

// ── Benchmark Packs ───────────────────────────────────────────────────────────

/// <summary>
/// A single question within a benchmark pack.
/// </summary>
public sealed class BenchmarkQuestion
{
    /// <summary>Identifier for this question within its pack (e.g. "q1").</summary>
    public string Id { get; set; } = "";

    /// <summary>The prompt text sent to each model.</summary>
    public string Prompt { get; set; } = "";

    /// <summary>Category label (mirrors the pack category for mixed packs).</summary>
    public string Category { get; set; } = "";

    /// <summary>Optional descriptive tags (e.g. "multi-step", "code-generation").</summary>
    public List<string> Tags { get; set; } = [];
}

/// <summary>
/// A versioned, fixed question set for a single benchmark category.
/// Benchmark packs are immutable once published — new versions get a new
/// <see cref="Version"/> suffix so that runs remain replayable.
/// </summary>
public sealed class BenchmarkPack
{
    /// <summary>
    /// Stable, versioned identifier (e.g. "reasoning-v1", "coding-v1").
    /// Follows the pattern <c>{category}-v{n}</c>.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>Human-readable display name.</summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Category: reasoning | factual | coding | summarization | domain-specific.
    /// </summary>
    public string Category { get; set; } = "";

    /// <summary>Semantic version string (e.g. "1.0.0").</summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>Short description of what this benchmark measures.</summary>
    public string Description { get; set; } = "";

    /// <summary>The fixed, ordered question set.</summary>
    public List<BenchmarkQuestion> Questions { get; set; } = [];

    /// <summary>UTC date-time when this pack was introduced.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}

// ── Benchmark Runs ────────────────────────────────────────────────────────────

/// <summary>
/// Persisted record of a single benchmark execution.
/// Every number in the benchmark results is traceable to the
/// <see cref="JudgmentIds"/> stored here.
/// </summary>
public sealed class BenchmarkRunRecord
{
    /// <summary>Unique run identifier (UUID).</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Identifier of the benchmark pack that was executed.</summary>
    public string BenchmarkId { get; set; } = "";

    /// <summary>Models that participated in this run.</summary>
    public List<string> ModelIds { get; set; } = [];

    /// <summary>UTC timestamp when the run started.</summary>
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>UTC timestamp when the run completed (null if still in progress or failed).</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Ordered list of <see cref="JudgmentRecord.Id"/> values — one per
    /// benchmark question, in the same order as the pack's question list.
    /// </summary>
    public List<string> JudgmentIds { get; set; } = [];

    /// <summary>"completed" | "failed" | "in_progress"</summary>
    public string Status { get; set; } = "in_progress";

    /// <summary>Optional error message when Status == "failed".</summary>
    public string? ErrorMessage { get; set; }
}

// ── Benchmark Results ─────────────────────────────────────────────────────────

/// <summary>
/// Aggregate benchmark score for a single model derived from a benchmark run.
/// </summary>
public sealed class ModelBenchmarkScore
{
    /// <summary>Model identifier.</summary>
    public string ModelId { get; set; } = "";

    /// <summary>Average overall score across all questions.</summary>
    public double AvgScore { get; set; }

    /// <summary>Average Clarity sub-score.</summary>
    public double AvgClarity { get; set; }

    /// <summary>Average Reasoning sub-score.</summary>
    public double AvgReasoning { get; set; }

    /// <summary>Average Completeness sub-score.</summary>
    public double AvgCompleteness { get; set; }

    /// <summary>Average Latency bonus.</summary>
    public double AvgLatency { get; set; }

    /// <summary>Average Reasoning-Quality sub-score.</summary>
    public double AvgReasoningQuality { get; set; }

    /// <summary>Number of questions this model won.</summary>
    public int WinCount { get; set; }

    /// <summary>Fraction of questions won (0–1).</summary>
    public double WinRate { get; set; }

    /// <summary>Standard deviation of the overall score (score consistency).</summary>
    public double ScoreStdDev { get; set; }

    /// <summary>Lower bound of the 95% confidence interval for the mean score.</summary>
    public double ConfidenceLow { get; set; }

    /// <summary>Upper bound of the 95% confidence interval for the mean score.</summary>
    public double ConfidenceHigh { get; set; }

    /// <summary>Number of questions answered without error.</summary>
    public int AnsweredCount { get; set; }

    /// <summary>Number of questions that resulted in an error.</summary>
    public int ErrorCount { get; set; }
}

/// <summary>
/// Pairwise disagreement cell: how much model A and model B disagreed on a
/// specific question, measured as the absolute score delta.
/// </summary>
public sealed class DisagreementCell
{
    /// <summary>Question ID this cell refers to.</summary>
    public string QuestionId { get; set; } = "";

    /// <summary>First model.</summary>
    public string ModelA { get; set; } = "";

    /// <summary>Second model.</summary>
    public string ModelB { get; set; } = "";

    /// <summary>
    /// Absolute score difference between ModelA and ModelB for this question.
    /// Higher values indicate stronger disagreement.
    /// </summary>
    public double ScoreDelta { get; set; }
}

/// <summary>
/// Full results object for a benchmark run.
/// All numeric values are derived from <see cref="JudgmentRecord"/> objects
/// persisted in <see cref="JudgmentStore"/>.
/// </summary>
public sealed class BenchmarkResults
{
    /// <summary>Benchmark pack identifier.</summary>
    public string BenchmarkId { get; set; } = "";

    /// <summary>Run record identifier.</summary>
    public string RunId { get; set; } = "";

    /// <summary>
    /// Per-model leaderboard ordered by descending <see cref="ModelBenchmarkScore.AvgScore"/>.
    /// </summary>
    public List<ModelBenchmarkScore> Leaderboard { get; set; } = [];

    /// <summary>
    /// Pairwise disagreement heatmap: one cell per (question, modelA, modelB) triple
    /// where the score delta exceeds zero.
    /// </summary>
    public List<DisagreementCell> DisagreementHeatmap { get; set; } = [];

    /// <summary>Total number of questions in the benchmark.</summary>
    public int TotalQuestions { get; set; }

    /// <summary>Number of distinct models evaluated.</summary>
    public int ModelsEvaluated { get; set; }

    /// <summary>UTC timestamp when the benchmark run was started.</summary>
    public DateTimeOffset RunAt { get; set; }

    /// <summary>UTC timestamp when these results were computed.</summary>
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}

// ── API Response types ────────────────────────────────────────────────────────

/// <summary>Response body for GET /supreme/benchmarks.</summary>
public sealed class BenchmarkListResponse
{
    /// <summary>All available benchmark packs.</summary>
    public List<BenchmarkPack> Benchmarks { get; set; } = [];

    /// <summary>Total number of available benchmark packs.</summary>
    public int Total { get; set; }
}

/// <summary>Response body for POST /supreme/benchmarks/{id}/run.</summary>
public sealed class BenchmarkRunResponse
{
    /// <summary>The persisted run record.</summary>
    public BenchmarkRunRecord Run { get; set; } = new();
}

/// <summary>Response body for GET /supreme/benchmarks/{id}/results.</summary>
public sealed class BenchmarkResultsResponse
{
    /// <summary>The computed benchmark results.</summary>
    public BenchmarkResults Results { get; set; } = new();
}

/// <summary>Optional request body for POST /supreme/benchmarks/{id}/run.</summary>
public sealed class BenchmarkRunRequest
{
    /// <summary>
    /// Model IDs to include in the judgment panel.
    /// Leave empty to use the default panel (gpt-4o, llama-3-1-70b, mistral-large).
    /// </summary>
    public List<string> ModelIds { get; set; } = [];
}
