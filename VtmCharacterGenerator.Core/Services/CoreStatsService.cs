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
            GenerationData chosenGenerationData = null;

            if (character.Generation.HasValue)
            {
                chosenGenerationData = _dataProvider.Generations
                    .FirstOrDefault(g => g.Generation == character.Generation.Value);
            }

            if (chosenGenerationData == null)
            {
                chosenGenerationData = SelectWeightedGeneration();
            }

            // We use defensive defaults (13th gen) just in case something went wrong with data loading
            if (chosenGenerationData != null)
            {
                character.Generation = chosenGenerationData.Generation;
                character.MaxTraitRating = chosenGenerationData.MaxTraitRating;
                character.MaximumBloodPool = chosenGenerationData.MaximumBloodPool;
                character.BloodPointsPerTurn = chosenGenerationData.BloodPointsPerTurn;
            }
            else
            {
                character.Generation = 13;
                character.MaxTraitRating = 5;
                character.MaximumBloodPool = 10;
                character.BloodPointsPerTurn = 1;
            }

            int conscience = character.Virtues.ContainsKey("conscience") ? character.Virtues["conscience"] : 1;
            int selfControl = character.Virtues.ContainsKey("self_control") ? character.Virtues["self_control"] : 1;
            int courage = character.Virtues.ContainsKey("courage") ? character.Virtues["courage"] : 1;

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