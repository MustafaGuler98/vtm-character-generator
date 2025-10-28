using System.Text.Json.Serialization;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Models
{
    public class Discipline
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

    }
}