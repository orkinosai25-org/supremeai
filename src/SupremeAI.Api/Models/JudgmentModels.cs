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
