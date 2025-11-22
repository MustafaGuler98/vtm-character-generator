using System;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class LifeCycleService
    {
        private readonly Random _random = new Random();

        public void DetermineLifeCycle(Character character)
        {
            if (string.IsNullOrEmpty(character.AgeCategory))
            {
                character.AgeCategory = DetermineCategory(character.Generation);
            }

            if (character.Age == 0)
            {
                character.Age = DetermineAge(character.AgeCategory);
            }

            if (character.TotalExperience == 0)
            {
                character.TotalExperience = DetermineXp(character.AgeCategory);
            }

            character.SpentExperience = 0;
        }

        private string DetermineCategory(int generation)
        {
            int roll = _random.Next(1, 101);

            if (generation >= 12)
            {
                if (roll <= 80) return "Neonate";
                if (roll <= 95) return "Ancilla";
                return "Elder";
            }
            else if (generation >= 10)
            {
                if (roll <= 40) return "Neonate";
                if (roll <= 90) return "Ancilla";
                return "Elder";
            }
            else if (generation >= 8)
            {
                if (roll <= 10) return "Neonate";
                if (roll <= 35) return "Ancilla";
                return "Elder";
            }
            else if (generation >= 6)
            {
                if (roll <= 20) return "Ancilla";
                return "Elder";
            }
            else
            {
                if (roll <= 5) return "Ancilla";
                return "Elder";
            }
        }

        private int DetermineAge(string category)
        {
            return category switch
            {
                "Neonate" => _random.Next(0, 100),
                "Ancilla" => _random.Next(100, 200),
                "Elder" => _random.Next(200, 1000),
                _ => 0
            };
        }

        private int DetermineXp(string category)
        {
            int roll = _random.Next(1, 101);

            if (category == "Neonate")
            {
                if (roll <= 80) return _random.Next(0, 36);
                return _random.Next(36, 76);
            }
            else if (category == "Ancilla")
            {
                if (roll <= 10) return _random.Next(35, 75);
                if (roll <= 90) return _random.Next(75, 221);
                return _random.Next(221, 251);
            }
            else // Elder
            {
                if (roll <= 10) return _random.Next(220, 250);
                return _random.Next(250, 601);
            }
        }
        public void ApplyHumanityDegeneration(Character character)
        {
  
            // 1. Calculate number of "Decay Cycles"
            if (character.Age <= 50) return;

            int decayCycles = (character.Age - 50) / 50;

            for (int i = 0; i < decayCycles; i++)
            {
                // Safety Floor: Never drop below 3 automatically
                if (character.Humanity <= 3) break;

                // Dynamic Probability:
                double chanceToLose = character.Humanity / 10.0;


                if (character.AgeCategory == "Elder") chanceToLose += 0.10;
                if (character.AgeCategory == "Ancilla") chanceToLose += 0.05;

                chanceToLose = Math.Min(chanceToLose, 0.95);

                // Roll
                if (_random.NextDouble() < chanceToLose)
                {
                    character.Humanity--;
                    character.DebugLog.Add($"[Decay] Humanity eroded to {character.Humanity} (Cycle {i + 1}/{decayCycles})");
                }
            }
        }
    }
}