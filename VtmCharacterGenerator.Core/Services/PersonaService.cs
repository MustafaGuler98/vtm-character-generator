using System.Linq;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class PersonaService
    {
        private readonly GameDataProvider _dataProvider;
        private readonly AffinityProcessorService _affinityProcessor;

        public PersonaService(GameDataProvider dataProvider, AffinityProcessorService affinityProcessor)
        {
            _dataProvider = dataProvider;
            _affinityProcessor = affinityProcessor;
        }

        // Null values in the input persona indicate that we need to generate them.
        public Persona CompletePersona(Persona inputPersona)
        {
            var finalPersona = new Persona();

            var currentAffinities = new System.Collections.Generic.Dictionary<string, int>();

            // Generator logic for each persona component
            finalPersona.Concept = inputPersona.Concept ?? _affinityProcessor.GetWeightedRandom(_dataProvider.Concepts, currentAffinities);
            currentAffinities = _affinityProcessor.BuildAffinityProfile(new Persona { Concept = finalPersona.Concept });

         
            finalPersona.Clan = inputPersona.Clan ?? _affinityProcessor.GetWeightedRandom(_dataProvider.Clans, currentAffinities);
            currentAffinities = _affinityProcessor.BuildAffinityProfile(new Persona { Concept = finalPersona.Concept, Clan = finalPersona.Clan });

            
            finalPersona.Nature = inputPersona.Nature ?? _affinityProcessor.GetWeightedRandom(_dataProvider.Natures, currentAffinities);
            currentAffinities = _affinityProcessor.BuildAffinityProfile(new Persona { Concept = finalPersona.Concept, Clan = finalPersona.Clan, Nature = finalPersona.Nature });

            
            var availableDemeanors = _dataProvider.Natures.Where(n => n.Id != finalPersona.Nature.Id).ToList();
            finalPersona.Demeanor = inputPersona.Demeanor ?? _affinityProcessor.GetWeightedRandom(availableDemeanors, currentAffinities);

            return finalPersona;
        }
    }
}