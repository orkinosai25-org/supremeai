using System.Text;
using System.Text.Json;
using SupremeAI.Api.Models;

namespace SupremeAI.Api.Services;

/// <summary>
/// Provider for Google Gemini models (gemini-1-5-pro).
/// Uses the Gemini REST API (generateContent endpoint).
/// </summary>
public sealed class GoogleProvider : IModelProvider
{
    private static readonly HashSet<string> SupportedModels = new(StringComparer.OrdinalIgnoreCase)
    {
        "gemini-1-5-pro",
    };

    // Maps SupremeAI model IDs → Gemini API model identifiers
    private static readonly Dictionary<string, string> ModelMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["gemini-1-5-pro"] = "gemini-1.5-pro",
    };

    private const string ApiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    private readonly HttpClient _http;
    private readonly string? _apiKey;
    private readonly ILogger<GoogleProvider> _logger;

    public GoogleProvider(IHttpClientFactory httpFactory, IConfiguration config, ILogger<GoogleProvider> logger)
    {
        _http   = httpFactory.CreateClient("google");
        _logger = logger;
        _apiKey = config["GOOGLE_GEMINI_API_KEY"] ?? config["Google:GeminiApiKey"];

        if (string.IsNullOrWhiteSpace(_apiKey))
            _logger.LogWarning("GoogleProvider: GOOGLE_GEMINI_API_KEY not set – provider disabled.");
    }

    public bool CanHandle(string modelId) => SupportedModels.Contains(modelId);

    public async Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return Error(request.ModelId, "Google Gemini API key not configured. Set GOOGLE_GEMINI_API_KEY.");

        if (!ModelMap.TryGetValue(request.ModelId, out var geminiModel))
            return Error(request.ModelId, $"Unknown model '{request.ModelId}'.");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // Build Gemini contents array (user/model turns only; system handled via systemInstruction)
            var systemMsg = request.Messages
                .Where(m => m.Role == "system")
                .Select(m => m.Content)
                .FirstOrDefault();

            var contents = request.Messages
                .Where(m => m.Role != "system")
                .Select(m => new
                {
                    role  = m.Role == "assistant" ? "model" : "user",
                    parts = new[] { new { text = m.Content } },
                })
                .ToList();

            var body = new Dictionary<string, object?>
            {
                ["contents"] = contents,
            };
            if (!string.IsNullOrWhiteSpace(systemMsg))
            {
                body["systemInstruction"] = new
                {
                    parts = new[] { new { text = systemMsg } },
                };
            }

            var genConfig = new Dictionary<string, object?>();
            if (request.MaxTokens > 0)   genConfig["maxOutputTokens"] = request.MaxTokens;
            if (request.Temperature > 0) genConfig["temperature"]     = request.Temperature;
            if (genConfig.Count > 0)     body["generationConfig"]     = genConfig;

            var url = $"{ApiBaseUrl}/{geminiModel}:generateContent?key={_apiKey}";

            using var reqMsg = new HttpRequestMessage(HttpMethod.Post, url);
            reqMsg.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            using var httpResponse = await _http.SendAsync(reqMsg, ct);
            var json = await httpResponse.Content.ReadAsStringAsync(ct);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Google Gemini API error {StatusCode}: {Body}", httpResponse.StatusCode, json);
                return Error(request.ModelId, $"Gemini API error {(int)httpResponse.StatusCode}: {json}", (int)sw.ElapsedMilliseconds);
            }

            using var doc = JsonDocument.Parse(json);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            var tokens = 0;
            if (doc.RootElement.TryGetProperty("usageMetadata", out var meta))
            {
                tokens = meta.TryGetProperty("totalTokenCount", out var t) ? t.GetInt32() : 0;
            }

            sw.Stop();
            return new ChatResponse
            {
                ModelId = request.ModelId,
                Text    = text,
                Status  = "done",
                Tokens  = tokens,
                Ms      = (int)sw.ElapsedMilliseconds,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GoogleProvider chat error for model {ModelId}", request.ModelId);
            return Error(request.ModelId, ex.Message, (int)sw.ElapsedMilliseconds);
        }
    }

    public Task<ImageResponse> ImageAsync(ImageRequest request, CancellationToken ct = default) =>
        Task.FromResult(new ImageResponse
        {
            ModelId      = request.ModelId,
            Status       = "error",
            ErrorMessage = "Image generation is not supported by Google Gemini via this provider.",
        });

    private static ChatResponse Error(string modelId, string msg, int ms = 0) =>
        new() { ModelId = modelId, Status = "error", ErrorMessage = msg, Ms = ms };
}
