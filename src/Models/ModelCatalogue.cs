namespace SupremeAI.Models;

public static class ModelCatalogue
{
    public static readonly IReadOnlyList<AiModel> ChatModels = new List<AiModel>
    {
        // ── Diamond ────────────────────────────────────────────────────────────
        new() { Id = "gpt-4o",              Name = "GPT-4o",            Provider = "Azure OpenAI",  Tier = ModelTier.Diamond, Color = "#10A37F", Initial = "G4",  DefaultSelected = true },
        new() { Id = "o1-preview",          Name = "o1 Preview",        Provider = "Azure OpenAI",  Tier = ModelTier.Diamond, Color = "#10A37F", Initial = "o1" },
        new() { Id = "claude-3-5-sonnet",   Name = "Claude 3.5 Sonnet", Provider = "Anthropic",     Tier = ModelTier.Diamond, Color = "#CC785C", Initial = "C3",  DefaultSelected = true },
        new() { Id = "gemini-1-5-pro",      Name = "Gemini 1.5 Pro",    Provider = "Google",        Tier = ModelTier.Diamond, Color = "#4285F4", Initial = "Gm",  DefaultSelected = true },
        new() { Id = "mistral-large",       Name = "Mistral Large",     Provider = "Mistral AI",    Tier = ModelTier.Diamond, Color = "#FF7000", Initial = "ML" },
        // ── Emerald ────────────────────────────────────────────────────────────
        new() { Id = "gpt-4o-mini",         Name = "GPT-4o Mini",       Provider = "Azure OpenAI",  Tier = ModelTier.Emerald, Color = "#10A37F", Initial = "Gm" },
        new() { Id = "phi-3-5-mini",        Name = "Phi-3.5 Mini",      Provider = "Microsoft",     Tier = ModelTier.Emerald, Color = "#0078D4", Initial = "Φ" },
        new() { Id = "phi-3-medium",        Name = "Phi-3 Medium 128k", Provider = "Microsoft",     Tier = ModelTier.Emerald, Color = "#0078D4", Initial = "Φ3" },
        new() { Id = "llama-3-1-70b",       Name = "Llama 3.1 70B",     Provider = "Meta",          Tier = ModelTier.Emerald, Color = "#0668E1", Initial = "L3" },
        new() { Id = "command-r-plus",      Name = "Command R+",        Provider = "Cohere",        Tier = ModelTier.Emerald, Color = "#D700D7", Initial = "Co" },
        // ── Gold ───────────────────────────────────────────────────────────────
        new() { Id = "grok-2",              Name = "Grok-2",            Provider = "xAI",           Tier = ModelTier.Gold,    Color = "#1DA1F2", Initial = "Gk" },
        new() { Id = "jais-30b",            Name = "Jais 30B",          Provider = "Core42",        Tier = ModelTier.Gold,    Color = "#8B5CF6", Initial = "Ja" },
        // ── Coming soon ────────────────────────────────────────────────────────
        new() { Id = "supreme-llama",       Name = "Supreme-Llama",     Provider = "SupremeAI",     Tier = ModelTier.Diamond, Color = "#50C878", Initial = "S",   ComingSoon = true },
    };

    public static readonly IReadOnlyList<AiModel> ImageModels = new List<AiModel>
    {
        new() { Id = "dalle-3",  Name = "DALL-E 3",             Provider = "Azure OpenAI",  Tier = ModelTier.Diamond, Color = "#10A37F", Initial = "D3" },
        new() { Id = "siu",      Name = "Stable Image Ultra",   Provider = "Stability AI",  Tier = ModelTier.Emerald, Color = "#FF4D00", Initial = "SI" },
        new() { Id = "sdxl",     Name = "Stable Diffusion XL",  Provider = "Stability AI",  Tier = ModelTier.Gold,    Color = "#FF4D00", Initial = "SD" },
    };

    public static readonly IReadOnlyList<AiModel> VideoModels = new List<AiModel>
    {
        new() { Id = "sora",      Name = "Sora",               Provider = "OpenAI",   Tier = ModelTier.Diamond, Color = "#10A37F", Initial = "So", ComingSoon = true },
        new() { Id = "runway-g3", Name = "Runway Gen-3 Alpha", Provider = "Runway",   Tier = ModelTier.Emerald, Color = "#00C9A7", Initial = "Rw" },
        new() { Id = "kling",     Name = "Kling 1.5",          Provider = "Kuaishou", Tier = ModelTier.Gold,    Color = "#FF6B35", Initial = "Kl" },
    };

    public static readonly string[] DemoResponses =
    [
        "Here's a comprehensive answer to your question. I've carefully analysed the request and identified the key concepts involved.\n\nThe primary considerations are:\n\n1. Context Understanding — Breaking down what you're asking into its core components.\n2. Knowledge Synthesis — Combining relevant information to form a coherent response.\n3. Structured Output — Presenting the answer in a clear, actionable format.\n\nIn summary, the answer depends on several factors, but the most important thing is to approach this systematically and validate each step before proceeding.",

        "Great question! Let me walk you through this step by step.\n\nFirst, it's important to understand the underlying principles at play here. The core mechanism involves a multi-layered approach where each component interacts with the others.\n\nFrom my analysis:\n• The primary factor accounts for roughly 60% of the outcome\n• Secondary considerations provide important nuance\n• Edge cases should always be handled explicitly\n\nI'd recommend starting with the fundamentals and building from there. Would you like me to go deeper on any specific aspect?",

        "Excellent prompt! Here's my take:\n\nThe question touches on an interesting intersection of technology and human experience. At its core, we're looking at how intelligent systems can augment human capability without replacing human judgement.\n\nKey insight: The best AI tools remain transparent about their reasoning and limitations.\n\nPractical next steps:\n1. Define your success criteria clearly\n2. Identify measurable outcomes\n3. Iterate based on real feedback\n\nLet me know if you'd like code examples or a deeper conceptual breakdown.",

        "I've processed your request. Here's a direct, concise answer:\n\nThe solution involves three main components working in concert. Implementing them correctly will give you the results you're looking for.\n\ncomponent_a  →  transforms input\ncomponent_b  →  validates output\ncomponent_c  →  handles edge cases\n\nThis approach is battle-tested and scales well. The main trade-off is initial setup complexity vs long-term maintainability — usually worth it for non-trivial systems.",
    ];
}
