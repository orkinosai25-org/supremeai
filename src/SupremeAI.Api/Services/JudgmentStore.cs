using System.Text.Json;
using SupremeAI.Api.Models;

namespace SupremeAI.Api.Services;

/// <summary>
/// Persists and retrieves <see cref="JudgmentRecord"/> objects using a newline-delimited
/// JSON file (one JSON object per line, appended on write).  This approach keeps writes
/// O(1) and reads O(n) while requiring zero external dependencies.
///
/// The default storage path is <c>judgments.ndjson</c> next to the application's
/// content root.  Override with the <c>JudgmentStorePath</c> configuration key.
/// </summary>
public sealed class JudgmentStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        WriteIndented               = false,
        DefaultIgnoreCondition      = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly string _filePath;
    private readonly ILogger<JudgmentStore> _logger;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public JudgmentStore(IConfiguration configuration, IWebHostEnvironment env, ILogger<JudgmentStore> logger)
    {
        _logger = logger;

        var configured = configuration["JudgmentStorePath"];
        _filePath = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(env.ContentRootPath, "judgments.ndjson")
            : configured;

        _logger.LogInformation("JudgmentStore: persistence file = {Path}", _filePath);
    }

    /// <summary>Appends <paramref name="record"/> to the store.</summary>
    public async Task SaveAsync(JudgmentRecord record, CancellationToken ct = default)
    {
        var line = JsonSerializer.Serialize(record, JsonOptions);

        await _writeLock.WaitAsync(ct);
        try
        {
            await File.AppendAllTextAsync(_filePath, line + Environment.NewLine, ct);
            _logger.LogInformation("JudgmentStore: saved judgment {Id}", record.Id);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Returns the <paramref name="n"/> most-recent judgments ordered by descending
    /// <see cref="JudgmentRecord.Timestamp"/>.
    /// </summary>
    public async Task<List<JudgmentRecord>> GetRecentAsync(int n, CancellationToken ct = default)
    {
        await _writeLock.WaitAsync(ct);
        try
        {
            if (!File.Exists(_filePath))
                return [];

            var lines = await File.ReadAllLinesAsync(_filePath, ct);

            var records = new List<JudgmentRecord>(lines.Length);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                try
                {
                    var record = JsonSerializer.Deserialize<JudgmentRecord>(trimmed, JsonOptions);
                    if (record is not null) records.Add(record);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "JudgmentStore: could not parse line; skipping.");
                }
            }

            return records
                .OrderByDescending(r => r.Timestamp)
                .Take(n)
                .ToList();
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Returns all stored judgments ordered by ascending <see cref="JudgmentRecord.Timestamp"/>.
    /// </summary>
    public async Task<List<JudgmentRecord>> GetAllAsync(CancellationToken ct = default)
    {
        await _writeLock.WaitAsync(ct);
        try
        {
            if (!File.Exists(_filePath))
                return [];

            var lines = await File.ReadAllLinesAsync(_filePath, ct);

            var records = new List<JudgmentRecord>(lines.Length);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                try
                {
                    var record = JsonSerializer.Deserialize<JudgmentRecord>(trimmed, JsonOptions);
                    if (record is not null) records.Add(record);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "JudgmentStore: could not parse line; skipping.");
                }
            }

            return records.OrderBy(r => r.Timestamp).ToList();
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>Returns the total number of stored judgments.</summary>
    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        await _writeLock.WaitAsync(ct);
        try
        {
            if (!File.Exists(_filePath))
                return 0;

            var lines = await File.ReadAllLinesAsync(_filePath, ct);
            return lines.Count(l => !string.IsNullOrWhiteSpace(l));
        }
        finally
        {
            _writeLock.Release();
        }
    }
}
