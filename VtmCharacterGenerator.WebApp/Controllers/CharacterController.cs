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
        private readonly PdfServiceClient _pdfClient;

        public CharacterController(CharacterGeneratorService characterGeneratorService, GameDataProvider dataProvider, PdfServiceClient pdfClient)
        {
            _characterGeneratorService = characterGeneratorService;
            _dataProvider = dataProvider;
            _pdfClient = pdfClient;
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
                    Demeanor = string.IsNullOrEmpty(demeanorId) ? null : _dataProvider.Natures.FirstOrDefault(n => n.Id == demeanorId),
                    // Value types (Name, Age, Generation) are passed directly as raw input.
                    // A null value here explicitly represents "no user preference", which the service layer handles via randomization logic.
                    // Maybe, in the future we could change these or concept, clan, nature, demeanor for consistency.
                    Name = request.Name,
                    Generation = request.Generation,
                    Age = request.Age,
                    AgeCategory = request.AgeCategory
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
                    Natures = _dataProvider.Natures,
                    Generations = _dataProvider.Generations.Select(g => g.Generation).OrderByDescending(g => g).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpPost("download-pdf")]
        public async Task<IActionResult> DownloadPdf([FromBody] Character character)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine($"[PDF Request] Stream starting for: {character.Name}");

            try
            {
                var pdfStream = await _pdfClient.GeneratePdfStreamAsync(character);

                watch.Stop();
                Console.WriteLine($"[PDF Request] Stream established in {watch.ElapsedMilliseconds}ms. Piping to user...");

                string safeName = character.Name?.Replace(" ", "_") ?? "Character";
                string fileName = $"{safeName}_Sheet.pdf";

                return File(pdfStream, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                watch.Stop();
                Console.WriteLine($"[PDF Request] FAILED. Error: {ex.Message}");
                return StatusCode(503, $"PDF service unavailable: {ex.Message}");
            }
        }
    }

    public class PersonaRequest
    {
        public string? ConceptId { get; set; }
        public string? ClanId { get; set; }
        public string? NatureId { get; set; }
        public string? DemeanorId { get; set; }
        public string? Name { get; set; }
        public int? Generation { get; set; }
        public int? Age { get; set; }
        public string? AgeCategory { get; set; }
    }

    public class PersonaOptions
    {
        public List<Concept> Concepts { get; set; }
        public List<Clan> Clans { get; set; }
        public List<Nature> Natures { get; set; }
        public List<int> Generations { get; set; }
    }
}