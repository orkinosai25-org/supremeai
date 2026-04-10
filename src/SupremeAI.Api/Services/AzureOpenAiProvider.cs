using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using SupremeAI.Api.Models;
using ApiChatMessage = SupremeAI.Api.Models.ChatMessage;

namespace SupremeAI.Api.Services;

/// <summary>
/// Provider for Azure OpenAI models: GPT-4o, GPT-4o-mini, o1-preview, DALL-E 3.
/// </summary>
public sealed class AzureOpenAiProvider : IModelProvider
{
    // Maps SupremeAI model IDs → Azure OpenAI deployment names
    private static readonly Dictionary<string, string> ChatDeployments = new(StringComparer.OrdinalIgnoreCase)
    {
        ["gpt-4o"]      = "gpt-4o",
        ["o1-preview"]  = "o1-preview",
        ["gpt-4o-mini"] = "gpt-4o-mini",
    };

    private static readonly HashSet<string> ImageModelIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "dalle-3",
    };

    private readonly AzureOpenAIClient? _client;
    private readonly ILogger<AzureOpenAiProvider> _logger;

    public AzureOpenAiProvider(IConfiguration config, ILogger<AzureOpenAiProvider> logger)
    {
        _logger = logger;

        var endpoint = config["AZURE_OPENAI_ENDPOINT"] ?? config["AzureOpenAI:Endpoint"];
        var apiKey   = config["AZURE_OPENAI_API_KEY"]  ?? config["AzureOpenAI:ApiKey"];

        if (!string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(apiKey))
        {
            _client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
            _logger.LogInformation("AzureOpenAiProvider: configured with endpoint {Endpoint}", endpoint);
        }
        else
        {
            _logger.LogWarning("AzureOpenAiProvider: AZURE_OPENAI_ENDPOINT or AZURE_OPENAI_API_KEY not set – provider disabled.");
        }
    }

    public bool CanHandle(string modelId) =>
        ChatDeployments.ContainsKey(modelId) || ImageModelIds.Contains(modelId);

    public async Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken ct = default)
    {
        if (_client is null)
            return Error(request.ModelId, "Azure OpenAI is not configured. Set AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_API_KEY.");

        if (!ChatDeployments.TryGetValue(request.ModelId, out var deployment))
            return Error(request.ModelId, $"Model '{request.ModelId}' is not a chat model handled by AzureOpenAiProvider.");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var chatClient = _client.GetChatClient(deployment);

            var messages = request.Messages
                .Select<ApiChatMessage, OpenAI.Chat.ChatMessage>(m => m.Role switch
                {
                    "system"    => OpenAI.Chat.ChatMessage.CreateSystemMessage(m.Content),
                    "assistant" => OpenAI.Chat.ChatMessage.CreateAssistantMessage(m.Content),
                    _           => OpenAI.Chat.ChatMessage.CreateUserMessage(m.Content),
                })
                .ToList();

            var options = new ChatCompletionOptions();
            if (request.MaxTokens > 0)   options.MaxOutputTokenCount = request.MaxTokens;
            if (request.Temperature > 0) options.Temperature = (float)request.Temperature;

            var completion = await chatClient.CompleteChatAsync(messages, options, ct);

            sw.Stop();
            return new ChatResponse
            {
                ModelId = request.ModelId,
                Text    = completion.Value.Content[0].Text,
                Status  = "done",
                Tokens  = completion.Value.Usage?.TotalTokenCount ?? 0,
                Ms      = (int)sw.ElapsedMilliseconds,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AzureOpenAiProvider chat error for model {ModelId}", request.ModelId);
            return Error(request.ModelId, ex.Message, (int)sw.ElapsedMilliseconds);
        }
    }

    public async Task<ImageResponse> ImageAsync(ImageRequest request, CancellationToken ct = default)
    {
        if (_client is null)
            return ImageError(request.ModelId, "Azure OpenAI is not configured.");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var imageClient = _client.GetImageClient("dall-e-3");

            var options = new ImageGenerationOptions
            {
                Size    = request.Size switch
                {
                    "1792x1024" => GeneratedImageSize.W1792xH1024,
                    "1024x1792" => GeneratedImageSize.W1024xH1792,
                    _           => GeneratedImageSize.W1024xH1024,
                },
                Quality = GeneratedImageQuality.Standard,
                ResponseFormat = GeneratedImageFormat.Uri,
            };

            var result = await imageClient.GenerateImageAsync(request.Prompt, options, ct);

            sw.Stop();
            return new ImageResponse
            {
                ModelId       = request.ModelId,
                Status        = "done",
                ImageUrl      = result.Value.ImageUri?.ToString() ?? "",
                RevisedPrompt = result.Value.RevisedPrompt,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AzureOpenAiProvider image error for model {ModelId}", request.ModelId);
            return ImageError(request.ModelId, ex.Message);
        }
    }

    private static ChatResponse Error(string modelId, string msg, int ms = 0) =>
        new() { ModelId = modelId, Status = "error", ErrorMessage = msg, Ms = ms };

    private static ImageResponse ImageError(string modelId, string msg) =>
        new() { ModelId = modelId, Status = "error", ErrorMessage = msg };
}
