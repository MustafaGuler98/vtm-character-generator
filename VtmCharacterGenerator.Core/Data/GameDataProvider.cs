using System.Text.Json;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Data
{
    public class GameDataProvider
    {
        
        public List<Clan> Clans { get; }
        public List<Discipline> Disciplines { get; }

       
        public GameDataProvider(string dataFolderPath)
        {
            var clansJson = File.ReadAllText(Path.Combine(dataFolderPath, "clans.json"));
            Clans = JsonSerializer.Deserialize<List<Clan>>(clansJson);

            var disciplinesJson = File.ReadAllText(Path.Combine(dataFolderPath, "disciplines.json"));
            Disciplines = JsonSerializer.Deserialize<List<Discipline>>(disciplinesJson);
        }
    }
}