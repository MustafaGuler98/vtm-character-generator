using System.Collections.Generic;
using System.Linq;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class VirtueDistributionService
    {
        private readonly GameDataProvider _dataProvider;
        private readonly AffinityProcessorService _affinityProcessor;
        private const int PointsToDistribute = 7;
        private const int StartingValue = 1;
        private const int MaxValue = 5;

        public VirtueDistributionService(GameDataProvider dataProvider, AffinityProcessorService affinityProcessor)
        {
            _dataProvider = dataProvider;
            _affinityProcessor = affinityProcessor;
        }

        public void DistributeVirtues(Character character, Dictionary<string, int> affinityProfile)
        {
            if (_dataProvider.Virtues == null || !_dataProvider.Virtues.Any())
            {
                return;
            }

            var availableVirtues = new List<Virtue>(_dataProvider.Virtues);

            foreach (var virtue in availableVirtues)
            {
                character.Virtues[virtue.Id] = StartingValue;
            }

    
            for (int i = 0; i < PointsToDistribute; i++)
            {
                var eligibleVirtues = availableVirtues
                    .Where(v => character.Virtues[v.Id] < MaxValue)
                    .ToList();

                if (!eligibleVirtues.Any())
                {
                    break;
                }

                var chosenVirtue = _affinityProcessor.GetWeightedRandom(eligibleVirtues, affinityProfile);

                if (chosenVirtue == null) continue;

                character.Virtues[chosenVirtue.Id]++;

                _affinityProcessor.ProcessAffinities(affinityProfile, chosenVirtue.Affinities);
            }
        }
    }
}