using SupremeAI.Api.Models;

namespace SupremeAI.Api.Services;

/// <summary>
/// Implemented by each AI provider (Azure OpenAI, Anthropic, Google, xAI, Azure AI Inference).
/// </summary>
public interface IModelProvider
{
    /// <summary>Returns true if this provider handles the given model ID.</summary>
    bool CanHandle(string modelId);

    /// <summary>Generates a chat completion for the given request.</summary>
    Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken ct = default);

    /// <summary>Generates an image for the given request (not all providers support this).</summary>
    Task<ImageResponse> ImageAsync(ImageRequest request, CancellationToken ct = default);
}
