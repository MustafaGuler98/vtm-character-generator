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

        public Character()
        {
            Attributes = new Dictionary<string, int>();
            Abilities = new Dictionary<string, int>();
            Backgrounds = new Dictionary<string, int>();
            Virtues = new Dictionary<string, int>();
            Disciplines = new Dictionary<string, int>();
        }
    }
}