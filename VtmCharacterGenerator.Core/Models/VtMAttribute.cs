using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VtmCharacterGenerator.Core.Models
{
    public class VtMAttribute : IHasTags
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