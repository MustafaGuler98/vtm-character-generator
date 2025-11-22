using System.Collections.Generic;
using VtmCharacterGenerator.Core.Enums;
using VtmCharacterGenerator.Core.Models;
using VtmCharacterGenerator.Core.Services.Strategies;

namespace VtmCharacterGenerator.Core.Services.XpStrategies
{
    public class XpHumanityStrategy : BaseXpStrategy
    {
        private readonly ITraitCostStrategy _costStrategy;

        public override TraitType Type => TraitType.Humanity;
        public override int MinRequiredXp => 2;

        public XpHumanityStrategy(ITraitCostStrategy costStrategy)
        {
            _costStrategy = costStrategy;
        }

        public override bool TrySpendXp(Character character, Dictionary<string, int> affinityProfile, int budget, ref int spentXp)
        {
            if (character.Humanity >= 10)
            {
                _isAvailable = false;
                return false;
            }
            int current = character.Humanity;
            int cost = _costStrategy.GetCost(TraitType.Humanity, current);

            if (cost > budget)
            {
                _isAvailable = false;
                return false;
            }

            // Note: We do NOT check Age here. XP spending represents the character's effort
            // to maintain their humanity. The "Decay" logic in LifeCycleService will handle
            // the erosion of that effort over time.

            character.Humanity++;
            spentXp += cost;

            character.DebugLog.Add($"[XP] Increased Humanity to {character.Humanity} (Cost: {cost})");

            return true;
        }
    }
}