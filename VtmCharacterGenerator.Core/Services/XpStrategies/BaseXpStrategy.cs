using System.Collections.Generic;
using VtmCharacterGenerator.Core.Enums;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services.XpStrategies
{
    public abstract class BaseXpStrategy : IXpStrategy
    {
        protected bool _isAvailable = true;

        public abstract TraitType Type { get; }
        public abstract int MinRequiredXp { get; }

        public bool IsAvailable => _isAvailable;

        public void ResetAvailability()
        {
            _isAvailable = true;
        }

        public abstract bool TrySpendXp(Character character, Dictionary<string, int> affinityProfile, int budget, ref int spentXp);
    }
}