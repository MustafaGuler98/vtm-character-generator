using System.Collections.Generic;
using VtmCharacterGenerator.Core.Enums;
using VtmCharacterGenerator.Core.Models;
using VtmCharacterGenerator.Core.Services.Strategies;

namespace VtmCharacterGenerator.Core.Services.XpStrategies
{
    public class XpWillpowerStrategy : BaseXpStrategy
    {
        private readonly ITraitCostStrategy _costStrategy;
        public override TraitType Type => TraitType.Willpower;
        public override int MinRequiredXp => 1;
        public XpWillpowerStrategy(ITraitCostStrategy costStrategy)
        {
            _costStrategy = costStrategy;
        }

        public override bool TrySpendXp(Character character, Dictionary<string, int> affinityProfile, int budget, ref int spentXp)
        {
            if (character.Willpower >= 10)
            {
                _isAvailable = false;
                return false;
            }
            int currentRating = character.Willpower;
            int cost = _costStrategy.GetCost(TraitType.Willpower, currentRating);

            if (cost > budget)
            {
                _isAvailable = false;
                return false;
            }

            character.Willpower++;
            spentXp += cost;

            character.DebugLog.Add($"[XP] Increased Willpower to {character.Willpower} (Cost: {cost})");

            return true;
        }
    }
}