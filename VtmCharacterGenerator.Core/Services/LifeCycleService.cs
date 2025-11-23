using System;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class LifeCycleService
    {
        private readonly GameDataProvider _dataProvider;
        private readonly AffinityProcessorService _affinityProcessor;
        private readonly Random _random = new Random();

        public LifeCycleService(GameDataProvider dataProvider, AffinityProcessorService affinityProcessor)
        {
            _dataProvider = dataProvider;
            _affinityProcessor = affinityProcessor;
        }

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
                character.TotalExperience = DetermineXp(character.AgeCategory, character.Age);
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

        private int DetermineXp(string category, int age)
        {
            // +/- 20% variance.

            double minAge, maxAge, minXp, maxXp;

            if (category == "Neonate" || age <= 100)
            {
                // Neonate: 0-100 Age -> 0-75 XP
                minAge = 0; maxAge = 100;
                minXp = 0; maxXp = 75;
            }
            else if (category == "Ancilla" || age <= 200)
            {
                // Ancilla: 100-200 Age -> 75-250 XP
                minAge = 100; maxAge = 200;
                minXp = 75; maxXp = 250;
            }
            else // Elder
            {
                // Elder: 200-1000 Age -> 250-600 XP
                minAge = 200; maxAge = 1000;
                minXp = 250; maxXp = 600;
            }

            double effectiveAge = Math.Min(age, maxAge);
            double ratio = (effectiveAge - minAge) / (maxAge - minAge);

            double baseXp = minXp + (ratio * (maxXp - minXp));

            double variancePercent = (_random.NextDouble() * 0.4) - 0.2;

            int finalXp = (int)(baseXp * (1.0 + variancePercent));
            return Math.Max(0, finalXp);
        }


public void EvolveBackgrounds(Character character, Dictionary<string, int> affinityProfile)
        {
 
            if (character.Age <= 50) return;

            int bonusPoints = 0;

            // ---Ages 50 to 100---
            if (character.Age > 50)
            {
                int effectiveAge = Math.Min(character.Age, 100);
                int checks = (effectiveAge - 50) / 10;

                for (int i = 0; i < checks; i++)
                {
                    if (_random.NextDouble() < 0.4) bonusPoints++;
                }
            }

            // ---Ages 100 to 400---
            if (character.Age > 100)
            {
                int effectiveAge = Math.Min(character.Age, 400);
                int checks = (effectiveAge - 100) / 10;

                for (int i = 0; i < checks; i++)
                {
                    if (_random.NextDouble() < 0.5) bonusPoints++;
                }
            }

            //Ages 400---
            if (character.Age > 400)
            {
                int checks = (character.Age - 400) / 50;

                for (int i = 0; i < checks; i++)
                {
                    if (_random.NextDouble() < 0.5) bonusPoints++;
                }
            }

            if (bonusPoints == 0) return;

            var allBackgrounds = _dataProvider.Backgrounds;

            while (bonusPoints > 0)
            {
                var candidates = allBackgrounds.Where(bg =>
                    !character.Backgrounds.ContainsKey(bg.Id) ||
                    character.Backgrounds[bg.Id] < 5
                ).ToList();

                if (!candidates.Any()) break;

                var selectedBg = _affinityProcessor.GetWeightedRandom(candidates, affinityProfile);

                if (selectedBg == null) break;

                string id = selectedBg.Id;
                if (!character.Backgrounds.ContainsKey(id)) character.Backgrounds[id] = 0;

                character.Backgrounds[id]++;
                bonusPoints--;

                _affinityProcessor.ProcessAffinities(affinityProfile, selectedBg.Affinities);

                character.DebugLog.Add($"[Evolution] Gained Background point: {selectedBg.Name} -> {character.Backgrounds[id]} (Age: {character.Age})");
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