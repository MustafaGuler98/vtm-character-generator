using Microsoft.AspNetCore.Mvc;
using VtmCharacterGenerator.Core.Models;
using VtmCharacterGenerator.Core.Services;

namespace VtmCharacterGenerator.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterController : ControllerBase
    {
        private readonly PersonaService _personaService;

        public CharacterController(PersonaService personaService)
        {
            _personaService = personaService;
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
    }
}