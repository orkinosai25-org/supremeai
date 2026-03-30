using Azure;
using Azure.AI.Inference;
using SupremeAI.Api.Models;
using ApiChatMessage = SupremeAI.Api.Models.ChatMessage;

namespace SupremeAI.Api.Services;

/// <summary>
/// Provider for Azure AI Foundry serverless inference endpoints.
/// Handles: Phi-3.5 Mini, Phi-3 Medium, Llama 3.1 70B, Mistral Large, Command R+, Jais 30B.
/// Each model has its own endpoint URL + API key stored in configuration / environment variables.
/// </summary>
public sealed class AzureAiInferenceProvider : IModelProvider
{
    /// <summary>
    /// Maps SupremeAI model ID → (endpoint config key, api key config key).
    /// Configuration keys map to environment variables or appsettings values.
    /// </summary>
    private static readonly Dictionary<string, (string EndpointKey, string ApiKeyKey)> ModelConfig =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["phi-3-5-mini"]   = ("PHI_35_MINI_ENDPOINT",  "PHI_35_MINI_API_KEY"),
            ["phi-3-medium"]   = ("PHI_3_MEDIUM_ENDPOINT", "PHI_3_MEDIUM_API_KEY"),
            ["llama-3-1-70b"]  = ("LLAMA_31_70B_ENDPOINT", "LLAMA_31_70B_API_KEY"),
            ["mistral-large"]  = ("MISTRAL_LARGE_ENDPOINT","MISTRAL_LARGE_API_KEY"),
            ["command-r-plus"] = ("COMMAND_R_PLUS_ENDPOINT","COMMAND_R_PLUS_API_KEY"),
            ["jais-30b"]       = ("JAIS_30B_ENDPOINT",     "JAIS_30B_API_KEY"),
        };

    private readonly IConfiguration _config;
    private readonly ILogger<AzureAiInferenceProvider> _logger;

    public AzureAiInferenceProvider(IConfiguration config, ILogger<AzureAiInferenceProvider> logger)
    {
        _config = config;
        _logger = logger;
    }

    public bool CanHandle(string modelId) => ModelConfig.ContainsKey(modelId);

    public async Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken ct = default)
    {
        if (!ModelConfig.TryGetValue(request.ModelId, out var keys))
            return Error(request.ModelId, $"Unknown model '{request.ModelId}' for AzureAiInferenceProvider.");

        var endpoint = _config[keys.EndpointKey];
        var apiKey   = _config[keys.ApiKeyKey];

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
        {
            return Error(request.ModelId,
                $"Endpoint or API key not configured for '{request.ModelId}'. " +
                $"Set {keys.EndpointKey} and {keys.ApiKeyKey}.");
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var client = new ChatCompletionsClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey));

            var inferenceMessages = request.Messages
                .Select<ApiChatMessage, ChatRequestMessage>(m => m.Role switch
                {
                    "system"    => new ChatRequestSystemMessage(m.Content),
                    "assistant" => new ChatRequestAssistantMessage(m.Content),
                    _           => new ChatRequestUserMessage(m.Content),
                })
                .ToList();

            var options = new ChatCompletionsOptions(inferenceMessages);
            if (request.MaxTokens > 0)   options.MaxTokens   = request.MaxTokens;
            if (request.Temperature > 0) options.Temperature = (float)request.Temperature;

            var response = await client.CompleteAsync(options, ct);

            sw.Stop();
            return new ChatResponse
            {
                ModelId = request.ModelId,
                Text    = response.Value.Content,
                Status  = "done",
                Tokens  = response.Value.Usage?.TotalTokens ?? 0,
                Ms      = (int)sw.ElapsedMilliseconds,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AzureAiInferenceProvider chat error for model {ModelId}", request.ModelId);
            return Error(request.ModelId, ex.Message, (int)sw.ElapsedMilliseconds);
        }
    }

    public Task<ImageResponse> ImageAsync(ImageRequest request, CancellationToken ct = default) =>
        Task.FromResult(new ImageResponse
        {
            ModelId      = request.ModelId,
            Status       = "error",
            ErrorMessage = $"Image generation is not supported by '{request.ModelId}'.",
        });

    private static ChatResponse Error(string modelId, string msg, int ms = 0) =>
        new() { ModelId = modelId, Status = "error", ErrorMessage = msg, Ms = ms };
}
