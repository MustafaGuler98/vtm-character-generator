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
        private readonly PersonaService _personaService;
        private readonly GameDataProvider _dataProvider;

        public CharacterController(PersonaService personaService, GameDataProvider dataProvider)
        {
            _personaService = personaService;
            _dataProvider = dataProvider;
        }

        [HttpGet("generate")]
        public ActionResult<Persona> GenerateNewPersona()
        {
            try
            {
                var inputPersona = new Persona();
                var finalPersona = _personaService.CompletePersona(inputPersona);
                return Ok(finalPersona);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("create")]
        public ActionResult<Persona> CreateCustomPersona([FromBody] PersonaRequest request)
        {
            try
            {
                // Defensive: if client submitted same id for Nature and Demeanor, ignore Demeanor.
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

                var finalPersona = _personaService.CompletePersona(inputPersona);
                return Ok(finalPersona);
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