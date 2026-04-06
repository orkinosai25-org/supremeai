using System.Diagnostics;
using SupremeAI.Api.Models;

namespace SupremeAI.Api.Services;

/// <summary>
/// The SupremeAI Brain — the meta-intelligence engine that sits above all models.
///
/// Responsibilities:
///   1. Fan a prompt out to multiple AI models in parallel.
///   2. Score each response using a multi-factor heuristic rubric.
///   3. Rank the results and surface the winning "Supreme Answer".
///
/// No model training is required. The Brain learns through evaluation.
/// </summary>
public sealed class BrainService
{
    /// <summary>Default panel used when no model IDs are supplied in the request.</summary>
    private static readonly string[] DefaultModelIds = ["gpt-4o", "llama-3-1-70b", "mistral-large"];

    // ── Scoring constants ─────────────────────────────────────────────────────
    private const double MaxLengthScore            = 3.0;
    private const double LengthScoreDivisor        = 300.0;
    private const double MaxReasoningScore         = 2.0;
    private const double ReasoningScorePerKeyword  = 0.5;
    private const double MaxLatencyBonus           = 1.0;
    private const double LatencyBonusThresholdMs   = 3000.0;
    private const int    MinimumTextLengthThreshold = 80;

    private readonly ModelProviderFactory _factory;
    private readonly ILogger<BrainService> _logger;

    public BrainService(ModelProviderFactory factory, ILogger<BrainService> logger)
    {
        _factory = factory;
        _logger  = logger;
    }

    /// <summary>
    /// Evaluates <paramref name="request"/> across all specified (or default) models,
    /// scores each response, and returns a ranked <see cref="SupremeResponse"/>.
    /// </summary>
    public async Task<SupremeResponse> EvaluateAsync(SupremeRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            throw new ArgumentException("Query must not be empty.", nameof(request));

        var modelIds = request.ModelIds.Count > 0
            ? request.ModelIds
            : [.. DefaultModelIds];

        _logger.LogInformation("BrainService: evaluating {Count} model(s) for query length {Len}",
            modelIds.Count, request.Query.Length);

        var sw = Stopwatch.StartNew();

        // ── Fan out to all models in parallel ────────────────────────────────
        var tasks = modelIds
            .Select(id => CallModelAsync(id, request, ct))
            .ToArray();

        var results = await Task.WhenAll(tasks);
        sw.Stop();

        // ── Score and rank ────────────────────────────────────────────────────
        foreach (var r in results)
            r.Score = ScoreResponse(r);

        var ranked = results
            .OrderByDescending(r => r.Score)
            .ToList();

        var winner = ranked.FirstOrDefault(r => r.Status == "done") ?? ranked[0];

        _logger.LogInformation(
            "BrainService: winner='{WinnerId}' score={Score} totalMs={Ms}",
            winner.ModelId, winner.Score, sw.ElapsedMilliseconds);

        return new SupremeResponse
        {
            Query         = request.Query,
            Results       = ranked,
            WinnerId      = winner.ModelId,
            SupremeAnswer = winner.Text,
            TotalMs       = (int)sw.ElapsedMilliseconds,
        };
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<ModelEvalResult> CallModelAsync(
        string modelId,
        SupremeRequest request,
        CancellationToken ct)
    {
        var chatRequest = new ChatRequest
        {
            ModelId     = modelId,
            Messages    = [new ChatMessage { Role = "user", Content = request.Query }],
            MaxTokens   = request.MaxTokens,
            Temperature = request.Temperature,
        };

        try
        {
            var response = await _factory.ChatAsync(chatRequest, ct);
            return new ModelEvalResult
            {
                ModelId      = response.ModelId,
                Text         = response.Text,
                Status       = response.Status,
                Tokens       = response.Tokens,
                Ms           = response.Ms,
                ErrorMessage = response.ErrorMessage,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BrainService: error calling model '{ModelId}'", modelId);
            return new ModelEvalResult
            {
                ModelId      = modelId,
                Status       = "error",
                ErrorMessage = ex.Message,
            };
        }
    }

    /// <summary>
    /// Multi-factor heuristic scoring rubric (max ~10 points).
    ///
    /// Factors:
    ///   • Length richness   — up to 3 pts (based on character count)
    ///   • Reasoning depth   — up to 2 pts (presence of explanation keywords)
    ///   • Structural quality — up to 2 pts (multi-line, lists, numbering)
    ///   • Sentence coverage — up to 2 pts (count of terminal punctuation)
    ///   • Latency bonus     — up to 1 pt  (fast responses score slightly higher)
    ///
    /// Error responses score 0.
    /// </summary>
    internal static double ScoreResponse(ModelEvalResult result)
    {
        if (result.Status != "done" || string.IsNullOrWhiteSpace(result.Text))
            return 0.0;

        var text  = result.Text;
        double score = 0;

        // ── Length richness (0–MaxLengthScore) ───────────────────────────────
        score += Math.Min(MaxLengthScore, text.Length / LengthScoreDivisor);

        // ── Reasoning depth (0–MaxReasoningScore) ────────────────────────────
        var reasoningKeywords = new[]
        {
            "because", "therefore", "however", "although", "thus", "hence",
            "since", "as a result", "in conclusion", "furthermore", "moreover",
        };
        var keywordHits = reasoningKeywords
            .Count(w => text.Contains(w, StringComparison.OrdinalIgnoreCase));
        score += Math.Min(MaxReasoningScore, keywordHits * ReasoningScorePerKeyword);

        // ── Structural quality (0–2) ──────────────────────────────────────────
        if (text.Contains('\n'))  score += 0.5;
        if (text.Contains("1.") || text.Contains("•") || text.Contains("- "))
            score += 0.5;
        if (text.Contains("**") || text.Contains("###") || text.Contains("##"))
            score += 0.5;
        if (text.Contains(':'))   score += 0.5;

        // ── Sentence coverage (0–2) ───────────────────────────────────────────
        var sentenceEnds = text.Count(c => c == '.' || c == '!' || c == '?');
        score += Math.Min(2.0, sentenceEnds * 0.25);

        // ── Latency bonus (0–MaxLatencyBonus) ────────────────────────────────
        // Responses under LatencyBonusThresholdMs get a small bonus.
        if (result.Ms > 0)
            score += Math.Max(0.0, MaxLatencyBonus - result.Ms / LatencyBonusThresholdMs);

        // ── Penalise very short answers ───────────────────────────────────────
        if (text.Length < MinimumTextLengthThreshold)
            score *= 0.5;

        return Math.Round(score, 2);
    }
}
