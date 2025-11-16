using System.Collections.Generic;
using System.Linq;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class DisciplineDistributionService
    {
        private readonly GameDataProvider _dataProvider;
        private readonly AffinityProcessorService _affinityProcessor;
        private const int PointsToDistribute = 3;
        private const int MaxValue = 5; // This is going to be configurable later.

		public DisciplineDistributionService(GameDataProvider dataProvider, AffinityProcessorService affinityProcessor)
        {
            _dataProvider = dataProvider;
            _affinityProcessor = affinityProcessor;
        }

        public void DistributeDisciplines(Character character, Dictionary<string, int> affinityProfile)
        {
            if (character.Clan?.Disciplines == null || !character.Clan.Disciplines.Any())
            {
                // TODO: Caitiff?
                return;
            }

            var clanDisciplineIds = character.Clan.Disciplines;
            var availableDisciplines = _dataProvider.Disciplines
                .Where(d => clanDisciplineIds.Contains(d.Id))
                .ToList();

            if (!availableDisciplines.Any())
            {
                return;
            }
            
            for (int i = 0; i < PointsToDistribute; i++)
            {
                var eligibleDisciplines = availableDisciplines.Where(d => 
                    !character.Disciplines.ContainsKey(d.Id) || character.Disciplines[d.Id] < MaxValue
                ).ToList();

                if (!eligibleDisciplines.Any())
                {
                    break;
                }

                var chosenDiscipline = _affinityProcessor.GetWeightedRandom(eligibleDisciplines, affinityProfile);
                if (chosenDiscipline == null) continue;

                if (!character.Disciplines.ContainsKey(chosenDiscipline.Id))
                {
                    character.Disciplines[chosenDiscipline.Id] = 0;
                }
                character.Disciplines[chosenDiscipline.Id]++;

                _affinityProcessor.ProcessAffinities(affinityProfile, chosenDiscipline.Affinities);
            }
        }
    }
}