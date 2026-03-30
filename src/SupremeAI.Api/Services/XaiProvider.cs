using System.Text;
using System.Text.Json;
using SupremeAI.Api.Models;

namespace SupremeAI.Api.Services;

/// <summary>
/// Provider for xAI Grok models (grok-2).
/// Uses xAI's OpenAI-compatible REST API.
/// </summary>
public sealed class XaiProvider : IModelProvider
{
    private static readonly HashSet<string> SupportedModels = new(StringComparer.OrdinalIgnoreCase)
    {
        "grok-2",
    };

    // Maps SupremeAI model IDs → xAI model identifiers
    private static readonly Dictionary<string, string> ModelMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["grok-2"] = "grok-2-latest",
    };

    private const string ApiBaseUrl = "https://api.x.ai/v1/chat/completions";

    private readonly HttpClient _http;
    private readonly string? _apiKey;
    private readonly ILogger<XaiProvider> _logger;

    public XaiProvider(IHttpClientFactory httpFactory, IConfiguration config, ILogger<XaiProvider> logger)
    {
        _http   = httpFactory.CreateClient("xai");
        _logger = logger;
        _apiKey = config["XAI_GROK_API_KEY"] ?? config["Xai:GrokApiKey"];

        if (string.IsNullOrWhiteSpace(_apiKey))
            _logger.LogWarning("XaiProvider: XAI_GROK_API_KEY not set – provider disabled.");
    }

    public bool CanHandle(string modelId) => SupportedModels.Contains(modelId);

    public async Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return Error(request.ModelId, "xAI Grok API key not configured. Set XAI_GROK_API_KEY.");

        if (!ModelMap.TryGetValue(request.ModelId, out var xaiModel))
            return Error(request.ModelId, $"Unknown model '{request.ModelId}'.");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // xAI uses OpenAI-compatible messages format
            var messages = request.Messages
                .Select(m => new { role = m.Role, content = m.Content })
                .ToList();

            var body = new Dictionary<string, object?>
            {
                ["model"]    = xaiModel,
                ["messages"] = messages,
            };
            if (request.MaxTokens > 0)   body["max_tokens"]  = request.MaxTokens;
            if (request.Temperature > 0) body["temperature"] = request.Temperature;

            using var reqMsg = new HttpRequestMessage(HttpMethod.Post, ApiBaseUrl);
            reqMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            reqMsg.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            using var httpResponse = await _http.SendAsync(reqMsg, ct);
            var json = await httpResponse.Content.ReadAsStringAsync(ct);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("xAI API error {StatusCode}: {Body}", httpResponse.StatusCode, json);
                return Error(request.ModelId, $"xAI API error {(int)httpResponse.StatusCode}: {json}", (int)sw.ElapsedMilliseconds);
            }

            using var doc = JsonDocument.Parse(json);
            var text   = doc.RootElement.GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString() ?? "";

            var tokens = 0;
            if (doc.RootElement.TryGetProperty("usage", out var usage))
                tokens = usage.TryGetProperty("total_tokens", out var t) ? t.GetInt32() : 0;

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
            _logger.LogError(ex, "XaiProvider chat error for model {ModelId}", request.ModelId);
            return Error(request.ModelId, ex.Message, (int)sw.ElapsedMilliseconds);
        }
    }

    public Task<ImageResponse> ImageAsync(ImageRequest request, CancellationToken ct = default) =>
        Task.FromResult(new ImageResponse
        {
            ModelId      = request.ModelId,
            Status       = "error",
            ErrorMessage = "Image generation is not supported by xAI Grok.",
        });

    private static ChatResponse Error(string modelId, string msg, int ms = 0) =>
        new() { ModelId = modelId, Status = "error", ErrorMessage = msg, Ms = ms };
}
