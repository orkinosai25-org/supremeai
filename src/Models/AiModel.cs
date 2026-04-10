namespace SupremeAI.Models;

public enum ModelTier { Diamond, Emerald, Gold, Silver }

public class AiModel
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Provider { get; init; } = "";
    public ModelTier Tier { get; init; }
    public string Color { get; init; } = "#888";
    public string Initial { get; init; } = "?";
    public bool DefaultSelected { get; init; }
    public bool ComingSoon { get; init; }

    public string TierLabel => Tier.ToString();
    public string TierCssClass => $"sai-gem-{Tier.ToString().ToLowerInvariant()}";
}

public class ModelResponse
{
    public string ModelId { get; set; } = "";
    public string Text { get; set; } = "";
    public string Status { get; set; } = "loading"; // loading | done | error
    public int Tokens { get; set; }
    public int Ms { get; set; }
}

public class Conversation
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];
    public string Prompt { get; set; } = "";
    public List<ModelResponse> Responses { get; init; } = [];
}
