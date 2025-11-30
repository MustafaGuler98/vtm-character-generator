using System.Collections.Generic;
using System.Linq;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Enums;
using VtmCharacterGenerator.Core.Models;
using VtmCharacterGenerator.Core.Services.Strategies;

namespace VtmCharacterGenerator.Core.Services.XpStrategies
{
    public class XpAttributeStrategy : BaseXpStrategy
    {
        private readonly GameDataProvider _dataProvider;
        private readonly ITraitCostStrategy _costStrategy;
        private readonly AffinityProcessorService _affinityProcessor;

        public override TraitType Type => TraitType.Attribute;

        public override int MinRequiredXp => 4;

        public XpAttributeStrategy(
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
  
            var validCategories = new List<TaggableItem>();

            foreach (var category in _dataProvider.AttributeCategories)
            {
                bool categoryHasValidOption = false;

                foreach (var attr in category.Attributes)
                {
                    // Nosferatu Check
                    if (character.Clan?.Id == "nosferatu" && attr.Id == "appearance") continue;

                    if (!character.Attributes.ContainsKey(attr.Id)) continue;

                    int currentRating = character.Attributes[attr.Id];

                    if (currentRating >= character.MaxTraitRating) continue;

                    int cost = _costStrategy.GetCost(TraitType.Attribute, currentRating);

                    if (cost <= budget)
                    {
                        categoryHasValidOption = true;
                        break;
                    }
                }
                if (categoryHasValidOption)
                {
                    validCategories.Add(new TaggableItem
                    {
                        Id = category.Id,
                        Tags = new List<string> { category.Name.ToLowerInvariant() }
                    });
                }
            }
            if (validCategories.Count == 0)
            {
                _isAvailable = false;
                return false;
            }
            var selectedCategoryItem = _affinityProcessor.GetWeightedRandom(validCategories, affinityProfile);
            if (selectedCategoryItem == null) return false;

            var selectedCategory = _dataProvider.AttributeCategories.FirstOrDefault(c => c.Id == selectedCategoryItem.Id);

            var validAttributes = new List<VtMAttribute>();

            foreach (var attr in selectedCategory.Attributes)
            {
                // Nosferatu Check
                if (character.Clan?.Id == "nosferatu" && attr.Id == "appearance") continue;

                int currentRating = character.Attributes[attr.Id];

                if (currentRating >= character.MaxTraitRating) continue;

                int cost = _costStrategy.GetCost(TraitType.Attribute, currentRating);
                if (cost <= budget)
                {
                    validAttributes.Add(attr);
                }
            }
            if (validAttributes.Count == 0) return false;

            var finalAttribute = _affinityProcessor.GetWeightedRandom(validAttributes, affinityProfile);
            if (finalAttribute == null) return false;


            string id = finalAttribute.Id;
            int rating = character.Attributes[id];
            int finalCost = _costStrategy.GetCost(TraitType.Attribute, rating);

            character.Attributes[id]++;     
            spentXp += finalCost;
            _affinityProcessor.ProcessAffinities(affinityProfile, finalAttribute.Affinities);

            character.DebugLog.Add($"[XP] Attribute Upgraded: {finalAttribute.Name} ({rating} -> {rating + 1}) Cost: {finalCost}");

            return true;
        }
    }
}