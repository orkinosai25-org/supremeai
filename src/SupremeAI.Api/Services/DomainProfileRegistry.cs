using System.Text.Json;
using System.Text.Json.Serialization;
using SupremeAI.Api.Models;

namespace SupremeAI.Api.Services;

/// <summary>
/// Loads and provides access to the canonical domain authority profiles.
///
/// Profiles are read from <c>domain_profiles.json</c> at startup and cached
/// for the lifetime of the process.  The file is looked up in the content root
/// first (deployed environments) then in <see cref="AppContext.BaseDirectory"/>
/// (local development).
///
/// When the file cannot be found or parsed the registry falls back to an empty
/// collection so the rest of the application continues to function without
/// domain-profile enrichment.
/// </summary>
public sealed class DomainProfileRegistry
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        // Allow trailing commas in the JSON file for ease of editing.
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly IReadOnlyDictionary<string, DomainAuthorityProfile> _profiles;

    public DomainProfileRegistry(IWebHostEnvironment env, ILogger<DomainProfileRegistry> logger)
    {
        _profiles = LoadProfiles(env, logger);
    }

    /// <summary>
    /// Returns the domain authority profile for the given domain key,
    /// or <c>null</c> if no profile is registered for that domain.
    /// </summary>
    public DomainAuthorityProfile? GetProfile(string domain) =>
        _profiles.TryGetValue(domain, out var profile) ? profile : null;

    /// <summary>Returns all registered domain authority profiles, ordered by domain key.</summary>
    public IReadOnlyList<DomainAuthorityProfile> GetAll() =>
        [.. _profiles.Values.OrderBy(p => p.Domain)];

    // ── Loader ────────────────────────────────────────────────────────────────

    private static IReadOnlyDictionary<string, DomainAuthorityProfile> LoadProfiles(
        IWebHostEnvironment env,
        ILogger logger)
    {
        const string FileName = "domain_profiles.json";

        // Resolution order: content root (deployed) → base directory (dev/test)
        var candidates = new[]
        {
            Path.Combine(env.ContentRootPath, FileName),
            Path.Combine(AppContext.BaseDirectory, FileName),
        };

        foreach (var path in candidates)
        {
            if (!File.Exists(path))
                continue;

            try
            {
                var json = File.ReadAllText(path);
                var raw  = JsonSerializer.Deserialize<Dictionary<string, DomainAuthorityProfile>>(
                    json, JsonOptions);

                if (raw is { Count: > 0 })
                {
                    logger.LogInformation(
                        "DomainProfileRegistry: loaded {Count} domain profile(s) from {Path}",
                        raw.Count, path);
                    return raw.AsReadOnly();
                }

                logger.LogWarning(
                    "DomainProfileRegistry: file found at {Path} but contained no profiles", path);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "DomainProfileRegistry: failed to parse domain profiles from {Path}", path);
            }
        }

        logger.LogWarning(
            "DomainProfileRegistry: {File} not found in any search location — " +
            "domain-profile enrichment will be unavailable", FileName);

        return new Dictionary<string, DomainAuthorityProfile>().AsReadOnly();
    }
}
