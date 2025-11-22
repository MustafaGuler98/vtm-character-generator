using System.Collections.Generic;
using System.Linq;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Enums;
using VtmCharacterGenerator.Core.Models;
using VtmCharacterGenerator.Core.Services.Strategies;

namespace VtmCharacterGenerator.Core.Services.XpStrategies
{
    public class XpAbilityStrategy : BaseXpStrategy
    {
        private readonly GameDataProvider _dataProvider;
        private readonly ITraitCostStrategy _costStrategy;
        private readonly AffinityProcessorService _affinityProcessor;

        public override TraitType Type => TraitType.Ability;

        public override int MinRequiredXp => 2;

        public XpAbilityStrategy(
            GameDataProvider dataProvider,
            ITraitCostStrategy costStrategy,
            AffinityProcessorService affinityProcessor)
        {
            _dataProvider = dataProvider;
            _costStrategy = costStrategy;
            _affinityProcessor = affinityProcessor;
        }
        public override bool TrySpendXp(Character character, Dictionary<string, int> affinityProfile, int budget, ref int spentXp)
        {

            var validGroups = new Dictionary<string, List<Ability>>();

            foreach (var ability in _dataProvider.Abilities)
            {
                int currentRating = character.Abilities.ContainsKey(ability.Id) ? character.Abilities[ability.Id] : 0;
                if (currentRating >= character.MaxTraitRating) continue;
                int cost = _costStrategy.GetCost(TraitType.Ability, currentRating);

                if (cost <= budget)
                {
         
                    if (!validGroups.ContainsKey(ability.Category))
                    {
                        validGroups[ability.Category] = new List<Ability>();
                    }
                    validGroups[ability.Category].Add(ability);
                }
            }

            if (validGroups.Count == 0)
            {
                _isAvailable = false;
                return false;
            }

            var categoryCandidates = new List<TaggableItem>();
            foreach (var groupKey in validGroups.Keys)
            {
                categoryCandidates.Add(new TaggableItem
                {
                    Id = groupKey,
                    Tags = new List<string> { groupKey.ToLowerInvariant() }
                });
            }

            var selectedCategoryItem = _affinityProcessor.GetWeightedRandom(categoryCandidates, affinityProfile);
            if (selectedCategoryItem == null) return false;

            var abilitiesInSelectedCategory = validGroups[selectedCategoryItem.Id];
            var selectedAbility = _affinityProcessor.GetWeightedRandom(abilitiesInSelectedCategory, affinityProfile);

            if (selectedAbility == null) return false;

            string id = selectedAbility.Id;
            int rating = character.Abilities.ContainsKey(id) ? character.Abilities[id] : 0;
            int finalCost = _costStrategy.GetCost(TraitType.Ability, rating);

            if (!character.Abilities.ContainsKey(id))
            {
                character.Abilities[id] = 0;
            }
            character.Abilities[id]++;
            spentXp += finalCost;

            _affinityProcessor.ProcessAffinities(affinityProfile, selectedAbility.Affinities);

            character.DebugLog.Add($"[XP] Increased Ability {selectedAbility.Name} to {character.Abilities[id]} (Cost: {finalCost})");

            return true;
        }
    }
}