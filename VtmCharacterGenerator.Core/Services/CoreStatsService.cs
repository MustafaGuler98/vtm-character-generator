using System.Collections.Generic;
using System.Linq;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class CoreStatsService
    {
        private readonly GameDataProvider _dataProvider;
        private readonly Random _random = new Random();

        public CoreStatsService(GameDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public void CalculateCoreStats(Character character)
        {
            var chosenGenerationData = SelectWeightedGeneration();
            if (chosenGenerationData == null)
            {
                // Defensive fallback values
                character.Generation = 13;
                character.MaxTraitRating = 5;
                character.MaximumBloodPool = 10;
                character.BloodPointsPerTurn = 1;
            }
            else
            {
                character.Generation = chosenGenerationData.Generation;
                character.MaxTraitRating = chosenGenerationData.MaxTraitRating;
                character.MaximumBloodPool = chosenGenerationData.MaximumBloodPool;
                character.BloodPointsPerTurn = chosenGenerationData.BloodPointsPerTurn;
            }

            int conscience = character.Virtues.ContainsKey("conscience") ? character.Virtues["conscience"] : 0;
            int selfControl = character.Virtues.ContainsKey("self_control") ? character.Virtues["self_control"] : 0;
            int courage = character.Virtues.ContainsKey("courage") ? character.Virtues["courage"] : 0;

            character.Humanity = conscience + selfControl;
            character.Willpower = courage;
        }

        private GenerationData SelectWeightedGeneration()
        {
            if (_dataProvider.Generations == null || !_dataProvider.Generations.Any())
            {
                return null;
            }

       
            int totalWeight = _dataProvider.Generations.Sum(g => g.Weight);

            int randomNumber = _random.Next(1, totalWeight + 1);

            foreach (var genData in _dataProvider.Generations.OrderBy(g => g.Generation))
            {
                if (randomNumber <= genData.Weight)
                {
                    return genData;
                }
                randomNumber -= genData.Weight;
            }

            return _dataProvider.Generations.LastOrDefault();
        }
    }
}