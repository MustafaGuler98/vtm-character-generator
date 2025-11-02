using System;
using System.Collections.Generic;
using System.Linq;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class AbilityDistributionService
    {
        private readonly GameDataProvider _dataProvider;
        private readonly AffinityProcessorService _affinityProcessor;
        private readonly Random _random = new Random();

        public AbilityDistributionService(GameDataProvider dataProvider, AffinityProcessorService affinityProcessor)
        {
            _dataProvider = dataProvider;
            _affinityProcessor = affinityProcessor;
        }

        public void DistributeAbilities(Character character, Dictionary<string, int> affinityProfile)
        {
            var abilitiesByCategory = _dataProvider.Abilities
                .GroupBy(a => a.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            var categories = new List<string> { "Talents", "Skills", "Knowledges" };
            var points = new List<int> { 13, 9, 5 };

            points = points.OrderBy(p => _random.Next()).ToList();

            var categoryScores = new Dictionary<string, int>();

            foreach (var pointValue in points)
            {
                var weightedCategories = categories
                    .Where(c => !categoryScores.ContainsKey(c))
                    .Select(c => new TaggableItem { Id = c, Name = c, Tags = new List<string> { c.ToLower() } })
                    .ToList();

                var chosenCategory = _affinityProcessor.GetWeightedRandom(weightedCategories, affinityProfile);
                categoryScores[chosenCategory.Id] = pointValue;
            }

            foreach (var category in categoryScores)
            {
                var categoryName = category.Key;
                var pointsToDistribute = category.Value;
                var availableAbilities = abilitiesByCategory[categoryName];

                for (int i = 0; i < pointsToDistribute; i++)
                {
                   
                    var chosenAbility = _affinityProcessor.GetWeightedRandom(availableAbilities, affinityProfile);

                    if (chosenAbility == null) continue;

                    if (!character.Abilities.ContainsKey(chosenAbility.Id))
                    {
                        character.Abilities[chosenAbility.Id] = 0;
                    }

                    // Enforce the "max 3 points" rule for this phase
                    if (character.Abilities[chosenAbility.Id] < 3)
                    {
                        character.Abilities[chosenAbility.Id]++;
                        _affinityProcessor.ProcessAffinities(affinityProfile, chosenAbility.Affinities);
                    }
                    else
                    {
                        // If the chosen ability is already maxed out, we try again.
                        i--;
                    }
                }
            }
        }
    }

    // Helper class to make categories compatible with GetWeightedRandom<T>
    internal class TaggableItem : IHasTags
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Tags { get; set; }
    }
}