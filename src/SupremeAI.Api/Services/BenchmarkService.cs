using System.Text;
using SupremeAI.Api.Models;

namespace SupremeAI.Api.Services;

/// <summary>
/// The Benchmark Service exposes SupremeAI's judgment intelligence as
/// structured benchmark results.
///
/// Responsibilities:
///   1. Provide the built-in benchmark pack catalogue (reasoning, factual,
///      coding, summarization, domain-specific). Packs are immutable and
///      versioned so runs are always replayable.
///   2. Execute benchmark runs by fanning each question through the
///      <see cref="JudgmentEngine"/> and recording the resulting judgment IDs.
///   3. Derive <see cref="BenchmarkResults"/> — leaderboard, confidence ranges,
///      and disagreement heatmap — purely from <see cref="JudgmentRecord"/>
///      objects persisted in <see cref="JudgmentStore"/>.
///
/// Constraints:
///   • No model training.
///   • No hidden model judging.
///   • All numeric values are directly traceable to JudgmentStore records.
/// </summary>
public sealed class BenchmarkService
{
    // ── Built-in benchmark pack catalogue ────────────────────────────────────

    private static readonly IReadOnlyList<BenchmarkPack> BuiltInPacks = BuildBuiltInPacks();

    private readonly JudgmentEngine _engine;
    private readonly JudgmentStore  _judgmentStore;
    private readonly BenchmarkStore _benchmarkStore;
    private readonly ILogger<BenchmarkService> _logger;

    public BenchmarkService(
        JudgmentEngine engine,
        JudgmentStore judgmentStore,
        BenchmarkStore benchmarkStore,
        ILogger<BenchmarkService> logger)
    {
        _engine         = engine;
        _judgmentStore  = judgmentStore;
        _benchmarkStore = benchmarkStore;
        _logger         = logger;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Returns all available benchmark packs.</summary>
    public IReadOnlyList<BenchmarkPack> GetAllPacks() => BuiltInPacks;

    /// <summary>
    /// Returns the benchmark pack with the given <paramref name="id"/>,
    /// or <c>null</c> if it does not exist.
    /// </summary>
    public BenchmarkPack? GetPack(string id) =>
        BuiltInPacks.FirstOrDefault(p =>
            string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Executes a benchmark run: iterates through each question in the pack
    /// and calls <see cref="JudgmentEngine.JudgeAsync"/> for each one.
    /// The resulting judgment IDs are stored in a <see cref="BenchmarkRunRecord"/>.
    /// </summary>
    /// <param name="benchmarkId">ID of the benchmark pack to run.</param>
    /// <param name="modelIds">
    /// Models to evaluate.  Pass an empty list to use the engine's default panel.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The persisted run record.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="benchmarkId"/> does not match a known pack.
    /// </exception>
    public async Task<BenchmarkRunRecord> RunAsync(
        string benchmarkId,
        List<string> modelIds,
        CancellationToken ct = default)
    {
        var pack = GetPack(benchmarkId)
            ?? throw new ArgumentException($"Unknown benchmark id '{benchmarkId}'.", nameof(benchmarkId));

        _logger.LogInformation(
            "BenchmarkService: starting run for '{BenchmarkId}' with {Q} questions, {M} model(s)",
            benchmarkId, pack.Questions.Count, modelIds.Count > 0 ? modelIds.Count : -1);

        var run = new BenchmarkRunRecord
        {
            BenchmarkId = benchmarkId,
            ModelIds    = modelIds,
            StartedAt   = DateTimeOffset.UtcNow,
            Status      = "in_progress",
        };

        await _benchmarkStore.SaveAsync(run, ct);

        try
        {
            foreach (var question in pack.Questions)
            {
                ct.ThrowIfCancellationRequested();

                var request = new JudgeRequest
                {
                    Prompt   = question.Prompt,
                    ModelIds = modelIds,
                };

                var judgment = await _engine.JudgeAsync(request, ct);
                run.JudgmentIds.Add(judgment.Id);

                _logger.LogDebug(
                    "BenchmarkService: completed question '{QId}' → judgment '{JId}'",
                    question.Id, judgment.Id);
            }

            run.CompletedAt = DateTimeOffset.UtcNow;
            run.Status      = "completed";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "BenchmarkService: run '{RunId}' failed", run.Id);
            run.Status       = "failed";
            run.ErrorMessage = ex.Message;
            run.CompletedAt  = DateTimeOffset.UtcNow;
        }

        await _benchmarkStore.UpdateAsync(run, ct);
        return run;
    }

    /// <summary>
    /// Computes <see cref="BenchmarkResults"/> for the most recent completed
    /// run of the benchmark identified by <paramref name="benchmarkId"/>.
    /// Returns <c>null</c> when no completed run exists.
    /// </summary>
    public async Task<BenchmarkResults?> GetResultsAsync(
        string benchmarkId,
        CancellationToken ct = default)
    {
        var latestRun = await _benchmarkStore.GetLatestCompletedRunAsync(benchmarkId, ct);
        if (latestRun is null)
            return null;

        return await ComputeResultsAsync(latestRun, ct);
    }

    /// <summary>
    /// Renders <paramref name="results"/> as a Markdown summary string.
    /// </summary>
    public string ToMarkdown(BenchmarkResults results, BenchmarkPack pack)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"# Benchmark Results: {pack.Name}");
        sb.AppendLine();
        sb.AppendLine($"**Benchmark ID:** `{results.BenchmarkId}`  ");
        sb.AppendLine($"**Run ID:** `{results.RunId}`  ");
        sb.AppendLine($"**Questions:** {results.TotalQuestions}  ");
        sb.AppendLine($"**Models evaluated:** {results.ModelsEvaluated}  ");
        sb.AppendLine($"**Run started:** {results.RunAt:u}  ");
        sb.AppendLine($"**Results generated:** {results.GeneratedAt:u}  ");
        sb.AppendLine();

        // ── Leaderboard ──────────────────────────────────────────────────────
        sb.AppendLine("## Leaderboard");
        sb.AppendLine();
        sb.AppendLine("| Rank | Model | Avg Score | Win Rate | Clarity | Reasoning | Completeness | 95% CI |");
        sb.AppendLine("|------|-------|-----------|----------|---------|-----------|--------------|--------|");

        var rank = 1;
        foreach (var s in results.Leaderboard)
        {
            var medal = rank switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => $"{rank}." };
            sb.AppendLine(
                $"| {medal} | {Sanitize(s.ModelId)} | {s.AvgScore:F2} | {s.WinRate:P0} " +
                $"| {s.AvgClarity:F2} | {s.AvgReasoning:F2} | {s.AvgCompleteness:F2} " +
                $"| [{s.ConfidenceLow:F2}, {s.ConfidenceHigh:F2}] |");
            rank++;
        }

        sb.AppendLine();

        // ── Disagreement heatmap (top 10 pairs by delta) ─────────────────────
        if (results.DisagreementHeatmap.Count > 0)
        {
            sb.AppendLine("## Top Disagreements");
            sb.AppendLine();
            sb.AppendLine("| Question | Model A | Model B | Score Δ |");
            sb.AppendLine("|----------|---------|---------|---------|");

            foreach (var cell in results.DisagreementHeatmap
                         .OrderByDescending(c => c.ScoreDelta)
                         .Take(10))
            {
                sb.AppendLine(
                    $"| {cell.QuestionId} | {Sanitize(cell.ModelA)} " +
                    $"| {Sanitize(cell.ModelB)} | {cell.ScoreDelta:F2} |");
            }

            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Derives <see cref="BenchmarkResults"/> from the judgment records
    /// referenced by <paramref name="run"/>.
    /// </summary>
    private async Task<BenchmarkResults> ComputeResultsAsync(
        BenchmarkRunRecord run,
        CancellationToken ct)
    {
        // Retrieve all judgments from the store and index them by ID.
        var allJudgments = await _judgmentStore.GetAllAsync(ct);
        var byId = allJudgments.ToDictionary(j => j.Id);

        var pack = GetPack(run.BenchmarkId);

        // Collect the judgment records for this run (in question order).
        var judgments = run.JudgmentIds
            .Select(id => byId.TryGetValue(id, out var j) ? j : null)
            .Where(j => j is not null)
            .Select(j => j!)
            .ToList();

        // Determine which models participated.
        var modelIds = judgments
            .SelectMany(j => j.ModelResults.Select(r => r.ModelId))
            .Distinct()
            .ToList();

        // ── Per-model score aggregation ───────────────────────────────────────

        var scores = new Dictionary<string, List<double>>(StringComparer.Ordinal);
        var clarityMap      = new Dictionary<string, List<double>>(StringComparer.Ordinal);
        var reasoningMap    = new Dictionary<string, List<double>>(StringComparer.Ordinal);
        var completenessMap = new Dictionary<string, List<double>>(StringComparer.Ordinal);
        var latencyMap      = new Dictionary<string, List<double>>(StringComparer.Ordinal);
        var rqMap           = new Dictionary<string, List<double>>(StringComparer.Ordinal);
        var winMap          = new Dictionary<string, int>(StringComparer.Ordinal);
        var errorMap        = new Dictionary<string, int>(StringComparer.Ordinal);
        var answeredMap     = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var mid in modelIds)
        {
            scores[mid]          = [];
            clarityMap[mid]      = [];
            reasoningMap[mid]    = [];
            completenessMap[mid] = [];
            latencyMap[mid]      = [];
            rqMap[mid]           = [];
            winMap[mid]          = 0;
            errorMap[mid]        = 0;
            answeredMap[mid]     = 0;
        }

        foreach (var judgment in judgments)
        {
            foreach (var result in judgment.ModelResults)
            {
                var mid = result.ModelId;
                if (!scores.ContainsKey(mid)) continue;

                if (result.Status == "error")
                {
                    errorMap[mid]++;
                    continue;
                }

                answeredMap[mid]++;
                scores[mid].Add(result.Score);
                clarityMap[mid].Add(result.ScoreBreakdown.Clarity);
                reasoningMap[mid].Add(result.ScoreBreakdown.Reasoning);
                completenessMap[mid].Add(result.ScoreBreakdown.Completeness);
                latencyMap[mid].Add(result.ScoreBreakdown.Latency);
                rqMap[mid].Add(result.ScoreBreakdown.ReasoningQuality);
            }

            // Count wins
            if (!string.IsNullOrWhiteSpace(judgment.WinnerId) && winMap.ContainsKey(judgment.WinnerId))
                winMap[judgment.WinnerId]++;
        }

        var leaderboard = modelIds.Select(mid =>
        {
            var s = scores[mid];
            var n = s.Count;

            var avg     = n > 0 ? s.Average() : 0.0;
            var stdDev  = n > 1 ? Math.Sqrt(s.Select(x => Math.Pow(x - avg, 2)).Average()) : 0.0;
            var total   = judgments.Count;
            var wins    = winMap[mid];

            // 95% confidence interval using the t-approximation (z=1.96 for large n, fallback for small n)
            var margin = n > 1 ? 1.96 * stdDev / Math.Sqrt(n) : stdDev;

            return new ModelBenchmarkScore
            {
                ModelId             = mid,
                AvgScore            = Math.Round(avg, 3),
                AvgClarity          = n > 0 ? Math.Round(clarityMap[mid].Average(), 3) : 0.0,
                AvgReasoning        = n > 0 ? Math.Round(reasoningMap[mid].Average(), 3) : 0.0,
                AvgCompleteness     = n > 0 ? Math.Round(completenessMap[mid].Average(), 3) : 0.0,
                AvgLatency          = n > 0 ? Math.Round(latencyMap[mid].Average(), 3) : 0.0,
                AvgReasoningQuality = n > 0 ? Math.Round(rqMap[mid].Average(), 3) : 0.0,
                WinCount            = wins,
                WinRate             = total > 0 ? Math.Round((double)wins / total, 4) : 0.0,
                ScoreStdDev         = Math.Round(stdDev, 3),
                ConfidenceLow       = Math.Round(Math.Max(0.0, avg - margin), 3),
                ConfidenceHigh      = Math.Round(avg + margin, 3),
                AnsweredCount       = answeredMap[mid],
                ErrorCount          = errorMap[mid],
            };
        })
        .OrderByDescending(s => s.AvgScore)
        .ToList();

        // ── Disagreement heatmap ──────────────────────────────────────────────

        var heatmap = new List<DisagreementCell>();

        if (pack is not null)
        {
            for (var qi = 0; qi < judgments.Count && qi < pack.Questions.Count; qi++)
            {
                var judgment  = judgments[qi];
                var questionId = pack.Questions[qi].Id;

                var resultsByModel = judgment.ModelResults
                    .Where(r => r.Status == "done")
                    .ToDictionary(r => r.ModelId);

                var modelList = resultsByModel.Keys.ToList();
                for (var a = 0; a < modelList.Count; a++)
                {
                    for (var b = a + 1; b < modelList.Count; b++)
                    {
                        var scoreA = resultsByModel[modelList[a]].Score;
                        var scoreB = resultsByModel[modelList[b]].Score;
                        var delta  = Math.Abs(scoreA - scoreB);

                        if (delta > 0.0)
                        {
                            heatmap.Add(new DisagreementCell
                            {
                                QuestionId = questionId,
                                ModelA     = modelList[a],
                                ModelB     = modelList[b],
                                ScoreDelta = Math.Round(delta, 3),
                            });
                        }
                    }
                }
            }
        }

        return new BenchmarkResults
        {
            BenchmarkId        = run.BenchmarkId,
            RunId              = run.Id,
            Leaderboard        = leaderboard,
            DisagreementHeatmap = heatmap,
            TotalQuestions     = judgments.Count,
            ModelsEvaluated    = modelIds.Count,
            RunAt              = run.StartedAt,
            GeneratedAt        = DateTimeOffset.UtcNow,
        };
    }

    private static string Sanitize(string value) =>
        value.Replace('\r', ' ').Replace('\n', ' ');

    // ── Built-in benchmark pack definitions ───────────────────────────────────

    private static List<BenchmarkPack> BuildBuiltInPacks()
    {
        var createdAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        return
        [
            new BenchmarkPack
            {
                Id          = "reasoning-v1",
                Name        = "Reasoning Benchmark (v1)",
                Category    = "reasoning",
                Version     = "1.0.0",
                Description = "Tests multi-step logical deduction, analogical reasoning, and causal inference.",
                CreatedAt   = createdAt,
                Questions   =
                [
                    new() { Id = "r1", Category = "reasoning", Prompt = "If all Bloops are Razzies and all Razzies are Lazzies, are all Bloops definitely Lazzies? Explain your reasoning step by step." },
                    new() { Id = "r2", Category = "reasoning", Prompt = "A bat and a ball cost $1.10 in total. The bat costs $1.00 more than the ball. How much does the ball cost? Show your working." },
                    new() { Id = "r3", Category = "reasoning", Prompt = "There are 5 houses in a row, each painted a different color. The green house is immediately to the left of the white house. What can you deduce about their positions? List all valid configurations." },
                    new() { Id = "r4", Category = "reasoning", Prompt = "If it takes 5 machines 5 minutes to make 5 widgets, how long does it take 100 machines to make 100 widgets? Explain your reasoning." },
                    new() { Id = "r5", Category = "reasoning", Prompt = "A farmer has 17 sheep. All but 9 die. How many are left? Explain the potential ambiguity in the question." },
                ],
            },

            new BenchmarkPack
            {
                Id          = "factual-v1",
                Name        = "Factual Knowledge Benchmark (v1)",
                Category    = "factual",
                Version     = "1.0.0",
                Description = "Tests retrieval of well-established factual knowledge across science, history, and geography.",
                CreatedAt   = createdAt,
                Questions   =
                [
                    new() { Id = "f1", Category = "factual", Prompt = "What is the speed of light in a vacuum? Give the exact SI value and explain its significance in physics." },
                    new() { Id = "f2", Category = "factual", Prompt = "Describe the structure of DNA and explain the base-pairing rules discovered by Watson and Crick." },
                    new() { Id = "f3", Category = "factual", Prompt = "What were the main causes of World War I? Provide at least four distinct contributing factors." },
                    new() { Id = "f4", Category = "factual", Prompt = "Name the seven continents and give the approximate population and land area of each." },
                    new() { Id = "f5", Category = "factual", Prompt = "Explain the water cycle (hydrological cycle) in detail, including all major stages and processes." },
                ],
            },

            new BenchmarkPack
            {
                Id          = "coding-v1",
                Name        = "Coding Benchmark (v1)",
                Category    = "coding",
                Version     = "1.0.0",
                Description = "Tests code generation, debugging, and algorithmic problem solving.",
                CreatedAt   = createdAt,
                Questions   =
                [
                    new() { Id = "c1", Category = "coding", Tags = ["python", "algorithms"], Prompt = "Write a Python function that checks whether a given string is a valid palindrome, ignoring spaces and non-alphanumeric characters. Include docstring and at least three test cases." },
                    new() { Id = "c2", Category = "coding", Tags = ["algorithms", "complexity"], Prompt = "Implement a binary search algorithm in any language of your choice. Explain the time and space complexity." },
                    new() { Id = "c3", Category = "coding", Tags = ["debugging"], Prompt = "The following Python code is supposed to flatten a nested list but has a bug. Find and fix it:\n\n```python\ndef flatten(lst):\n    result = []\n    for item in lst:\n        if isinstance(item, list):\n            result.append(flatten(item))\n        else:\n            result.append(item)\n    return result\n```" },
                    new() { Id = "c4", Category = "coding", Tags = ["sql"], Prompt = "Write a SQL query that finds the top 3 customers by total order value from a table called 'orders' with columns: customer_id, order_date, amount. Include ties." },
                    new() { Id = "c5", Category = "coding", Tags = ["design-patterns"], Prompt = "Explain the Observer design pattern. Provide a concrete implementation example in any object-oriented language." },
                ],
            },

            new BenchmarkPack
            {
                Id          = "summarization-v1",
                Name        = "Summarization Benchmark (v1)",
                Category    = "summarization",
                Version     = "1.0.0",
                Description = "Tests the ability to produce accurate, concise, and well-structured summaries.",
                CreatedAt   = createdAt,
                Questions   =
                [
                    new() { Id = "s1", Category = "summarization", Prompt = "Summarize the key principles of machine learning in no more than 150 words, suitable for a non-technical audience." },
                    new() { Id = "s2", Category = "summarization", Prompt = "Provide a one-paragraph executive summary of the implications of climate change on global food security." },
                    new() { Id = "s3", Category = "summarization", Prompt = "Summarize the main arguments for and against universal basic income in bullet-point form, with no more than three points on each side." },
                    new() { Id = "s4", Category = "summarization", Prompt = "Condense the following concept into a single sentence suitable for a tweet (≤280 characters): Quantum entanglement is a phenomenon where two particles become correlated such that the quantum state of each particle cannot be described independently of the state of the other, even when separated by large distances." },
                    new() { Id = "s5", Category = "summarization", Prompt = "Create an abstract (100–200 words) for a hypothetical research paper on the effect of social media usage on teenage mental health." },
                ],
            },

            new BenchmarkPack
            {
                Id          = "domain-v1",
                Name        = "Domain-Specific Benchmark (v1)",
                Category    = "domain-specific",
                Version     = "1.0.0",
                Description = "Tests understanding of policy documents, technical specifications, and domain-expert knowledge.",
                CreatedAt   = createdAt,
                Questions   =
                [
                    new() { Id = "d1", Category = "domain-specific", Tags = ["policy"], Prompt = "Explain the key differences between GDPR and CCPA data privacy regulations. What obligations do they place on businesses?" },
                    new() { Id = "d2", Category = "domain-specific", Tags = ["technical"], Prompt = "Describe the CAP theorem in distributed systems. Give a real-world example of a system that prioritises each possible pair of guarantees." },
                    new() { Id = "d3", Category = "domain-specific", Tags = ["policy", "ai"], Prompt = "Summarise the EU AI Act's risk classification framework. What are the four risk tiers and what obligations apply to each?" },
                    new() { Id = "d4", Category = "domain-specific", Tags = ["technical", "networking"], Prompt = "Explain the difference between TCP and UDP protocols. When should each be chosen and why?" },
                    new() { Id = "d5", Category = "domain-specific", Tags = ["policy", "finance"], Prompt = "What is Basel III? Describe its three pillars and the key capital requirements it introduced for banks." },
                ],
            },
        ];
    }
}
