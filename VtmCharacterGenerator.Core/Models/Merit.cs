using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VtmCharacterGenerator.Core.Models
{
    public class Merit : IHasTags
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("cost")]
        public int Cost { get; set; }
        [JsonPropertyName("rarity")]
        public int Rarity { get; set; } = 3;

        [JsonPropertyName("conflictingTraits")]
        public List<string> ConflictingTraits { get; set; } = new();

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("affinities")]
        public List<Affinity> Affinities { get; set; } = new();
    }
}
