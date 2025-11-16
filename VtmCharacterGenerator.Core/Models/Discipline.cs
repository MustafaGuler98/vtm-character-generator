using System.Collections.Generic;
using System.Text.Json.Serialization;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Models
{
    public class Discipline : IHasTags
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [JsonPropertyName("affinities")]
        public List<Affinity> Affinities { get; set; } = new List<Affinity>();
    }
}