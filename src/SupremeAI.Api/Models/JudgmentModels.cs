namespace SupremeAI.Api.Models;

// ── Judgment Engine – Request / Response / Record models ─────────────────────

/// <summary>
/// Request body for POST /supreme/judge.
/// </summary>
public sealed class JudgeRequest
{
    /// <summary>The prompt to evaluate across all selected models.</summary>
    public string Prompt { get; set; } = "";

    /// <summary>
    /// Model IDs to include in the judgment panel.
    /// Leave empty to use the default panel (gpt-4o, llama-3-1-70b, mistral-large).
    /// </summary>
    public List<string> ModelIds { get; set; } = [];

    /// <summary>Maximum tokens per model response (0 = provider default).</summary>
    public int MaxTokens { get; set; } = 0;

    /// <summary>Temperature 0–2 (0 = provider default).</summary>
    public double Temperature { get; set; } = 0;
}

/// <summary>
/// Breakdown of scores across the three primary evaluation criteria.
/// Each criterion is scored on a 0–3 scale; Latency is a 0–1 bonus.
/// </summary>
public sealed class ScoreBreakdown
{
    /// <summary>
    /// Clarity: structural quality — headers, lists, code blocks, colons.
    /// Max 3 points.
    /// </summary>
    public double Clarity { get; set; }

    /// <summary>
    /// Reasoning: presence of logical connectives in the main answer.
    /// Max 3 points.
    /// </summary>
    public double Reasoning { get; set; }

    /// <summary>
    /// Completeness: richness based on length and sentence coverage.
    /// Max 3 points.
    /// </summary>
    public double Completeness { get; set; }

    /// <summary>
    /// Latency bonus: responses under 3 s receive up to 1 extra point.
    /// </summary>
    public double Latency { get; set; }

    /// <summary>
    /// Reasoning-interview quality: richness of the model's self-explanation.
    /// Max 1 point.
    /// </summary>
    public double ReasoningQuality { get; set; }
}

/// <summary>Per-model result produced by the Judgment Engine.</summary>
public sealed class ModelJudgmentResult
{
    /// <summary>Model that produced this result.</summary>
    public string ModelId { get; set; } = "";

    /// <summary>Main answer to the prompt.</summary>
    public string Answer { get; set; } = "";

    /// <summary>Self-explanation returned by the model when asked to explain its reasoning.</summary>
    public string Reasoning { get; set; } = "";

    /// <summary>"done" | "error"</summary>
    public string Status { get; set; } = "done";

    /// <summary>Latency for the main answer in milliseconds.</summary>
    public int Ms { get; set; }

    /// <summary>Total tokens used (answer + reasoning interview).</summary>
    public int Tokens { get; set; }

    /// <summary>Overall quality score (sum of ScoreBreakdown fields).</summary>
    public double Score { get; set; }

    /// <summary>Per-criterion score breakdown.</summary>
    public ScoreBreakdown ScoreBreakdown { get; set; } = new();

    /// <summary>Error detail when Status == "error".</summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Immutable judgment record stored for audit / replay.
/// </summary>
public sealed class JudgmentRecord
{
    /// <summary>Unique identifier for this judgment (UUID).</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Original user prompt.</summary>
    public string Prompt { get; set; } = "";

    /// <summary>Per-model results, ordered by descending score.</summary>
    public List<ModelJudgmentResult> ModelResults { get; set; } = [];

    /// <summary>Model ID of the winner.</summary>
    public string WinnerId { get; set; } = "";

    /// <summary>The winning answer.</summary>
    public string WinnerAnswer { get; set; } = "";

    /// <summary>
    /// Human-readable rationale explaining why the winner beat the runners-up
    /// and which scoring criteria mattered most.
    /// </summary>
    public string Rationale { get; set; } = "";

    /// <summary>UTC timestamp when this judgment was produced.</summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>Response body for POST /supreme/judge.</summary>
public sealed class JudgmentResponse
{
    /// <summary>The full judgment record.</summary>
    public JudgmentRecord Judgment { get; set; } = new();
}

/// <summary>Response body for GET /supreme/history.</summary>
public sealed class JudgmentHistoryResponse
{
    /// <summary>Judgments ordered by most recent first.</summary>
    public List<JudgmentRecord> Judgments { get; set; } = [];

    /// <summary>Total number of judgments stored.</summary>
    public int Total { get; set; }
}

// ── Intelligence Layer – Analytics Models ─────────────────────────────────────

/// <summary>
/// Rolling statistics for a single model derived from all historical judgments.
/// All values are deterministic aggregates — no ML involved.
/// </summary>
public sealed class ModelStats
{
    /// <summary>Model this statistics record belongs to.</summary>
    public string ModelId { get; set; } = "";

    /// <summary>Number of judgments this model participated in.</summary>
    public int TotalJudgments { get; set; }

    /// <summary>Number of judgments this model won.</summary>
    public int Wins { get; set; }

    /// <summary>Fraction of participated judgments that this model won (0–1).</summary>
    public double WinRate { get; set; }

    /// <summary>Mean overall score across all participated judgments.</summary>
    public double AvgScore { get; set; }

    /// <summary>Mean Clarity sub-score across all participated judgments.</summary>
    public double AvgClarity { get; set; }

    /// <summary>Mean Reasoning sub-score across all participated judgments.</summary>
    public double AvgReasoning { get; set; }

    /// <summary>Mean Completeness sub-score across all participated judgments.</summary>
    public double AvgCompleteness { get; set; }

    /// <summary>Mean Latency bonus across all participated judgments.</summary>
    public double AvgLatency { get; set; }

    /// <summary>Mean Reasoning-Quality sub-score across all participated judgments.</summary>
    public double AvgReasoningQuality { get; set; }

    /// <summary>
    /// Standard deviation of the overall score (measure of score volatility).
    /// A higher value indicates inconsistent performance.
    /// </summary>
    public double ScoreVolatility { get; set; }

    /// <summary>
    /// Fraction of judgments where this model was NOT ranked first
    /// despite having the highest historical average score
    /// (proxy for panel disagreement involving this model).
    /// </summary>
    public double DisagreementRate { get; set; }

    /// <summary>Number of judgments where the model returned an error status.</summary>
    public int ErrorCount { get; set; }
}

/// <summary>
/// Derived intelligence profile for a model — strengths, weaknesses and
/// typical failure modes inferred from historical judgment data.
/// </summary>
public sealed class ModelProfile
{
    /// <summary>Model identifier.</summary>
    public string ModelId { get; set; } = "";

    /// <summary>Rolling statistics that underpin this profile.</summary>
    public ModelStats Stats { get; set; } = new();

    /// <summary>
    /// Criteria where this model scores above the panel average
    /// (deterministically computed from historical averages).
    /// </summary>
    public List<string> Strengths { get; set; } = [];

    /// <summary>
    /// Criteria where this model scores below the panel average.
    /// </summary>
    public List<string> Weaknesses { get; set; } = [];

    /// <summary>
    /// Human-readable descriptions of observed failure patterns,
    /// derived from scoring thresholds and error rates.
    /// </summary>
    public List<string> TypicalFailureModes { get; set; } = [];

    /// <summary>
    /// Estimated confidence in this model's future judgments based on
    /// historical consistency and score gap analysis.
    /// </summary>
    public ConfidenceInfo Confidence { get; set; } = new();
}

/// <summary>
/// Deterministic confidence estimate for a model's judgment quality.
/// No ML fitting — all values derive from historical statistics.
/// </summary>
public sealed class ConfidenceInfo
{
    /// <summary>Composite confidence score (0–1).</summary>
    public double Score { get; set; }

    /// <summary>
    /// Average gap between this model's score and the runner-up across
    /// won judgments.  Larger gap → higher confidence.
    /// </summary>
    public double AvgWinMargin { get; set; }

    /// <summary>
    /// Measure of how consistently this model ranks similarly across
    /// judgments (1 − normalised score volatility).
    /// </summary>
    public double HistoricalConsistency { get; set; }

    /// <summary>
    /// Fraction of judgments where this model agreed with the majority
    /// panel ranking (used as a proxy for inter-model agreement).
    /// </summary>
    public double PanelAgreementRate { get; set; }

    /// <summary>Plain-English explanation of how the confidence was derived.</summary>
    public string Explanation { get; set; } = "";
}

/// <summary>
/// Overall system-level metrics aggregated across all historical judgments
/// and all models.
/// </summary>
public sealed class JudgmentMetrics
{
    /// <summary>Total judgments stored.</summary>
    public int TotalJudgments { get; set; }

    /// <summary>Number of distinct models that have participated in at least one judgment.</summary>
    public int TotalModels { get; set; }

    /// <summary>
    /// Overall disagreement rate: fraction of judgments where the winner
    /// did not have the highest average historical score at the time.
    /// </summary>
    public double OverallDisagreementRate { get; set; }

    /// <summary>Average composite confidence across all models.</summary>
    public double AvgConfidence { get; set; }

    /// <summary>Per-model rolling statistics.</summary>
    public List<ModelStats> ModelStats { get; set; } = [];

    /// <summary>ID of the model with the highest historical win-rate.</summary>
    public string TopModel { get; set; } = "";

    /// <summary>Average overall score across all judgments and all models.</summary>
    public double GlobalAvgScore { get; set; }
}

/// <summary>Response body for GET /supreme/models.</summary>
public sealed class ModelsResponse
{
    /// <summary>Profiles for every model that has participated in at least one judgment.</summary>
    public List<ModelProfile> Models { get; set; } = [];

    /// <summary>Total number of distinct models.</summary>
    public int Total { get; set; }
}

/// <summary>Response body for GET /supreme/metrics.</summary>
public sealed class MetricsResponse
{
    /// <summary>Aggregated system metrics.</summary>
    public JudgmentMetrics Metrics { get; set; } = new();
}
