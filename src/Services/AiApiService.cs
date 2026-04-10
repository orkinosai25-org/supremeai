using System.Net.Http.Json;
using SupremeAI.Models;

namespace SupremeAI.Services;

/// <summary>
/// Client-side service that calls the SupremeAI backend API.
/// Falls back to demo/mock responses when the API is unreachable.
/// </summary>
public sealed class AiApiService
{
    private readonly HttpClient _http;

    // Backend API base path – relative to the Blazor app's base address.
    // Override by setting the "ApiBaseUrl" configuration key, or point to your
    // deployed API (e.g. https://api.supremeai.example.com).
    private const string ApiBase = "api/ai";

    public AiApiService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Sends a chat request to the backend API.
    /// Returns null on network error so the caller can fall back to demo mode.
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
    /// Returns null on network error so the caller can fall back to demo mode.
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
    /// Returns null on network error so the caller can fall back to demo mode.
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
