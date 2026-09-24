using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VtmCharacterGenerator.Core.Models
{
    public class Ability : IHasTags
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("affinities")]
        public List<Affinity> Affinities { get; set; } = new();
    }
}
