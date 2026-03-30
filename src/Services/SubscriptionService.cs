using SupremeAI.Models;

namespace SupremeAI.Services;

public class SubscriptionService
{
    private SubscriptionPlan _currentPlan = SubscriptionPlans.Plans[0]; // Free by default

    public SubscriptionPlan CurrentPlan => _currentPlan;

    public event Action? OnPlanChanged;

    public void SelectPlan(PlanTier tier)
    {
        var plan = SubscriptionPlans.Plans.FirstOrDefault(p => p.Tier == tier);
        if (plan is not null && plan != _currentPlan)
        {
            _currentPlan = plan;
            OnPlanChanged?.Invoke();
        }
    }

    public bool CanSelectModel(string modelId, HashSet<string> currentlySelected)
    {
        var model = ModelCatalogue.ChatModels.FirstOrDefault(m => m.Id == modelId);
        if (model is null) return false;
        if (!_currentPlan.AllowedTiers.Contains(model.Tier)) return false;
        if (_currentPlan.MaxModels > 0 && currentlySelected.Count >= _currentPlan.MaxModels) return false;
        return true;
    }

    public bool IsModelAllowed(string modelId)
    {
        var model = ModelCatalogue.ChatModels.FirstOrDefault(m => m.Id == modelId);
        return model is not null && _currentPlan.AllowedTiers.Contains(model.Tier);
    }
}
