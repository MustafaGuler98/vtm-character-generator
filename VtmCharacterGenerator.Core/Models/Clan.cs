using System.Text.Json.Serialization;

namespace VtmCharacterGenerator.Core.Models
{
    public class Clan
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("disciplines")]
        public List<string> Disciplines { get; set; }

        [JsonPropertyName("weakness")]
        public string Weakness { get; set; }
    }
}