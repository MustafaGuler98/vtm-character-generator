using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class CharacterGeneratorService
    {
        private readonly PersonaService _personaService;
        private readonly AttributeService _attributeService;
        private readonly AffinityProcessorService _affinityProcessor;

        public CharacterGeneratorService(PersonaService personaService, AttributeService attributeService, AffinityProcessorService affinityProcessor)
        {
            _personaService = personaService;
            _attributeService = attributeService;
            _affinityProcessor = affinityProcessor;
        }

        public Character GenerateCharacter(Persona inputPersona)
        {
            
            var finalPersona = _personaService.CompletePersona(inputPersona);
            var affinityProfile = _affinityProcessor.BuildAffinityProfile(finalPersona);

            var character = new Character
            {
                Concept = finalPersona.Concept,
                Clan = finalPersona.Clan,
                Nature = finalPersona.Nature,
                Demeanor = finalPersona.Demeanor
            };

            character.Attributes = _attributeService.DistributeAttributes(affinityProfile);

            // TODO: Generate other character parts like abilities, disciplines, etc.

            return character;
        }
    }
}