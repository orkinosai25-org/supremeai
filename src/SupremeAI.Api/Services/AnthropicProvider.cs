using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SupremeAI.Api.Models;

namespace SupremeAI.Api.Services;

/// <summary>
/// Provider for Anthropic Claude models (claude-3-5-sonnet).
/// Uses the Anthropic Messages REST API directly.
/// </summary>
public sealed class AnthropicProvider : IModelProvider
{
    private static readonly HashSet<string> SupportedModels = new(StringComparer.OrdinalIgnoreCase)
    {
        "claude-3-5-sonnet",
    };

    // Maps SupremeAI model IDs → Anthropic model identifiers
    private static readonly Dictionary<string, string> ModelMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["claude-3-5-sonnet"] = "claude-3-5-sonnet-20241022",
    };

    private const string ApiBaseUrl = "https://api.anthropic.com/v1/messages";
    private const string AnthropicVersion = "2023-06-01";

    private readonly HttpClient _http;
    private readonly string? _apiKey;
    private readonly ILogger<AnthropicProvider> _logger;

    public AnthropicProvider(IHttpClientFactory httpFactory, IConfiguration config, ILogger<AnthropicProvider> logger)
    {
        _http   = httpFactory.CreateClient("anthropic");
        _logger = logger;
        _apiKey = config["ANTHROPIC_API_KEY"] ?? config["Anthropic:ApiKey"];

        if (string.IsNullOrWhiteSpace(_apiKey))
            _logger.LogWarning("AnthropicProvider: ANTHROPIC_API_KEY not set – provider disabled.");
    }

    public bool CanHandle(string modelId) => SupportedModels.Contains(modelId);

    public async Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return Error(request.ModelId, "Anthropic API key not configured. Set ANTHROPIC_API_KEY.");

        if (!ModelMap.TryGetValue(request.ModelId, out var anthropicModel))
            return Error(request.ModelId, $"Unknown model '{request.ModelId}'.");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // Separate system prompt from chat messages
            var systemMsg = request.Messages
                .Where(m => m.Role == "system")
                .Select(m => m.Content)
                .FirstOrDefault();

            var chatMessages = request.Messages
                .Where(m => m.Role != "system")
                .Select(m => new { role = m.Role, content = m.Content })
                .ToList();

            var body = new Dictionary<string, object?>
            {
                ["model"]      = anthropicModel,
                ["max_tokens"] = request.MaxTokens > 0 ? request.MaxTokens : 4096,
                ["messages"]   = chatMessages,
            };
            if (!string.IsNullOrWhiteSpace(systemMsg)) body["system"] = systemMsg;
            if (request.Temperature > 0) body["temperature"] = request.Temperature;

            using var reqMsg = new HttpRequestMessage(HttpMethod.Post, ApiBaseUrl);
            reqMsg.Headers.Add("x-api-key", _apiKey);
            reqMsg.Headers.Add("anthropic-version", AnthropicVersion);
            reqMsg.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            using var httpResponse = await _http.SendAsync(reqMsg, ct);
            var json = await httpResponse.Content.ReadAsStringAsync(ct);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Anthropic API error {StatusCode}: {Body}", httpResponse.StatusCode, json);
                return Error(request.ModelId, $"Anthropic API error {(int)httpResponse.StatusCode}: {json}", (int)sw.ElapsedMilliseconds);
            }

            using var doc = JsonDocument.Parse(json);
            var text   = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString() ?? "";
            var tokens = doc.RootElement.TryGetProperty("usage", out var usage)
                ? usage.GetProperty("input_tokens").GetInt32() + usage.GetProperty("output_tokens").GetInt32()
                : 0;

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
            _logger.LogError(ex, "AnthropicProvider chat error for model {ModelId}", request.ModelId);
            return Error(request.ModelId, ex.Message, (int)sw.ElapsedMilliseconds);
        }
    }

    public Task<ImageResponse> ImageAsync(ImageRequest request, CancellationToken ct = default) =>
        Task.FromResult(new ImageResponse
        {
            ModelId      = request.ModelId,
            Status       = "error",
            ErrorMessage = "Image generation is not supported by Anthropic.",
        });

    private static ChatResponse Error(string modelId, string msg, int ms = 0) =>
        new() { ModelId = modelId, Status = "error", ErrorMessage = msg, Ms = ms };
}
