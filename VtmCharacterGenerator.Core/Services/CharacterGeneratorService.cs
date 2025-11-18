using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class CharacterGeneratorService
    {
        private readonly PersonaService _personaService;
        private readonly AttributeService _attributeService;
        private readonly AbilityDistributionService _abilityDistributionService;
        private readonly AffinityProcessorService _affinityProcessor;
        private readonly BackgroundDistributionService _backgroundDistributionService;
        private readonly VirtueDistributionService _virtueDistributionService;
        private readonly DisciplineDistributionService _disciplineDistributionService;
        private readonly CoreStatsService _coreStatsService;

        public CharacterGeneratorService(PersonaService personaService, 
               AttributeService attributeService, 
               AbilityDistributionService abilityDistributionService, 
               AffinityProcessorService affinityProcessor,
               BackgroundDistributionService backgroundDistributionService,
               VirtueDistributionService virtueDistributionService,
               DisciplineDistributionService disciplineDistributionService,
               CoreStatsService coreStatsService)

        {
            _personaService = personaService;
            _attributeService = attributeService;
            _abilityDistributionService = abilityDistributionService;
            _affinityProcessor = affinityProcessor;
            _backgroundDistributionService = backgroundDistributionService;
            _virtueDistributionService = virtueDistributionService;
            _disciplineDistributionService = disciplineDistributionService;
            _coreStatsService = coreStatsService;

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

            // I tried different approach for distributing attributes to see what works best
            character.Attributes = _attributeService.DistributeAttributes(affinityProfile);
            _abilityDistributionService.DistributeAbilities(character, affinityProfile);
            _backgroundDistributionService.DistributeBackgrounds(character, affinityProfile);
            _virtueDistributionService.DistributeVirtues(character, affinityProfile);
            _disciplineDistributionService.DistributeDisciplines(character, affinityProfile);
            _coreStatsService.CalculateCoreStats(character);

            // TODO: Generate other character parts

            return character;
        }
    }
}