using System.Diagnostics;
using System.Text;
using SupremeAI.Api.Models;

namespace SupremeAI.Api.Services;

/// <summary>
/// The SupremeAI Judgment Engine — transforms a prompt into a structured
/// <see cref="JudgmentRecord"/> by:
///
///   1. Fanning the prompt out to all panel models in parallel.
///   2. Conducting a "Reasoning Interview" — asking each model to briefly
///      explain its reasoning (a second, lightweight call per model).
///   3. Scoring every response across three primary criteria plus a latency
///      bonus and a reasoning-quality signal.
///   4. Generating a human-readable <em>rationale</em> that names the winner,
///      explains why it beat the runners-up, and highlights the criteria that
///      mattered most.
///   5. Persisting the judgment via <see cref="JudgmentStore"/> for audit/replay.
///
/// No model training or fine-tuning is required — this is pure inference
/// plus meta-evaluation.
/// </summary>
public sealed class JudgmentEngine
{
    private static readonly string[] DefaultModelIds = ["gpt-4o", "llama-3-1-70b", "mistral-large"];

    // ── Scoring weights ────────────────────────────────────────────────────────
    private const double MaxClarityScore          = 3.0;
    private const double MaxReasoningScore        = 3.0;
    private const double ReasoningScorePerKeyword = 0.5;
    private const double MaxCompletenessScore     = 3.0;
    private const double MaxLatencyBonus          = 1.0;
    private const double LatencyBonusThresholdMs  = 3000.0;
    private const double MaxReasoningQuality      = 1.0;
    private const int    MinimumLengthThreshold   = 80;

    // ── Clarity sub-weights (sum to MaxClarityScore = 3.0) ───────────────────
    /// <summary>Bonus for multi-line responses (presence of newlines).</summary>
    private const double ClarityNewlineBonus    = 0.5;
    /// <summary>Bonus for list formatting (numbered, bullet, or dash lists).</summary>
    private const double ClarityListBonus       = 0.75;
    /// <summary>Bonus for markdown emphasis/headers.</summary>
    private const double ClarityMarkdownBonus   = 0.75;
    /// <summary>Bonus for colon usage (definitions, enumerations).</summary>
    private const double ClarityColonBonus      = 0.5;
    /// <summary>Bonus for fenced code blocks.</summary>
    private const double ClarityCodeBlockBonus  = 0.5;

    // ── Completeness sub-weights ─────────────────────────────────────────────
    /// <summary>Characters-per-completeness-point: 300 chars → 1 completeness point.</summary>
    private const double LengthScalingFactor       = 300.0;
    /// <summary>Sentence-ends-per-completeness-point: 5 sentences → 1 completeness point.</summary>
    private const double SentenceCoverageScale     = 0.2;

    // ── Reasoning-interview quality thresholds ────────────────────────────────
    /// <summary>Minimum character length for a reasoning response to receive a length bonus.</summary>
    private const int    MinReasoningLength    = 50;
    /// <summary>Score awarded when the reasoning response meets the minimum length.</summary>
    private const double ReasoningLengthScore  = 0.5;
    /// <summary>Score awarded when reasoning contains logical connective keywords.</summary>
    private const double ReasoningKeywordScore = 0.5;

    /// <summary>Maximum number of recent judgments that can be requested via the history endpoint.</summary>
    internal const int MaxHistoryLimit = 1000;

    private static readonly string[] ReasoningKeywords =
    [
        "because", "therefore", "however", "although", "thus", "hence",
        "since", "as a result", "in conclusion", "furthermore", "moreover",
        "consequently", "this means", "which means", "it follows",
    ];

    private readonly ModelProviderFactory _factory;
    private readonly JudgmentStore _store;
    private readonly ILogger<JudgmentEngine> _logger;

    public JudgmentEngine(
        ModelProviderFactory factory,
        JudgmentStore store,
        ILogger<JudgmentEngine> logger)
    {
        _factory = factory;
        _store   = store;
        _logger  = logger;
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Runs the full judgment pipeline and returns a persisted
    /// <see cref="JudgmentRecord"/>.
    /// </summary>
    public async Task<JudgmentRecord> JudgeAsync(JudgeRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            throw new ArgumentException("Prompt must not be empty.", nameof(request));

        var modelIds = request.ModelIds.Count > 0
            ? request.ModelIds
            : [.. DefaultModelIds];

        _logger.LogInformation(
            "JudgmentEngine: starting judgment — {Count} model(s), prompt length={Len}",
            modelIds.Count, request.Prompt.Length);

        // ── 1. Fan out — collect main answers in parallel ─────────────────────
        var answerTasks = modelIds
            .Select(id => GetAnswerAsync(id, request, ct))
            .ToArray();

        var answers = await Task.WhenAll(answerTasks);

        // ── 2. Reasoning Interview — one follow-up call per successful model ──
        var interviewTasks = answers
            .Select(r => ConductReasoningInterviewAsync(r, request, ct))
            .ToArray();

        var results = await Task.WhenAll(interviewTasks);

        // ── 3. Score each result ───────────────────────────────────────────────
        foreach (var r in results)
        {
            r.ScoreBreakdown = ComputeScoreBreakdown(r);
            r.Score          = Math.Round(
                r.ScoreBreakdown.Clarity
              + r.ScoreBreakdown.Reasoning
              + r.ScoreBreakdown.Completeness
              + r.ScoreBreakdown.Latency
              + r.ScoreBreakdown.ReasoningQuality,
                2);
        }

        // ── 4. Rank and pick winner ────────────────────────────────────────────
        var ranked = results
            .OrderByDescending(r => r.Score)
            .ToList();

        var winner = ranked.FirstOrDefault(r => r.Status == "done") ?? ranked[0];

        // ── 5. Build rationale ────────────────────────────────────────────────
        var rationale = BuildRationale(winner, ranked);

        _logger.LogInformation(
            "JudgmentEngine: winner='{WinnerId}' score={Score}",
            Sanitize(winner.ModelId), winner.Score);

        // ── 6. Assemble and persist record ─────────────────────────────────────
        var record = new JudgmentRecord
        {
            Prompt       = request.Prompt,
            ModelResults = ranked,
            WinnerId     = winner.ModelId,
            WinnerAnswer = winner.Answer,
            Rationale    = rationale,
            Timestamp    = DateTimeOffset.UtcNow,
        };

        await _store.SaveAsync(record, ct);
        return record;
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    /// <summary>Calls a single model with the user prompt and returns a partial result.</summary>
    private async Task<ModelJudgmentResult> GetAnswerAsync(
        string modelId,
        JudgeRequest request,
        CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var chatRequest = new ChatRequest
            {
                ModelId     = modelId,
                Messages    = [new ChatMessage { Role = "user", Content = request.Prompt }],
                MaxTokens   = request.MaxTokens,
                Temperature = request.Temperature,
            };

            var response = await _factory.ChatAsync(chatRequest, ct);
            sw.Stop();

            return new ModelJudgmentResult
            {
                ModelId      = modelId,
                Answer       = response.Text,
                Status       = response.Status,
                Ms           = (int)sw.ElapsedMilliseconds,
                Tokens       = response.Tokens,
                ErrorMessage = response.ErrorMessage,
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "JudgmentEngine: error calling model '{ModelId}'", Sanitize(modelId));
            return new ModelJudgmentResult
            {
                ModelId      = modelId,
                Status       = "error",
                Ms           = (int)sw.ElapsedMilliseconds,
                ErrorMessage = ex.Message,
            };
        }
    }

    /// <summary>
    /// Conducts the Reasoning Interview: asks the model to briefly explain its
    /// reasoning using a follow-up message appended to the conversation.
    /// The reasoning text is stored in <see cref="ModelJudgmentResult.Reasoning"/>.
    /// </summary>
    private async Task<ModelJudgmentResult> ConductReasoningInterviewAsync(
        ModelJudgmentResult result,
        JudgeRequest request,
        CancellationToken ct)
    {
        // Only interview models that answered successfully.
        if (result.Status != "done" || string.IsNullOrWhiteSpace(result.Answer))
            return result;

        try
        {
            var chatRequest = new ChatRequest
            {
                ModelId  = result.ModelId,
                MaxTokens   = request.MaxTokens > 0 ? Math.Min(request.MaxTokens, 256) : 256,
                Temperature = request.Temperature,
                Messages =
                [
                    new ChatMessage { Role = "user",      Content = request.Prompt  },
                    new ChatMessage { Role = "assistant", Content = result.Answer   },
                    new ChatMessage { Role = "user",      Content = "Briefly explain your reasoning." },
                ],
            };

            var response = await _factory.ChatAsync(chatRequest, ct);
            result.Reasoning  = response.Text;
            result.Tokens    += response.Tokens;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "JudgmentEngine: reasoning interview failed for model '{ModelId}'", Sanitize(result.ModelId));
            // Non-fatal — we carry on without the reasoning text.
        }

        return result;
    }

    /// <summary>
    /// Computes the detailed <see cref="ScoreBreakdown"/> for a single model result.
    /// </summary>
    internal static ScoreBreakdown ComputeScoreBreakdown(ModelJudgmentResult result)
    {
        var breakdown = new ScoreBreakdown();

        if (result.Status != "done" || string.IsNullOrWhiteSpace(result.Answer))
            return breakdown;

        var text = result.Answer;

        // ── Clarity (0–MaxClarityScore): structural formatting signals ────────
        double clarity = 0;
        if (text.Contains('\n'))                                                  clarity += ClarityNewlineBonus;
        if (text.Contains("1.") || text.Contains("•") || text.Contains("- "))    clarity += ClarityListBonus;
        if (text.Contains("**") || text.Contains("###") || text.Contains("##"))  clarity += ClarityMarkdownBonus;
        if (text.Contains(':'))                                                   clarity += ClarityColonBonus;
        if (text.Contains("```"))                                                 clarity += ClarityCodeBlockBonus;
        breakdown.Clarity = Math.Round(Math.Min(MaxClarityScore, clarity), 2);

        // ── Reasoning (0–MaxReasoningScore): logical connectives ─────────────
        var keywordHits = ReasoningKeywords
            .Count(w => text.Contains(w, StringComparison.OrdinalIgnoreCase));
        breakdown.Reasoning = Math.Round(
            Math.Min(MaxReasoningScore, keywordHits * ReasoningScorePerKeyword), 2);

        // ── Completeness (0–MaxCompletenessScore): length + sentence coverage ─
        double completeness = Math.Min(MaxCompletenessScore / 2, text.Length / LengthScalingFactor);
        var sentenceEnds    = text.Count(c => c == '.' || c == '!' || c == '?');
        completeness       += Math.Min(MaxCompletenessScore / 2, sentenceEnds * SentenceCoverageScale);
        if (text.Length < MinimumLengthThreshold) completeness *= 0.5;
        breakdown.Completeness = Math.Round(completeness, 2);

        // ── Latency bonus (0–MaxLatencyBonus) ────────────────────────────────
        if (result.Ms > 0)
            breakdown.Latency = Math.Round(
                Math.Max(0.0, MaxLatencyBonus - result.Ms / LatencyBonusThresholdMs), 2);

        // ── Reasoning-interview quality (0–MaxReasoningQuality) ──────────────
        if (!string.IsNullOrWhiteSpace(result.Reasoning))
        {
            var rq = 0.0;
            var reasoningText = result.Reasoning;
            if (reasoningText.Length >= MinReasoningLength)  rq += ReasoningLengthScore;
            if (ReasoningKeywords.Any(w =>
                    reasoningText.Contains(w, StringComparison.OrdinalIgnoreCase)))
                rq += ReasoningKeywordScore;
            breakdown.ReasoningQuality = Math.Round(Math.Min(MaxReasoningQuality, rq), 2);
        }

        return breakdown;
    }

    /// <summary>
    /// Builds a human-readable rationale explaining why the winner beat the others
    /// and which scoring criteria mattered most.
    /// </summary>
    internal static string BuildRationale(
        ModelJudgmentResult winner,
        IReadOnlyList<ModelJudgmentResult> ranked)
    {
        if (winner.Status != "done")
            return $"All models failed to produce a valid answer. The highest-ranked result was '{winner.ModelId}' but it ended in an error.";

        var sb = new StringBuilder();

        sb.Append($"**{winner.ModelId}** was selected as the Supreme Answer");

        // Mention runner-up comparison if there is one
        var runnerUp = ranked.FirstOrDefault(r => r.ModelId != winner.ModelId && r.Status == "done");
        if (runnerUp is not null)
        {
            var delta = winner.Score - runnerUp.Score;
            sb.Append($" over **{runnerUp.ModelId}** (margin: +{delta:F2} pts)");
        }

        sb.AppendLine(".");
        sb.AppendLine();

        // Criteria comparison
        var bd = winner.ScoreBreakdown;
        sb.AppendLine("**Criteria breakdown for the winner:**");
        sb.AppendLine($"- Clarity: {bd.Clarity:F2} / 3.0");
        sb.AppendLine($"- Reasoning: {bd.Reasoning:F2} / 3.0");
        sb.AppendLine($"- Completeness: {bd.Completeness:F2} / 3.0");
        sb.AppendLine($"- Latency bonus: {bd.Latency:F2} / 1.0");
        sb.AppendLine($"- Reasoning-interview quality: {bd.ReasoningQuality:F2} / 1.0");
        sb.AppendLine();

        // Identify the top criterion
        var criteria = new Dictionary<string, double>
        {
            { "Clarity",            bd.Clarity            },
            { "Reasoning",          bd.Reasoning          },
            { "Completeness",       bd.Completeness       },
            { "Latency",            bd.Latency            },
            { "Reasoning quality",  bd.ReasoningQuality   },
        };
        var topCriterion = criteria.MaxBy(kv => kv.Value);
        sb.AppendLine($"**Most decisive criterion:** {topCriterion.Key} ({topCriterion.Value:F2} pts).");

        // Brief comparison table if more than one model responded
        var done = ranked.Where(r => r.Status == "done").ToList();
        if (done.Count > 1)
        {
            sb.AppendLine();
            sb.AppendLine("**Model comparison:**");
            foreach (var r in done)
            {
                var tag = r.ModelId == winner.ModelId ? " 🏆" : "";
                sb.AppendLine($"- {r.ModelId}{tag}: {r.Score:F2} pts " +
                              $"(clarity={r.ScoreBreakdown.Clarity:F2}, " +
                              $"reasoning={r.ScoreBreakdown.Reasoning:F2}, " +
                              $"completeness={r.ScoreBreakdown.Completeness:F2})");
            }
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Strips newline characters from a user-supplied value before it is written
    /// to application logs, preventing log-injection attacks.
    /// </summary>
    private static string Sanitize(string value) =>
        value.Replace('\r', ' ').Replace('\n', ' ');
}
