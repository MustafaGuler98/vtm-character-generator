using Microsoft.AspNetCore.Mvc;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Models;
using VtmCharacterGenerator.Core.Services;

namespace VtmCharacterGenerator.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterController : ControllerBase
    {
        private readonly CharacterGeneratorService _characterGeneratorService;
        private readonly GameDataProvider _dataProvider;

        public CharacterController(CharacterGeneratorService characterGeneratorService, GameDataProvider dataProvider)
        {
            _characterGeneratorService = characterGeneratorService;
            _dataProvider = dataProvider;
        }

        [HttpGet("generate")]
        public ActionResult<Character> GenerateNewCharacter()
        {
            try
            {
                var inputPersona = new Persona(); 
                var finalCharacter = _characterGeneratorService.GenerateCharacter(inputPersona);
                return Ok(finalCharacter);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("create")]
        public ActionResult<Character> CreateCustomCharacter([FromBody] PersonaRequest request)
        {
            try
            {
                var demeanorId = string.IsNullOrEmpty(request.DemeanorId) ? null : request.DemeanorId;
                if (!string.IsNullOrEmpty(request.NatureId) && demeanorId == request.NatureId)
                {
                    demeanorId = null;
                }

                var inputPersona = new Persona
                {
                    Concept = string.IsNullOrEmpty(request.ConceptId) ? null : _dataProvider.Concepts.FirstOrDefault(c => c.Id == request.ConceptId),
                    Clan = string.IsNullOrEmpty(request.ClanId) ? null : _dataProvider.Clans.FirstOrDefault(c => c.Id == request.ClanId),
                    Nature = string.IsNullOrEmpty(request.NatureId) ? null : _dataProvider.Natures.FirstOrDefault(n => n.Id == request.NatureId),
                    Demeanor = string.IsNullOrEmpty(demeanorId) ? null : _dataProvider.Natures.FirstOrDefault(n => n.Id == demeanorId)
                };

                var finalCharacter = _characterGeneratorService.GenerateCharacter(inputPersona); 
                return Ok(finalCharacter); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("options")]
        public ActionResult<PersonaOptions> GetPersonaOptions()
        {
            try
            {
                return Ok(new PersonaOptions
                {
                    Concepts = _dataProvider.Concepts,
                    Clans = _dataProvider.Clans,
                    Natures = _dataProvider.Natures
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }

    public class PersonaRequest
    {
        public string ConceptId { get; set; }
        public string ClanId { get; set; }
        public string NatureId { get; set; }
        public string DemeanorId { get; set; }
    }

    public class PersonaOptions
    {
        public List<Concept> Concepts { get; set; }
        public List<Clan> Clans { get; set; }
        public List<Nature> Natures { get; set; }
    }
}