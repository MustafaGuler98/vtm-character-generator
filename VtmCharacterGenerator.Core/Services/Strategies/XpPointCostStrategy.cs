using VtmCharacterGenerator.Core.Enums;
using VtmCharacterGenerator.Core.Services.Strategies;

namespace VtmCharacterGenerator.Core.Services.Strategies
{
    public class XpPointCostStrategy : ITraitCostStrategy
    {
        public int GetCost(TraitType type, int currentRating = 0, bool isClanTrait = false, bool isSecondaryPath = false)
        {
            switch (type)
            {
                case TraitType.Attribute:
                    return currentRating * 4;

                case TraitType.Ability:
                    return currentRating == 0 ? 3 : currentRating * 2;

                case TraitType.Discipline:
                    if (isSecondaryPath)
                    {
                        if (currentRating == 0) return 7;
                        return currentRating * 4;
                    }

                    if (currentRating == 0) return 10;
                    return currentRating * (isClanTrait ? 5 : 7);

                case TraitType.Virtue:
                    return currentRating * 2;

                case TraitType.Humanity:
                    return currentRating * 2;

                case TraitType.Willpower:
                    return currentRating;

                // Not purchasable with XP
                case TraitType.Background:
                case TraitType.Merit:
                case TraitType.Flaw:
                    return 9999;

                default:
                    return 0;
            }
        }
    }
}