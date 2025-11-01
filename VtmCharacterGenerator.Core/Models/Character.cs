﻿using System.Collections.Generic;

namespace VtmCharacterGenerator.Core.Models
{
    public class Character
    {
        public Concept Concept { get; set; }
        public Clan Clan { get; set; }
        public Nature Nature { get; set; }
        public Nature Demeanor { get; set; }
        public Dictionary<string, int> Attributes { get; set; }

        public Character()
        {
            Attributes = new Dictionary<string, int>();
        }
    }
}