using System.Text.Json.Serialization;

namespace VtmCharacterGenerator.Core.Models
{
    public class GenerationData
    {
        [JsonPropertyName("generation")]
        public int Generation { get; set; }

        [JsonPropertyName("maxTraitRating")]
        public int MaxTraitRating { get; set; }

        [JsonPropertyName("maxBloodPool")]
        public int MaximumBloodPool { get; set; }

        [JsonPropertyName("bloodPerTurn")]
        public int BloodPointsPerTurn { get; set; }

        [JsonPropertyName("weight")]
        public int Weight { get; set; }
    }
}