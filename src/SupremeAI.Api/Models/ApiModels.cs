namespace SupremeAI.Api.Models;

/// <summary>A single chat message (user or assistant turn).</summary>
public sealed class ChatMessage
{
    public string Role { get; set; } = "user";    // "user" | "assistant" | "system"
    public string Content { get; set; } = "";
}

/// <summary>Incoming chat completion request from the frontend.</summary>
public sealed class ChatRequest
{
    /// <summary>Model ID as defined in the frontend's ModelCatalogue.</summary>
    public string ModelId { get; set; } = "";

    /// <summary>Conversation history. The last message is the current user prompt.</summary>
    public List<ChatMessage> Messages { get; set; } = [];

    /// <summary>Maximum tokens to generate (0 = use provider default).</summary>
    public int MaxTokens { get; set; } = 0;

    /// <summary>Temperature 0–2 (0 = use provider default).</summary>
    public double Temperature { get; set; } = 0;
}

/// <summary>Incoming image generation request.</summary>
public sealed class ImageRequest
{
    /// <summary>Model ID (dalle-3, siu, sdxl).</summary>
    public string ModelId { get; set; } = "";

    /// <summary>Text prompt for the image.</summary>
    public string Prompt { get; set; } = "";

    /// <summary>Image size (e.g. "1024x1024").</summary>
    public string Size { get; set; } = "1024x1024";
}

/// <summary>Chat completion response returned to the frontend.</summary>
public sealed class ChatResponse
{
    /// <summary>Model ID that produced this response.</summary>
    public string ModelId { get; set; } = "";

    /// <summary>Generated text.</summary>
    public string Text { get; set; } = "";

    /// <summary>"done" | "error"</summary>
    public string Status { get; set; } = "done";

    /// <summary>Approximate token count.</summary>
    public int Tokens { get; set; }

    /// <summary>Latency in milliseconds.</summary>
    public int Ms { get; set; }

    /// <summary>Error message if Status == "error".</summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>Image generation response.</summary>
public sealed class ImageResponse
{
    public string ModelId { get; set; } = "";
    public string Status { get; set; } = "done";

    /// <summary>URL or base64-encoded image.</summary>
    public string ImageUrl { get; set; } = "";

    /// <summary>Revised prompt returned by DALL-E.</summary>
    public string? RevisedPrompt { get; set; }

    public string? ErrorMessage { get; set; }
}

/// <summary>API error response envelope.</summary>
public sealed class ErrorResponse
{
    public string Error { get; set; } = "";
}

// ── SupremeAI Brain ──────────────────────────────────────────────────────────

/// <summary>
/// Request to the SupremeAI Brain.
/// The Brain fans the prompt out to all specified models, scores each response,
/// and returns the winning answer together with per-model evaluation data.
/// </summary>
public sealed class SupremeRequest
{
    /// <summary>The prompt to benchmark across all selected models.</summary>
    public string Query { get; set; } = "";

    /// <summary>
    /// Model IDs to evaluate. Leave empty to use the default panel
    /// (gpt-4o, llama-3-1-70b, mistral-large).
    /// </summary>
    public List<string> ModelIds { get; set; } = [];

    /// <summary>Maximum tokens per model (0 = provider default).</summary>
    public int MaxTokens { get; set; } = 0;

    /// <summary>Temperature 0–2 (0 = provider default).</summary>
    public double Temperature { get; set; } = 0;
}

/// <summary>Per-model evaluation result produced by the SupremeAI Brain.</summary>
public sealed class ModelEvalResult
{
    /// <summary>Model that produced this result.</summary>
    public string ModelId { get; set; } = "";

    /// <summary>Generated text (empty on error).</summary>
    public string Text { get; set; } = "";

    /// <summary>"done" | "error"</summary>
    public string Status { get; set; } = "done";

    /// <summary>Total tokens used.</summary>
    public int Tokens { get; set; }

    /// <summary>Latency in milliseconds.</summary>
    public int Ms { get; set; }

    /// <summary>Brain quality score (0–10).</summary>
    public double Score { get; set; }

    /// <summary>Error detail when Status == "error".</summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Response from the SupremeAI Brain containing all model evaluations
/// and the final winning answer.
/// </summary>
public sealed class SupremeResponse
{
    /// <summary>Original query that was evaluated.</summary>
    public string Query { get; set; } = "";

    /// <summary>Per-model results ordered by descending score.</summary>
    public List<ModelEvalResult> Results { get; set; } = [];

    /// <summary>Model ID of the winner.</summary>
    public string WinnerId { get; set; } = "";

    /// <summary>The winning model's answer — the Supreme Answer.</summary>
    public string SupremeAnswer { get; set; } = "";

    /// <summary>Total wall-clock time for the entire evaluation (ms).</summary>
    public int TotalMs { get; set; }
}
