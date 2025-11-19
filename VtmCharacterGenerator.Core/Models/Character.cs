using System.Collections.Generic;

namespace VtmCharacterGenerator.Core.Models
{
    public class Character
    {
        public Concept Concept { get; set; }
        public Clan Clan { get; set; }
        public Nature Nature { get; set; }
        public Nature Demeanor { get; set; }
        public Dictionary<string, int> Attributes { get; set; }
        public Dictionary<string, int> Abilities { get; set; }
        public Dictionary<string, int> Backgrounds { get; set; }
        public Dictionary<string, int> Virtues { get; set; }
        public Dictionary<string, int> Disciplines { get; set; }
        public List<Merit> Merits { get; set; }
        public List<Flaw> Flaws { get; set; }
        public List<string> DebugLog { get; set; }
        public int Generation { get; set; }
        public int MaxTraitRating { get; set; }
        public int MaximumBloodPool { get; set; }
        public int BloodPointsPerTurn { get; set; }
        public int Humanity { get; set; }
        public int Willpower { get; set; }
      

        public Character()
        {
            Attributes = new Dictionary<string, int>();
            Abilities = new Dictionary<string, int>();
            Backgrounds = new Dictionary<string, int>();
            Virtues = new Dictionary<string, int>();
            Disciplines = new Dictionary<string, int>();
            Merits = new List<Merit>();
            Flaws = new List<Flaw>();
            DebugLog = new List<string>();
        }
    }
}