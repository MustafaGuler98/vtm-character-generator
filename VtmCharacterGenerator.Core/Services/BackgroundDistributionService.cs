using System;
using System.Collections.Generic;
using System.Linq;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class BackgroundDistributionService
    {
        private readonly GameDataProvider _dataProvider;
        private readonly AffinityProcessorService _affinityProcessor;
        private const int PointsToDistribute = 5;

        public BackgroundDistributionService(GameDataProvider dataProvider, AffinityProcessorService affinityProcessor)
        {
            _dataProvider = dataProvider;
            _affinityProcessor = affinityProcessor;
        }
        public void DistributeBackgrounds(Character character, Dictionary<string, int> affinityProfile)
        {
            
            if (_dataProvider.Backgrounds == null || !_dataProvider.Backgrounds.Any())
            {
                return;
            }

            var availableBackgrounds = new List<Background>(_dataProvider.Backgrounds);

            for (int i = 0; i < PointsToDistribute; i++)
            {
               
                var chosenBackground = _affinityProcessor.GetWeightedRandom(availableBackgrounds, affinityProfile);

                // Safety check
                if (chosenBackground == null) continue;

                if (!character.Backgrounds.ContainsKey(chosenBackground.Id))
                {
                    character.Backgrounds[chosenBackground.Id] = 0;
                }

                if (character.Backgrounds[chosenBackground.Id] < 5)
                {
                    character.Backgrounds[chosenBackground.Id]++;

                    _affinityProcessor.ProcessAffinities(affinityProfile, chosenBackground.Affinities);
                }
                else
                {
                    // If the chosen background is already maxed out, we must try again
                    // without consuming a point. So we decrement the loop counter.
                    i--;
                }
            }
        }
    }
}