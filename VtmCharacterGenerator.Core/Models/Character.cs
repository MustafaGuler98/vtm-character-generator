using System.Collections.Generic;

namespace VtmCharacterGenerator.Core.Models
{
    public class Character
    {
        public required Concept Concept { get; set; }
        public required Clan Clan { get; set; }
        public required Nature Nature { get; set; }
        public required Nature Demeanor { get; set; }
        public string AgeCategory { get; set; } = string.Empty; // Neonate, Ancilla, Elder
        public string Name { get; set; } = "Unknown";
        public int? Age { get; set; }
        public int TotalExperience { get; set; }
        public int SpentExperience { get; set; }
        public Dictionary<string, int> Attributes { get; set; } = new();
        public Dictionary<string, int> Abilities { get; set; } = new();
        public Dictionary<string, int> Backgrounds { get; set; } = new();
        public Dictionary<string, int> Virtues { get; set; } = new();
        public Dictionary<string, int> Disciplines { get; set; } = new();
        public List<Merit> Merits { get; set; } = new();
        public List<Flaw> Flaws { get; set; } = new();
        public List<string> DebugLog { get; set; } = new();
        public int? Generation { get; set; }
        public int MaxTraitRating { get; set; }
        public int MaximumBloodPool { get; set; }
        public int BloodPointsPerTurn { get; set; }
        public int Humanity { get; set; }
        public int Willpower { get; set; }
    }
}
