using Microsoft.AspNetCore.Mvc;
using SupremeAI.Api.Models;
using SupremeAI.Api.Services;

namespace SupremeAI.Api.Controllers;

/// <summary>
/// AI model API endpoints consumed by the SupremeAI Blazor WASM frontend.
/// </summary>
[ApiController]
[Route("api/ai")]
[Produces("application/json")]
public sealed class AiController : ControllerBase
{
    private readonly ModelProviderFactory _factory;
    private readonly ILogger<AiController> _logger;

    public AiController(ModelProviderFactory factory, ILogger<AiController> logger)
    {
        _factory = factory;
        _logger  = logger;
    }

    // ── GET /api/ai/models ────────────────────────────────────────────────────

    /// <summary>Returns all available model IDs and their enabled status.</summary>
    [HttpGet("models")]
    public IActionResult GetModels()
    {
        // This mirrors the ModelCatalogue from the frontend so the API can also
        // report which models are available (useful for health checks / admin UIs).
        var models = new[]
        {
            // Chat – Azure OpenAI
            new { id = "gpt-4o",          provider = "Azure OpenAI",   type = "chat"  },
            new { id = "o1-preview",       provider = "Azure OpenAI",   type = "chat"  },
            new { id = "gpt-4o-mini",      provider = "Azure OpenAI",   type = "chat"  },
            // Chat – Azure AI Inference
            new { id = "phi-3-5-mini",     provider = "Microsoft",      type = "chat"  },
            new { id = "phi-3-medium",     provider = "Microsoft",      type = "chat"  },
            new { id = "llama-3-1-70b",    provider = "Meta",           type = "chat"  },
            new { id = "mistral-large",    provider = "Mistral AI",     type = "chat"  },
            new { id = "command-r-plus",   provider = "Cohere",         type = "chat"  },
            new { id = "jais-30b",         provider = "Core42",         type = "chat"  },
            // Chat – third-party REST
            new { id = "claude-3-5-sonnet",provider = "Anthropic",      type = "chat"  },
            new { id = "gemini-1-5-pro",   provider = "Google",         type = "chat"  },
            new { id = "grok-2",           provider = "xAI",            type = "chat"  },
            // Image
            new { id = "dalle-3",          provider = "Azure OpenAI",   type = "image" },
        };

        return Ok(models);
    }

    // ── POST /api/ai/chat ─────────────────────────────────────────────────────

    /// <summary>Generates a chat completion from the specified model.</summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ModelId))
            return BadRequest(new ErrorResponse { Error = "ModelId is required." });

        if (request.Messages is not { Count: > 0 })
            return BadRequest(new ErrorResponse { Error = "At least one message is required." });

        _logger.LogInformation("Chat request: model={ModelId}, messages={Count}",
            request.ModelId, request.Messages.Count);

        var response = await _factory.ChatAsync(request, ct);
        return Ok(response);
    }

    // ── POST /api/ai/image ────────────────────────────────────────────────────

    /// <summary>Generates an image from the specified model and prompt.</summary>
    [HttpPost("image")]
    public async Task<IActionResult> Image([FromBody] ImageRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ModelId))
            return BadRequest(new ErrorResponse { Error = "ModelId is required." });

        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest(new ErrorResponse { Error = "Prompt is required." });

        _logger.LogInformation("Image request: model={ModelId}", request.ModelId);

        var response = await _factory.ImageAsync(request, ct);
        return Ok(response);
    }
}
