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
        }
    }
}