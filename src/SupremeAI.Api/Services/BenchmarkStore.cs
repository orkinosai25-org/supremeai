using System.Text.Json;
using SupremeAI.Api.Models;

namespace SupremeAI.Api.Services;

/// <summary>
/// Persists and retrieves <see cref="BenchmarkRunRecord"/> objects using a
/// newline-delimited JSON file — the same pattern used by <see cref="JudgmentStore"/>.
///
/// The default storage path is <c>benchmark-runs.ndjson</c> next to the
/// application's content root.  Override with the <c>BenchmarkStorePath</c>
/// configuration key.
/// </summary>
public sealed class BenchmarkStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy   = JsonNamingPolicy.CamelCase,
        WriteIndented          = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly string _filePath;
    private readonly ILogger<BenchmarkStore> _logger;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public BenchmarkStore(
        IConfiguration configuration,
        IWebHostEnvironment env,
        ILogger<BenchmarkStore> logger)
    {
        _logger = logger;

        var configured = configuration["BenchmarkStorePath"];
        _filePath = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(env.ContentRootPath, "benchmark-runs.ndjson")
            : configured;

        _logger.LogInformation("BenchmarkStore: persistence file = {Path}", _filePath);
    }

    /// <summary>Appends <paramref name="record"/> to the store.</summary>
    public async Task SaveAsync(BenchmarkRunRecord record, CancellationToken ct = default)
    {
        var line = JsonSerializer.Serialize(record, JsonOptions);

        await _writeLock.WaitAsync(ct);
        try
        {
            await File.AppendAllTextAsync(_filePath, line + Environment.NewLine, ct);
            _logger.LogInformation("BenchmarkStore: saved run {Id}", record.Id);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Replaces the most recent persisted record that has the same
    /// <see cref="BenchmarkRunRecord.Id"/> with the provided updated record.
    /// If no matching record is found the record is simply appended.
    /// </summary>
    public async Task UpdateAsync(BenchmarkRunRecord record, CancellationToken ct = default)
    {
        await _writeLock.WaitAsync(ct);
        try
        {
            List<BenchmarkRunRecord> records = [];

            if (File.Exists(_filePath))
            {
                var lines = await File.ReadAllLinesAsync(_filePath, ct);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;
                    try
                    {
                        var r = JsonSerializer.Deserialize<BenchmarkRunRecord>(trimmed, JsonOptions);
                        if (r is not null) records.Add(r);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "BenchmarkStore: could not parse line; skipping.");
                    }
                }
            }

            // Replace or append
            var idx = records.FindIndex(r => r.Id == record.Id);
            if (idx >= 0)
                records[idx] = record;
            else
                records.Add(record);

            var newContent = string.Concat(
                records.Select(r => JsonSerializer.Serialize(r, JsonOptions) + Environment.NewLine));

            await File.WriteAllTextAsync(_filePath, newContent, ct);
            _logger.LogInformation("BenchmarkStore: updated run {Id}", record.Id);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Returns all run records for the specified <paramref name="benchmarkId"/>,
    /// ordered by descending <see cref="BenchmarkRunRecord.StartedAt"/>.
    /// </summary>
    public async Task<List<BenchmarkRunRecord>> GetRunsByBenchmarkIdAsync(
        string benchmarkId,
        CancellationToken ct = default)
    {
        return (await ReadAllAsync(ct))
            .Where(r => r.BenchmarkId == benchmarkId)
            .OrderByDescending(r => r.StartedAt)
            .ToList();
    }

    /// <summary>
    /// Returns the most recent completed run for <paramref name="benchmarkId"/>,
    /// or <c>null</c> if none exists.
    /// </summary>
    public async Task<BenchmarkRunRecord?> GetLatestCompletedRunAsync(
        string benchmarkId,
        CancellationToken ct = default)
    {
        return (await ReadAllAsync(ct))
            .Where(r => r.BenchmarkId == benchmarkId && r.Status == "completed")
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefault();
    }

    /// <summary>Returns all stored run records ordered by ascending start time.</summary>
    public async Task<List<BenchmarkRunRecord>> ReadAllAsync(CancellationToken ct = default)
    {
        await _writeLock.WaitAsync(ct);
        try
        {
            return ReadAllInner();
        }
        finally
        {
            _writeLock.Release();
        }
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    /// <remarks>Must be called while holding <c>_writeLock</c>.</remarks>
    private List<BenchmarkRunRecord> ReadAllInner()
    {
        if (!File.Exists(_filePath))
            return [];

        var lines   = File.ReadAllLines(_filePath);
        var records = new List<BenchmarkRunRecord>(lines.Length);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            try
            {
                var r = JsonSerializer.Deserialize<BenchmarkRunRecord>(trimmed, JsonOptions);
                if (r is not null) records.Add(r);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "BenchmarkStore: could not parse line; skipping.");
            }
        }

        return records.OrderBy(r => r.StartedAt).ToList();
    }
}
