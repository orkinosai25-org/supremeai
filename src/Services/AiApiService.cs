using System.Net.Http.Json;
using SupremeAI.Models;

namespace SupremeAI.Services;

/// <summary>
/// Client-side service that calls the SupremeAI backend API.
/// Returns null when the API is unreachable; callers should treat null as a
/// connectivity failure and surface a configuration error — not a demo response.
/// </summary>
public sealed class AiApiService
{
    private readonly HttpClient _http;

    // Backend API base path – relative to the Blazor app's base address.
    private const string ApiBase = "api/ai";

    public AiApiService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Sends a chat request to the backend API.
    /// Returns null on network error; the caller should surface a configuration error.
    /// </summary>
    public async Task<ApiChatResponse?> ChatAsync(
        string modelId,
        IEnumerable<ApiChatMessage> messages,
        CancellationToken ct = default)
    {
        try
        {
            var request = new
            {
                modelId,
                messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToList(),
            };

            var response = await _http.PostAsJsonAsync($"{ApiBase}/chat", request, ct);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ApiChatResponse>(cancellationToken: ct);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Sends an image generation request to the backend API.
    /// Returns null on network error; the caller should surface a configuration error.
    /// </summary>
    public async Task<ApiImageResponse?> ImageAsync(
        string modelId,
        string prompt,
        string size = "1024x1024",
        CancellationToken ct = default)
    {
        try
        {
            var request = new { modelId, prompt, size };
            var response = await _http.PostAsJsonAsync($"{ApiBase}/image", request, ct);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ApiImageResponse>(cancellationToken: ct);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Calls the SupremeAI Brain endpoint: fans the query to multiple models,
    /// scores each response, and returns the ranked results + winning answer.
    /// Returns null on network error; the caller should surface a configuration error.
    /// </summary>
    public async Task<ApiSupremeResponse?> SupremeAsync(
        string query,
        IEnumerable<string>? modelIds = null,
        CancellationToken ct = default)
    {
        try
        {
            var request = new
            {
                query,
                modelIds = modelIds?.ToList() ?? [],
            };
            var response = await _http.PostAsJsonAsync($"{ApiBase}/supreme", request, ct);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ApiSupremeResponse>(cancellationToken: ct);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Calls the Judgment Engine endpoint: fans the prompt to the panel models,
    /// scores responses, and returns a full <see cref="ApiJudgmentRecord"/> that
    /// includes the structured advisory recommendation and alternatives.
    /// Returns null on network error.
    /// </summary>
    public async Task<ApiJudgmentResponse?> JudgeAsync(
        string prompt,
        IEnumerable<string>? modelIds = null,
        CancellationToken ct = default)
    {
        try
        {
            var request = new
            {
                prompt,
                modelIds = modelIds?.ToList() ?? [],
            };
            var response = await _http.PostAsJsonAsync("supreme/judge", request, ct);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ApiJudgmentResponse>(cancellationToken: ct);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Records the user's advisory override for the given judgment.
    /// Returns null on network error.
    /// </summary>
    public async Task<ApiOverrideResponse?> SubmitOverrideAsync(
        string judgmentId,
        string selectedApproach,
        string? reason = null,
        CancellationToken ct = default)
    {
        try
        {
            var request = new { selectedApproach, reason };
            var response = await _http.PostAsJsonAsync($"supreme/judge/{judgmentId}/override", request, ct);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ApiOverrideResponse>(cancellationToken: ct);
        }
        catch
        {
            return null;
        }
    }
}

// ── Lightweight DTOs matching the backend API contract ────────────────────────

public sealed class ApiChatMessage
{
    public string Role    { get; init; } = "user";
    public string Content { get; init; } = "";
}

public sealed class ApiChatResponse
{
    public string  ModelId      { get; init; } = "";
    public string  Text         { get; init; } = "";
    public string  Status       { get; init; } = "done";
    public int     Tokens       { get; init; }
    public int     Ms           { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed class ApiImageResponse
{
    public string  ModelId      { get; init; } = "";
    public string  Status       { get; init; } = "done";
    public string  ImageUrl     { get; init; } = "";
    public string? RevisedPrompt{ get; init; }
    public string? ErrorMessage { get; init; }
}

// ── SupremeAI Brain DTOs ──────────────────────────────────────────────────────

public sealed class ApiModelEvalResult
{
    public string  ModelId      { get; init; } = "";
    public string  Text         { get; init; } = "";
    public string  Status       { get; init; } = "done";
    public int     Tokens       { get; init; }
    public int     Ms           { get; init; }
    public double  Score        { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed class ApiSupremeResponse
{
    public string                   Query         { get; init; } = "";
    public List<ApiModelEvalResult> Results       { get; init; } = [];
    public string                   WinnerId      { get; init; } = "";
    public string                   SupremeAnswer { get; init; } = "";
    public int                      TotalMs       { get; init; }
}

// ── Judgment Engine DTOs ──────────────────────────────────────────────────────

public sealed class ApiRecommendationAlternative
{
    public string Label    { get; init; } = "Alternative";
    public string Approach { get; init; } = "";
    public string Tradeoff { get; init; } = "";
}

public sealed class ApiJudgmentRecommendation
{
    public string                              Domain         { get; init; } = "";
    public string                              Recommendation { get; init; } = "";
    public bool                                IsRecommended  { get; init; } = true;
    public string                              Confidence     { get; init; } = "Low";
    public List<string>                        Reasons        { get; init; } = [];
    public string                              Caveat         { get; init; } = "";
    public List<ApiRecommendationAlternative>  Alternatives   { get; init; } = [];
}

public sealed class ApiJudgmentModelResult
{
    public string  ModelId      { get; init; } = "";
    public string  Answer       { get; init; } = "";
    public string  Status       { get; init; } = "done";
    public int     Ms           { get; init; }
    public int     Tokens       { get; init; }
    public double  Score        { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed class ApiJudgmentRecord
{
    public string                      Id             { get; init; } = "";
    public string                      Prompt         { get; init; } = "";
    public string                      WinnerId       { get; init; } = "";
    public string                      WinnerAnswer   { get; init; } = "";
    public string                      Rationale      { get; init; } = "";
    public ApiJudgmentRecommendation   Recommendation { get; init; } = new();
    public List<ApiJudgmentModelResult> ModelResults  { get; init; } = [];
    public DateTimeOffset              Timestamp      { get; init; }
}

public sealed class ApiJudgmentResponse
{
    public ApiJudgmentRecord Judgment { get; init; } = new();
}

public sealed class ApiUserOverrideRecord
{
    public string  Id                     { get; init; } = "";
    public string  JudgmentId             { get; init; } = "";
    public string  SystemRecommendation   { get; init; } = "";
    public string  SelectedApproach       { get; init; } = "";
    public bool    IsSystemRecommendation { get; init; }
    public string? Reason                 { get; init; }
    public DateTimeOffset Timestamp       { get; init; }
}

public sealed class ApiOverrideResponse
{
    public ApiUserOverrideRecord Override { get; init; } = new();
}
