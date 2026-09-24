using System.Text.Json.Serialization;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Models
{
    public class Clan : IHasTags
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = string.Empty;

        [JsonPropertyName("disciplines")]
        public List<string> Disciplines { get; set; } = new();

        [JsonPropertyName("weakness")]
        public string Weakness { get; set; } = string.Empty;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("affinities")]
        public List<Affinity> Affinities { get; set; } = new();
    }
}
