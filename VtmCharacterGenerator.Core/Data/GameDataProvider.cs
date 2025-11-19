using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Data
{
    public class GameDataProvider
    {
        
        public List<Clan> Clans { get; }
        public List<Discipline> Disciplines { get; }
        public List<AttributeCategory> AttributeCategories { get; }
        public List<Concept> Concepts { get; }
        public List<Nature> Natures { get; }
        public List<Ability> Abilities { get; }
        public List<Background> Backgrounds { get; }
        public List<Virtue> Virtues { get; }
        public List<GenerationData> Generations { get; }
        public List<Merit> Merits { get; }
        public List<Flaw> Flaws { get; }


        public GameDataProvider(string dataFolderPath)
        {
            var clansJson = File.ReadAllText(Path.Combine(dataFolderPath, "clans.json"));
            Clans = JsonSerializer.Deserialize<List<Clan>>(clansJson);

            var disciplinesJson = File.ReadAllText(Path.Combine(dataFolderPath, "disciplines.json"));
            Disciplines = JsonSerializer.Deserialize<List<Discipline>>(disciplinesJson);

            var attributesJson = File.ReadAllText(Path.Combine(dataFolderPath, "attributes.json"));
            AttributeCategories = JsonSerializer.Deserialize<List<AttributeCategory>>(attributesJson);

            var conceptsJson = File.ReadAllText(Path.Combine(dataFolderPath, "concepts.json")); 
            Concepts = JsonSerializer.Deserialize<List<Concept>>(conceptsJson);

            var naturesJson = File.ReadAllText(Path.Combine(dataFolderPath, "natures.json"));
            Natures = JsonSerializer.Deserialize<List<Nature>>(naturesJson);

            var abilitiesJson = File.ReadAllText(Path.Combine(dataFolderPath, "abilities.json"));
            Abilities = JsonSerializer.Deserialize<List<Ability>>(abilitiesJson);

            var backgroundsJson = File.ReadAllText(Path.Combine(dataFolderPath, "backgrounds.json"));
            Backgrounds = JsonSerializer.Deserialize<List<Background>>(backgroundsJson);

            var virtuesJson = File.ReadAllText(Path.Combine(dataFolderPath, "virtues.json"));
            Virtues = JsonSerializer.Deserialize<List<Virtue>>(virtuesJson) ?? new List<Virtue>();

            var generationsJson = File.ReadAllText(Path.Combine(dataFolderPath, "generations.json"));
            Generations = JsonSerializer.Deserialize<List<GenerationData>>(generationsJson) ?? new List<GenerationData>();

            var meritsJson = File.ReadAllText(Path.Combine(dataFolderPath, "merits.json"));
            Merits = JsonSerializer.Deserialize<List<Merit>>(meritsJson) ?? new List<Merit>();

            var flawsJson = File.ReadAllText(Path.Combine(dataFolderPath, "flaws.json"));
            Flaws = JsonSerializer.Deserialize<List<Flaw>>(flawsJson) ?? new List<Flaw>();
        }
    }
}