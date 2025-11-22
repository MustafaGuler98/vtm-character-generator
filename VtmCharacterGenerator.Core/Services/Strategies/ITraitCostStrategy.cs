using VtmCharacterGenerator.Core.Enums;

namespace VtmCharacterGenerator.Core.Services.Strategies
{
    public interface ITraitCostStrategy
    {
        int GetCost(TraitType type, int currentRating = 0, bool isClanTrait = false, bool isSecondaryPath = false);
    }
}