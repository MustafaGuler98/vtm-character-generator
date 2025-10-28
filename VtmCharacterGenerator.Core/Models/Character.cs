namespace VtmCharacterGenerator.Core.Models
{
    public class Character
    {
        // public string Name { get; set; }
        public Clan Clan { get; set; }

        public Dictionary<string, int> Attributes { get; set; }

        public Character()
        {
            Attributes = new Dictionary<string, int>();
        }
    }
}