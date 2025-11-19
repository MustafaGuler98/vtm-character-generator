using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VtmCharacterGenerator.Core.Models
{
    public class Flaw : IHasTags
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("cost")]
        public int Cost { get; set; }

        [JsonPropertyName("rarity")]
        public int Rarity { get; set; } = 3;

        [JsonPropertyName("conflictingTraits")]
        public List<string> ConflictingTraits { get; set; } = new List<string>();

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [JsonPropertyName("affinities")]
        public List<Affinity> Affinities { get; set; } = new List<Affinity>();
    }
}