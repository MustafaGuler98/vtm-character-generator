using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class CharacterGeneratorService
    {
        private readonly PersonaService _personaService;
        private readonly AttributeService _attributeService;
        private readonly AbilityDistributionService _abilityDistributionService;
        private readonly AffinityProcessorService _affinityProcessor;

        public CharacterGeneratorService(PersonaService personaService, AttributeService attributeService, AbilityDistributionService abilityDistributionService, AffinityProcessorService affinityProcessor)
        {
            _personaService = personaService;
            _attributeService = attributeService;
            _abilityDistributionService = abilityDistributionService;
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

            // I tried differen approaches for distributing attributes and abilities.
            character.Attributes = _attributeService.DistributeAttributes(affinityProfile);
            _abilityDistributionService.DistributeAbilities(character, affinityProfile);

            // TODO: Generate other character parts like abilities, disciplines, etc.

            return character;
        }
    }
}