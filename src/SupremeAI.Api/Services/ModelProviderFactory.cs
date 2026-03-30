using SupremeAI.Api.Models;

namespace SupremeAI.Api.Services;

/// <summary>
/// Routes incoming requests to the correct <see cref="IModelProvider"/> implementation.
/// </summary>
public sealed class ModelProviderFactory
{
    private readonly IReadOnlyList<IModelProvider> _providers;
    private readonly ILogger<ModelProviderFactory> _logger;

    public ModelProviderFactory(
        IEnumerable<IModelProvider> providers,
        ILogger<ModelProviderFactory> logger)
    {
        _providers = providers.ToList();
        _logger    = logger;
    }

    /// <summary>Returns the provider that handles <paramref name="modelId"/>, or null.</summary>
    public IModelProvider? GetProvider(string modelId)
    {
        var provider = _providers.FirstOrDefault(p => p.CanHandle(modelId));
        if (provider is null)
            _logger.LogWarning("No provider found for model '{ModelId}'.", modelId);
        return provider;
    }

    /// <summary>Dispatches a chat request, returning an error response if no provider is found.</summary>
    public async Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken ct = default)
    {
        var provider = GetProvider(request.ModelId);
        if (provider is null)
        {
            return new ChatResponse
            {
                ModelId      = request.ModelId,
                Status       = "error",
                ErrorMessage = $"No provider is configured for model '{request.ModelId}'.",
            };
        }
        return await provider.ChatAsync(request, ct);
    }

    /// <summary>Dispatches an image generation request.</summary>
    public async Task<ImageResponse> ImageAsync(ImageRequest request, CancellationToken ct = default)
    {
        var provider = GetProvider(request.ModelId);
        if (provider is null)
        {
            return new ImageResponse
            {
                ModelId      = request.ModelId,
                Status       = "error",
                ErrorMessage = $"No provider is configured for model '{request.ModelId}'.",
            };
        }
        return await provider.ImageAsync(request, ct);
    }
}
