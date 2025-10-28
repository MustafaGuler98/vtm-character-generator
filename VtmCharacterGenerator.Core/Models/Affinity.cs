using System.Text.Json.Serialization;

namespace VtmCharacterGenerator.Core.Models
{
    public class Affinity
    {
        
        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        // How much this affinity modifies the score (e.g., 5, -3)
        [JsonPropertyName("value")]
        public int Value { get; set; }

        // Optional: for targeted affinities (e.g., to a specific clan or ability)
        [JsonPropertyName("targetType")]
        public string TargetType { get; set; }

        [JsonPropertyName("targetId")]
        public string TargetId { get; set; }
    }
}