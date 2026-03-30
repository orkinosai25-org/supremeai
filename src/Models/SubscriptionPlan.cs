namespace SupremeAI.Models;

public enum PlanTier { Free, Starter, Pro, Creative, Enterprise }

public class SubscriptionPlan
{
    public PlanTier Tier { get; init; }
    public string Name { get; init; } = "";
    public string Price { get; init; } = "";
    public string PriceSuffix { get; init; } = "";
    public int MaxModels { get; init; }          // -1 = unlimited
    public int MessagesPerDay { get; init; }     // -1 = unlimited
    public bool Multimodal { get; init; }
    public bool VideoGeneration { get; init; }
    public bool AudioGeneration { get; init; }
    public bool Uploads { get; init; }
    public int MaxUploadMb { get; init; }        // 0 = none, -1 = unlimited
    public bool HdImageGeneration { get; init; }
    public bool MultiAgentWorkflows { get; init; }
    public bool CommercialUse { get; init; }
    public bool TeamsAndAdmin { get; init; }
    public ModelTier[] AllowedTiers { get; init; } = [];
    public string[] Features { get; init; } = [];
    public string Color { get; init; } = "#888";
    public string GemClass { get; init; } = "";
    public bool IsPopular { get; init; }

    public string MaxModelsText => MaxModels == -1 ? "Unlimited" : MaxModels.ToString();
    public string MessagesText => MessagesPerDay == -1 ? "Unlimited" : $"{MessagesPerDay}/day";
}

public class PlanAddon
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Price { get; init; } = "";
    public string Description { get; init; } = "";
}

public static class SubscriptionPlans
{
    public static readonly IReadOnlyList<SubscriptionPlan> Plans = new List<SubscriptionPlan>
    {
        new()
        {
            Tier = PlanTier.Free,
            Name = "Free",
            Price = "£0",
            PriceSuffix = "forever",
            MaxModels = 2,
            MessagesPerDay = 20,
            Multimodal = false,
            VideoGeneration = false,
            AudioGeneration = false,
            Uploads = false,
            MaxUploadMb = 0,
            HdImageGeneration = false,
            MultiAgentWorkflows = false,
            CommercialUse = false,
            TeamsAndAdmin = false,
            AllowedTiers = [ModelTier.Emerald, ModelTier.Gold, ModelTier.Silver],
            Features =
            [
                "1–2 models at a time",
                "Basic models only (GPT-4o Mini, Phi-3.5, Llama 3.1, Grok-2)",
                "20 messages per day",
                "No multimodal",
                "No file uploads",
            ],
            Color = "#C0C0C0",
            GemClass = "sai-gem-silver",
        },
        new()
        {
            Tier = PlanTier.Starter,
            Name = "Starter",
            Price = "£5",
            PriceSuffix = "/month",
            MaxModels = 1,
            MessagesPerDay = 50,
            Multimodal = false,
            VideoGeneration = false,
            AudioGeneration = false,
            Uploads = false,
            MaxUploadMb = 0,
            HdImageGeneration = false,
            MultiAgentWorkflows = false,
            CommercialUse = false,
            TeamsAndAdmin = false,
            AllowedTiers = [ModelTier.Diamond, ModelTier.Emerald, ModelTier.Gold, ModelTier.Silver],
            Features =
            [
                "1 model at a time",
                "Access to all models (GPT-4o, Claude, Gemini, Grok)",
                "50 messages per day",
                "No multimodal",
                "No file uploads",
            ],
            Color = "#7FFFD4",
            GemClass = "sai-gem-aquamarine",
        },
        new()
        {
            Tier = PlanTier.Pro,
            Name = "Pro",
            Price = "£19",
            PriceSuffix = "/month",
            MaxModels = 5,
            MessagesPerDay = 300,
            Multimodal = true,
            VideoGeneration = false,
            AudioGeneration = false,
            Uploads = true,
            MaxUploadMb = 20,
            HdImageGeneration = false,
            MultiAgentWorkflows = false,
            CommercialUse = false,
            TeamsAndAdmin = false,
            AllowedTiers = [ModelTier.Diamond, ModelTier.Emerald, ModelTier.Gold, ModelTier.Silver],
            Features =
            [
                "Up to 5 models at once",
                "Full high-end models (GPT-4o, Gemini Pro, Claude Sonnet, Grok 1.5)",
                "300 messages per day",
                "Multimodal: images, OCR, PDFs",
            ],
            Color = "#50C878",
            GemClass = "sai-gem-emerald",
            IsPopular = true,
        },
        new()
        {
            Tier = PlanTier.Creative,
            Name = "Creative",
            Price = "£39",
            PriceSuffix = "/month",
            MaxModels = -1,
            MessagesPerDay = -1,
            Multimodal = true,
            VideoGeneration = true,
            AudioGeneration = true,
            Uploads = true,
            MaxUploadMb = 150,
            HdImageGeneration = true,
            MultiAgentWorkflows = true,
            CommercialUse = true,
            TeamsAndAdmin = false,
            AllowedTiers = [ModelTier.Diamond, ModelTier.Emerald, ModelTier.Gold, ModelTier.Silver],
            Features =
            [
                "Unlimited model switching",
                "Full multimodal + video + audio",
                "HD image generation",
                "Video generation (Runway / Pika / Sora)",
                "150 MB uploads",
                "Multi-agent workflows",
                "Commercial use licence",
            ],
            Color = "#FFD700",
            GemClass = "sai-gem-gold",
        },
        new()
        {
            Tier = PlanTier.Enterprise,
            Name = "Enterprise",
            Price = "£99",
            PriceSuffix = "/seat/month",
            MaxModels = -1,
            MessagesPerDay = -1,
            Multimodal = true,
            VideoGeneration = true,
            AudioGeneration = true,
            Uploads = true,
            MaxUploadMb = -1,
            HdImageGeneration = true,
            MultiAgentWorkflows = true,
            CommercialUse = true,
            TeamsAndAdmin = true,
            AllowedTiers = [ModelTier.Diamond, ModelTier.Emerald, ModelTier.Gold, ModelTier.Silver],
            Features =
            [
                "Everything in Creative",
                "Teams & admin dashboard",
                "SLA guarantees",
                "Dedicated instances",
                "Priority support",
                "Custom integrations",
            ],
            Color = "#A8D8EA",
            GemClass = "sai-gem-diamond",
        },
    };

    public static readonly IReadOnlyList<PlanAddon> Addons = new List<PlanAddon>
    {
        new() { Id = "extra-msgs",  Name = "+1,000 Messages", Price = "+£6/mo",  Description = "Extend your monthly message allowance" },
        new() { Id = "video-mins",  Name = "Video Minutes",   Price = "+£9/mo",  Description = "Additional video generation minutes" },
        new() { Id = "voice-clone", Name = "Voice Cloning",   Price = "+£12/mo", Description = "Clone and use custom voices" },
    };
}
