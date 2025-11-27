using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VtmCharacterGenerator.Core.Models
{
    public class NamePack : IHasTags
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("Type")]
        public string Type { get; set; }

        [JsonPropertyName("Era")]
        public string Era { get; set; }

        [JsonPropertyName("Tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [JsonPropertyName("LinkedLastNameId")]
        public string LinkedLastNameId { get; set; } // Only for first names

        [JsonPropertyName("Values")]
        public List<string> Values { get; set; } = new List<string>();
    }
}