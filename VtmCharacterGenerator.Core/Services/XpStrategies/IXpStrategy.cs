using System.Collections.Generic;
using VtmCharacterGenerator.Core.Enums;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services.XpStrategies
{
    public interface IXpStrategy
    {
        TraitType Type { get; }

        // The absolute minimum XP needed to even consider this category.
        int MinRequiredXp { get; }

        // Set to false internally if the strategy determines nothing else can be bought.
        bool IsAvailable { get; }

        void ResetAvailability();
        bool TrySpendXp(Character character, Dictionary<string, int> affinityProfile, int budget, ref int spentXp);
    }
}