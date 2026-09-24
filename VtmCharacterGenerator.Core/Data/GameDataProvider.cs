using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Data
{
    public class GameDataProvider
    {
        
        public List<Clan> Clans { get; } = new();
        public List<Discipline> Disciplines { get; } = new();
        public List<AttributeCategory> AttributeCategories { get; } = new();
        public List<Concept> Concepts { get; } = new();
        public List<Nature> Natures { get; } = new();
        public List<Ability> Abilities { get; } = new();
        public List<Background> Backgrounds { get; } = new();
        public List<Virtue> Virtues { get; } = new();
        public List<GenerationData> Generations { get; } = new();
        public List<Merit> Merits { get; } = new();
        public List<Flaw> Flaws { get; } = new();
        public List<NamePack> NamePacks { get; } = new();


        public GameDataProvider(string dataFolderPath)
        {
            var clansJson = File.ReadAllText(Path.Combine(dataFolderPath, "clans.json"));
            Clans = JsonSerializer.Deserialize<List<Clan>>(clansJson) ?? new();

            var disciplinesJson = File.ReadAllText(Path.Combine(dataFolderPath, "disciplines.json"));
            Disciplines = JsonSerializer.Deserialize<List<Discipline>>(disciplinesJson) ?? new();

            var attributesJson = File.ReadAllText(Path.Combine(dataFolderPath, "attributes.json"));
            AttributeCategories = JsonSerializer.Deserialize<List<AttributeCategory>>(attributesJson) ?? new();

            var conceptsJson = File.ReadAllText(Path.Combine(dataFolderPath, "concepts.json")); 
            var rawConcepts = JsonSerializer.Deserialize<List<Concept>>(conceptsJson) ?? new();
            Concepts = rawConcepts.OrderBy(n => n.Name).ToList();

            var naturesJson = File.ReadAllText(Path.Combine(dataFolderPath, "natures.json"));
            var rawNatures = JsonSerializer.Deserialize<List<Nature>>(naturesJson) ?? new();
            Natures = rawNatures.OrderBy(n => n.Name).ToList();

            var abilitiesJson = File.ReadAllText(Path.Combine(dataFolderPath, "abilities.json"));
            Abilities = JsonSerializer.Deserialize<List<Ability>>(abilitiesJson) ?? new();

            var backgroundsJson = File.ReadAllText(Path.Combine(dataFolderPath, "backgrounds.json"));
            Backgrounds = JsonSerializer.Deserialize<List<Background>>(backgroundsJson) ?? new();

            var virtuesJson = File.ReadAllText(Path.Combine(dataFolderPath, "virtues.json"));
            Virtues = JsonSerializer.Deserialize<List<Virtue>>(virtuesJson) ?? new();

            var generationsJson = File.ReadAllText(Path.Combine(dataFolderPath, "generations.json"));
            Generations = JsonSerializer.Deserialize<List<GenerationData>>(generationsJson) ?? new();

            var meritsJson = File.ReadAllText(Path.Combine(dataFolderPath, "merits.json"));
            Merits = JsonSerializer.Deserialize<List<Merit>>(meritsJson) ?? new();

            var flawsJson = File.ReadAllText(Path.Combine(dataFolderPath, "flaws.json"));
            Flaws = JsonSerializer.Deserialize<List<Flaw>>(flawsJson) ?? new();

            LoadNamePacks(dataFolderPath);
        }
        private void LoadNamePacks(string rootDataPath)
        {
            var namesFolderPath = Path.Combine(rootDataPath, "Names");

            if (!Directory.Exists(namesFolderPath))
            {
                return;
            }

            var files = Directory.GetFiles(namesFolderPath, "*.json", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                try
                {
                    var jsonContent = File.ReadAllText(file);
                    var pack = JsonSerializer.Deserialize<NamePack>(jsonContent);
                    if (pack != null)
                    {
                        NamePacks.Add(pack);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

    }
}
