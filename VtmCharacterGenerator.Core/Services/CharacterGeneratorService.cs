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
        private readonly FreebieSpendingService _freebieSpendingService;
        private readonly LifeCycleService _lifeCycleService;
        private readonly XpSpendingService _xpSpendingService;

        public CharacterGeneratorService(PersonaService personaService, 
               AttributeService attributeService, 
               AbilityDistributionService abilityDistributionService, 
               AffinityProcessorService affinityProcessor,
               BackgroundDistributionService backgroundDistributionService,
               VirtueDistributionService virtueDistributionService,
               DisciplineDistributionService disciplineDistributionService,
               CoreStatsService coreStatsService,
               FreebieSpendingService freebieSpendingService,
               LifeCycleService lifeCycleService,
               XpSpendingService xpSpendingService)

        {
            _personaService = personaService;
            _attributeService = attributeService;
            _abilityDistributionService = abilityDistributionService;
            _affinityProcessor = affinityProcessor;
            _backgroundDistributionService = backgroundDistributionService;
            _virtueDistributionService = virtueDistributionService;
            _disciplineDistributionService = disciplineDistributionService;
            _coreStatsService = coreStatsService;
            _freebieSpendingService = freebieSpendingService;
            _lifeCycleService = lifeCycleService;
            _xpSpendingService = xpSpendingService;

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
            _freebieSpendingService.DistributeFreebiePoints(character, affinityProfile);
            _coreStatsService.CalculateCoreStats(character);
            _lifeCycleService.DetermineLifeCycle(character);
            _xpSpendingService.DistributeXp(character, affinityProfile);
            _lifeCycleService.ApplyHumanityDegeneration(character);


            return character;
        }
    }
}