using SupremeAI.Api.Models;

namespace SupremeAI.Api.Services;

/// <summary>
/// The Judgment Intelligence Layer — derives deterministic analytics from the
/// historical judgment records persisted by <see cref="JudgmentStore"/>.
///
/// All computations are pure aggregations with no ML training or model fitting.
/// Every derived value is directly explainable from the underlying data.
///
/// Provides:
///   • Rolling statistics per model (win-rate, avg per criterion, volatility, disagreement rate)
///   • Model profiles (strengths, weaknesses, typical failure modes)
///   • Confidence estimation (score gap, inter-model agreement, historical consistency)
///   • System-level metrics
/// </summary>
public sealed class JudgmentAnalyticsService
{
    // ── Profile derivation thresholds ─────────────────────────────────────────

    /// <summary>
    /// A criterion is a "strength" when the model's average for that criterion
    /// is at least this many points above the cross-model panel average.
    /// </summary>
    private const double StrengthDeltaThreshold = 0.15;

    /// <summary>
    /// A criterion is a "weakness" when the model's average for that criterion
    /// is at least this many points below the cross-model panel average.
    /// </summary>
    private const double WeaknessDeltaThreshold = 0.15;

    /// <summary>
    /// Maximum possible score (sum of all criterion maximums: 3+3+3+1+1 = 11).
    /// Used to normalise volatility into a 0–1 range.
    /// </summary>
    private const double MaxPossibleScore = 11.0;

    /// <summary>
    /// Error rate above which "frequent errors" is listed as a failure mode.
    /// </summary>
    private const double HighErrorRateThreshold = 0.1;

    /// <summary>
    /// Criterion average below which "low &lt;criterion&gt;" is listed as a failure mode.
    /// </summary>
    private const double LowCriterionThreshold = 0.5;

    /// <summary>
    /// Win margin below which the model is considered to "rarely win by a clear margin".
    /// </summary>
    private const double LowWinMarginThreshold = 0.3;

    /// <summary>
    /// Score volatility threshold (as a fraction of <see cref="MaxPossibleScore"/>)
    /// above which "high score volatility" is listed as a failure mode.
    /// </summary>
    private const double HighVolatilityFraction = 0.15;

    /// <summary>
    /// Win-rate below which a model with enough history is flagged as "rarely wins".
    /// </summary>
    private const double LowWinRateThreshold = 0.1;

    /// <summary>
    /// Minimum number of judgments before win-rate based failure modes are reported.
    /// </summary>
    private const int MinJudgmentsForWinRateAnalysis = 5;

    /// <summary>
    /// Disagreement rate above which the model is flagged for panel disagreement.
    /// </summary>
    private const double HighDisagreementRateThreshold = 0.5;

    /// <summary>
    /// Half of the maximum possible score; used to normalise win-margin to 0–1.
    /// A margin equal to this value maps to a normalised margin of 1.0.
    /// </summary>
    private const double MaxMeaningfulMarginDivisor = MaxPossibleScore / 2.0;

    // ── Confidence weights (must sum to 1.0) ─────────────────────────────────

    /// <summary>Weight of historical consistency in the composite confidence score.</summary>
    private const double ConsistencyWeight = 0.40;

    /// <summary>Weight of average win margin in the composite confidence score.</summary>
    private const double WinMarginWeight = 0.30;

    /// <summary>Weight of panel agreement rate in the composite confidence score.</summary>
    private const double AgreementWeight = 0.30;

    private readonly JudgmentStore _store;
    private readonly ILogger<JudgmentAnalyticsService> _logger;

    public JudgmentAnalyticsService(
        JudgmentStore store,
        ILogger<JudgmentAnalyticsService> logger)
    {
        _store  = store;
        _logger = logger;
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns profiles for all models that have participated in at least one judgment.
    /// </summary>
    public async Task<List<ModelProfile>> GetAllProfilesAsync(CancellationToken ct = default)
    {
        var records = await _store.GetAllAsync(ct);
        if (records.Count == 0)
            return [];

        var stats = ComputeAllModelStats(records);
        var panelAverages = ComputePanelAverages(stats.Values);

        return stats.Values
            .OrderByDescending(s => s.WinRate)
            .ThenByDescending(s => s.AvgScore)
            .Select(s => BuildProfile(s, panelAverages, records))
            .ToList();
    }

    /// <summary>
    /// Returns the profile for a single model, or <c>null</c> if the model
    /// has no history.
    /// </summary>
    public async Task<ModelProfile?> GetProfileAsync(string modelId, CancellationToken ct = default)
    {
        var records = await _store.GetAllAsync(ct);
        if (records.Count == 0)
            return null;

        var stats = ComputeAllModelStats(records);
        if (!stats.TryGetValue(modelId, out var modelStats))
            return null;

        var panelAverages = ComputePanelAverages(stats.Values);
        return BuildProfile(modelStats, panelAverages, records);
    }

    /// <summary>
    /// Returns aggregated system-level metrics.
    /// </summary>
    public async Task<JudgmentMetrics> GetMetricsAsync(CancellationToken ct = default)
    {
        var records = await _store.GetAllAsync(ct);
        if (records.Count == 0)
        {
            return new JudgmentMetrics { TotalJudgments = 0, TotalModels = 0 };
        }

        var stats = ComputeAllModelStats(records);
        var panelAverages = ComputePanelAverages(stats.Values);

        // Overall disagreement rate: fraction of judgments where the model with
        // the highest historical win-rate was NOT the actual winner.
        var topModel = stats.Values
            .OrderByDescending(s => s.WinRate)
            .ThenByDescending(s => s.AvgScore)
            .FirstOrDefault();

        int disagreements = 0;
        foreach (var record in records)
        {
            if (topModel is not null && record.WinnerId != topModel.ModelId)
                disagreements++;
        }

        double overallDisagreementRate = records.Count > 0
            ? Math.Round((double)disagreements / records.Count, 4)
            : 0.0;

        // Average confidence across all models
        var profiles = stats.Values
            .Select(s => BuildProfile(s, panelAverages, records))
            .ToList();

        double avgConfidence = profiles.Count > 0
            ? Math.Round(profiles.Average(p => p.Confidence.Score), 4)
            : 0.0;

        double globalAvgScore = records
            .SelectMany(r => r.ModelResults)
            .Where(r => r.Status == "done")
            .Select(r => r.Score)
            .DefaultIfEmpty(0.0)
            .Average();

        return new JudgmentMetrics
        {
            TotalJudgments          = records.Count,
            TotalModels             = stats.Count,
            OverallDisagreementRate = overallDisagreementRate,
            AvgConfidence           = avgConfidence,
            ModelStats              = stats.Values
                                         .OrderByDescending(s => s.WinRate)
                                         .ThenByDescending(s => s.AvgScore)
                                         .ToList(),
            TopModel                = topModel?.ModelId ?? "",
            GlobalAvgScore          = Math.Round(globalAvgScore, 4),
        };
    }

    // ── Statistics computation ─────────────────────────────────────────────────

    /// <summary>
    /// Computes rolling <see cref="ModelStats"/> for every model that appears
    /// in the judgment history.
    /// </summary>
    private static Dictionary<string, ModelStats> ComputeAllModelStats(
        IReadOnlyList<JudgmentRecord> records)
    {
        // Group all model results by modelId
        var resultsByModel = new Dictionary<string, List<(ModelJudgmentResult Result, bool Won)>>(StringComparer.OrdinalIgnoreCase);

        foreach (var record in records)
        {
            foreach (var result in record.ModelResults)
            {
                if (!resultsByModel.TryGetValue(result.ModelId, out var list))
                {
                    list = [];
                    resultsByModel[result.ModelId] = list;
                }
                list.Add((result, result.ModelId == record.WinnerId));
            }
        }

        var statsMap = new Dictionary<string, ModelStats>(StringComparer.OrdinalIgnoreCase);

        foreach (var (modelId, entries) in resultsByModel)
        {
            var doneEntries = entries.Where(e => e.Result.Status == "done").ToList();
            var scores      = doneEntries.Select(e => e.Result.Score).ToList();

            int wins       = entries.Count(e => e.Won);
            int total      = entries.Count;
            int errorCount = entries.Count(e => e.Result.Status == "error");

            double avgScore              = scores.Count > 0 ? scores.Average() : 0.0;
            double avgClarity            = Mean(doneEntries, e => e.Result.ScoreBreakdown.Clarity);
            double avgReasoning          = Mean(doneEntries, e => e.Result.ScoreBreakdown.Reasoning);
            double avgCompleteness       = Mean(doneEntries, e => e.Result.ScoreBreakdown.Completeness);
            double avgLatency            = Mean(doneEntries, e => e.Result.ScoreBreakdown.Latency);
            double avgReasoningQuality   = Mean(doneEntries, e => e.Result.ScoreBreakdown.ReasoningQuality);
            double volatility            = StdDev(scores);

            // Disagreement rate: fraction of participated judgments where this
            // model was NOT the winner, conditioned on it having above-average score
            // in that judgment (i.e., it "should" have won but didn't).
            int shouldWinCount = 0;
            int didNotWinCount = 0;
            foreach (var record in records)
            {
                var thisResult = record.ModelResults.FirstOrDefault(r =>
                    string.Equals(r.ModelId, modelId, StringComparison.OrdinalIgnoreCase));
                if (thisResult is null) continue;
                if (thisResult.Status != "done") continue;

                var doneResults = record.ModelResults.Where(r => r.Status == "done").ToList();
                if (doneResults.Count < 2) continue;

                double panelAvg = doneResults.Average(r => r.Score);
                if (thisResult.Score >= panelAvg)
                {
                    shouldWinCount++;
                    if (record.WinnerId != modelId)
                        didNotWinCount++;
                }
            }

            double disagreementRate = shouldWinCount > 0
                ? Math.Round((double)didNotWinCount / shouldWinCount, 4)
                : 0.0;

            statsMap[modelId] = new ModelStats
            {
                ModelId              = modelId,
                TotalJudgments       = total,
                Wins                 = wins,
                WinRate              = total > 0 ? Math.Round((double)wins / total, 4) : 0.0,
                AvgScore             = Math.Round(avgScore, 4),
                AvgClarity           = Math.Round(avgClarity, 4),
                AvgReasoning         = Math.Round(avgReasoning, 4),
                AvgCompleteness      = Math.Round(avgCompleteness, 4),
                AvgLatency           = Math.Round(avgLatency, 4),
                AvgReasoningQuality  = Math.Round(avgReasoningQuality, 4),
                ScoreVolatility      = Math.Round(volatility, 4),
                DisagreementRate     = disagreementRate,
                ErrorCount           = errorCount,
            };
        }

        return statsMap;
    }

    // ── Panel averages ─────────────────────────────────────────────────────────

    private sealed record PanelAverages(
        double AvgClarity,
        double AvgReasoning,
        double AvgCompleteness,
        double AvgLatency,
        double AvgReasoningQuality,
        double AvgScore);

    private static PanelAverages ComputePanelAverages(IEnumerable<ModelStats> stats)
    {
        var list = stats.ToList();
        if (list.Count == 0)
            return new PanelAverages(0, 0, 0, 0, 0, 0);

        return new PanelAverages(
            AvgClarity:          list.Average(s => s.AvgClarity),
            AvgReasoning:        list.Average(s => s.AvgReasoning),
            AvgCompleteness:     list.Average(s => s.AvgCompleteness),
            AvgLatency:          list.Average(s => s.AvgLatency),
            AvgReasoningQuality: list.Average(s => s.AvgReasoningQuality),
            AvgScore:            list.Average(s => s.AvgScore));
    }

    // ── Profile building ───────────────────────────────────────────────────────

    private static ModelProfile BuildProfile(
        ModelStats stats,
        PanelAverages panelAvg,
        IReadOnlyList<JudgmentRecord> records)
    {
        var strengths    = DeriveStrengths(stats, panelAvg);
        var weaknesses   = DeriveWeaknesses(stats, panelAvg);
        var failureModes = DeriveFailureModes(stats, panelAvg);
        var confidence   = DeriveConfidence(stats, records);

        return new ModelProfile
        {
            ModelId          = stats.ModelId,
            Stats            = stats,
            Strengths        = strengths,
            Weaknesses       = weaknesses,
            TypicalFailureModes = failureModes,
            Confidence       = confidence,
        };
    }

    private static List<string> DeriveStrengths(ModelStats stats, PanelAverages panelAvg)
    {
        var strengths = new List<string>();

        if (stats.AvgClarity          - panelAvg.AvgClarity          >= StrengthDeltaThreshold)
            strengths.Add("Clarity");
        if (stats.AvgReasoning        - panelAvg.AvgReasoning        >= StrengthDeltaThreshold)
            strengths.Add("Reasoning");
        if (stats.AvgCompleteness     - panelAvg.AvgCompleteness     >= StrengthDeltaThreshold)
            strengths.Add("Completeness");
        if (stats.AvgLatency          - panelAvg.AvgLatency          >= StrengthDeltaThreshold)
            strengths.Add("Latency");
        if (stats.AvgReasoningQuality - panelAvg.AvgReasoningQuality >= StrengthDeltaThreshold)
            strengths.Add("ReasoningQuality");

        return strengths;
    }

    private static List<string> DeriveWeaknesses(ModelStats stats, PanelAverages panelAvg)
    {
        var weaknesses = new List<string>();

        if (panelAvg.AvgClarity          - stats.AvgClarity          >= WeaknessDeltaThreshold)
            weaknesses.Add("Clarity");
        if (panelAvg.AvgReasoning        - stats.AvgReasoning        >= WeaknessDeltaThreshold)
            weaknesses.Add("Reasoning");
        if (panelAvg.AvgCompleteness     - stats.AvgCompleteness     >= WeaknessDeltaThreshold)
            weaknesses.Add("Completeness");
        if (panelAvg.AvgLatency          - stats.AvgLatency          >= WeaknessDeltaThreshold)
            weaknesses.Add("Latency");
        if (panelAvg.AvgReasoningQuality - stats.AvgReasoningQuality >= WeaknessDeltaThreshold)
            weaknesses.Add("ReasoningQuality");

        return weaknesses;
    }

    private static List<string> DeriveFailureModes(ModelStats stats, PanelAverages panelAvg)
    {
        var failures = new List<string>();

        double errorRate = stats.TotalJudgments > 0
            ? (double)stats.ErrorCount / stats.TotalJudgments
            : 0.0;

        if (errorRate >= HighErrorRateThreshold)
            failures.Add($"Frequent errors ({errorRate:P0} of judgments failed)");

        if (stats.AvgClarity < LowCriterionThreshold)
            failures.Add("Consistently low clarity (unstructured responses)");

        if (stats.AvgReasoning < LowCriterionThreshold)
            failures.Add("Consistently low reasoning signal (few logical connectives)");

        if (stats.AvgCompleteness < LowCriterionThreshold)
            failures.Add("Consistently low completeness (short or shallow responses)");

        if (stats.ScoreVolatility > MaxPossibleScore * HighVolatilityFraction)
            failures.Add("High score volatility (inconsistent response quality)");

        if (stats.WinRate < LowWinRateThreshold && stats.TotalJudgments >= MinJudgmentsForWinRateAnalysis)
            failures.Add("Rarely wins panel judgments");

        if (stats.DisagreementRate > HighDisagreementRateThreshold)
            failures.Add("Frequently outscored by lower-ranked models (panel disagreement)");

        if (stats.AvgLatency < panelAvg.AvgLatency - WeaknessDeltaThreshold)
            failures.Add("Below-average latency performance");

        return failures;
    }

    // ── Confidence estimation ──────────────────────────────────────────────────

    private static ConfidenceInfo DeriveConfidence(
        ModelStats stats,
        IReadOnlyList<JudgmentRecord> records)
    {
        // ── 1. Historical consistency (0–1): 1 − normalised volatility ─────────
        double normalisedVolatility   = Math.Min(1.0, stats.ScoreVolatility / MaxPossibleScore);
        double historicalConsistency  = Math.Round(1.0 - normalisedVolatility, 4);

        // ── 2. Average win margin ───────────────────────────────────────────────
        var winMargins = new List<double>();
        foreach (var record in records)
        {
            if (!string.Equals(record.WinnerId, stats.ModelId, StringComparison.OrdinalIgnoreCase))
                continue;

            var doneResults = record.ModelResults
                .Where(r => r.Status == "done")
                .OrderByDescending(r => r.Score)
                .ToList();

            if (doneResults.Count < 2) continue;

            double winnerScore   = doneResults[0].Score;
            double runnerUpScore = doneResults[1].Score;
            winMargins.Add(winnerScore - runnerUpScore);
        }

        double avgWinMargin = winMargins.Count > 0
            ? Math.Round(winMargins.Average(), 4)
            : 0.0;

        // Normalise win margin to 0–1 (a margin ≥ MaxMeaningfulMarginDivisor maps to 1.0)
        double normalisedMargin = Math.Min(1.0, avgWinMargin / MaxMeaningfulMarginDivisor);

        // ── 3. Panel agreement rate (1 − disagreement rate) ────────────────────
        double panelAgreementRate = Math.Round(1.0 - stats.DisagreementRate, 4);

        // ── 4. Composite confidence: weighted average of the three components ──
        // Weights: ConsistencyWeight + WinMarginWeight + AgreementWeight = 1.0
        double compositeScore = Math.Round(
            historicalConsistency * ConsistencyWeight
          + normalisedMargin      * WinMarginWeight
          + panelAgreementRate    * AgreementWeight,
            4);

        // ── 5. Explanation ─────────────────────────────────────────────────────
        var explanation = BuildConfidenceExplanation(
            stats, historicalConsistency, avgWinMargin, panelAgreementRate, compositeScore);

        return new ConfidenceInfo
        {
            Score                 = compositeScore,
            AvgWinMargin          = avgWinMargin,
            HistoricalConsistency = historicalConsistency,
            PanelAgreementRate    = panelAgreementRate,
            Explanation           = explanation,
        };
    }

    private static string BuildConfidenceExplanation(
        ModelStats stats,
        double consistency,
        double avgMargin,
        double agreementRate,
        double composite)
    {
        var level = composite switch
        {
            >= 0.75 => "High",
            >= 0.50 => "Moderate",
            >= 0.25 => "Low",
            _       => "Very low",
        };

        return $"{level} confidence ({composite:P0}). " +
               $"Historical consistency: {consistency:P0} (score volatility σ={stats.ScoreVolatility:F2}). " +
               $"Avg win margin when victorious: {avgMargin:F2} pts. " +
               $"Panel agreement rate: {agreementRate:P0}.";
    }

    // ── Math helpers ───────────────────────────────────────────────────────────

    private static double Mean<T>(
        IReadOnlyList<T> items,
        Func<T, double> selector)
        => items.Count > 0 ? items.Average(selector) : 0.0;

    private static double StdDev(IReadOnlyList<double> values)
    {
        if (values.Count < 2) return 0.0;
        double mean  = values.Average();
        double sumSq = values.Sum(v => (v - mean) * (v - mean));
        return Math.Sqrt(sumSq / values.Count);
    }
}
