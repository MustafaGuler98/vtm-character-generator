using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VtmCharacterGenerator.Core.Models
{
    public class NamePack : IHasTags
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("Type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("Era")]
        public string Era { get; set; } = string.Empty;

        [JsonPropertyName("Tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("LinkedLastNameId")]
        public string? LinkedLastNameId { get; set; } // Only for first names

        [JsonPropertyName("Values")]
        public List<string> Values { get; set; } = new();
    }
}
