using Microsoft.AspNetCore.Mvc;
using VtmCharacterGenerator.Core.Services;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // This will result in a URL like /api/character
    public class CharacterController : ControllerBase
    {
        private readonly CharacterGeneratorService _generatorService;

        public CharacterController(CharacterGeneratorService generatorService)
        {
            _generatorService = generatorService;
        }

        [HttpGet("generate")] // This maps to a GET request at /api/character/generate
        public ActionResult<Character> GenerateNewCharacter()
        {
            try
            {
                var newCharacter = _generatorService.GenerateCharacter();
                return Ok(newCharacter); 
            }
            catch (Exception ex)
            {
              
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}