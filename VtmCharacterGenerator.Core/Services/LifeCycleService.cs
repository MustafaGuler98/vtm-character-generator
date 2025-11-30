using System;
using System.Collections.Generic;
using System.Linq;
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
            if (character.Age.HasValue)
            {
                character.AgeCategory = GetCategoryFromAge(character.Age.Value);
            }
            else if (!string.IsNullOrEmpty(character.AgeCategory))
            {
                character.Age = DetermineAge(character.AgeCategory);
            }

            else
            {
                // Ensure Generation is set (CoreStatsService runs before this, so it should be).
                int generation = character.Generation ?? 13;
                character.AgeCategory = DetermineRandomCategoryByGeneration(generation);
                character.Age = DetermineAge(character.AgeCategory);
            }


            character.TotalExperience = DetermineXp(character.AgeCategory, character.Age ?? 0);

            character.SpentExperience = 0;
        }

        private string GetCategoryFromAge(int age)
        {
            if (age < 100) return "Neonate";
            if (age < 200) return "Ancilla";
            if (age < 1000) return "Elder";
            return "Methuselah";
        }

        private string DetermineRandomCategoryByGeneration(int generation)
        {
            int roll = _random.Next(1, 101);

            if (generation >= 12)
            {
                if (roll <= 80) return "Neonate";
                if (roll <= 99) return "Ancilla";
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
                if (roll <= 1) return "Neonate";
                if (roll <= 15) return "Ancilla";
                return "Elder";
            }
            else if (generation >= 6)
            {
                if (roll <= 5) return "Ancilla";
                return "Elder";
            }
            else if (generation >= 4)
            {
                return "Methuselah";
            }
            else // Defensive
            {
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
                "Methuselah" => _random.Next(1000, 3001),
                _ => 0
            };
        }

        private int DetermineXp(string category, int age)
        {
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
            else if (category == "Elder" || age <= 1000)
            {
                // Elder: 200-1000 Age -> 250-600 XP
                minAge = 200; maxAge = 1000;
                minXp = 250; maxXp = 1000;
            }
            else // Methuselah
            {
                minAge = 1000; maxAge = 3000;
                minXp = 1000; maxXp = 2250;
            }

            double effectiveAge = Math.Min(age, maxAge);
            double ratio = (effectiveAge - minAge) / (maxAge - minAge);

            // Avoid division by zero if minAge == maxAge (edge case)
            if (maxAge == minAge) ratio = 0;

            double baseXp = minXp + (ratio * (maxXp - minXp));

            // +/- 20% variance
            double variancePercent = (_random.NextDouble() * 0.4) - 0.2;

            int finalXp = (int)(baseXp * (1.0 + variancePercent));
            return Math.Max(0, finalXp);
        }

        public void EvolveBackgrounds(Character character, Dictionary<string, int> affinityProfile)
        {
            // Age null check (default to 0)
            int currentAge = character.Age ?? 0;

            if (currentAge <= 50) return;

            int bonusPoints = 0;

            // ---Ages 50 to 100---
            if (currentAge > 50)
            {
                int effectiveAge = Math.Min(currentAge, 100);
                int checks = (effectiveAge - 50) / 10;

                for (int i = 0; i < checks; i++)
                {
                    if (_random.NextDouble() < 0.4) bonusPoints++;
                }
            }

            // ---Ages 100 to 400---
            if (currentAge > 100)
            {
                int effectiveAge = Math.Min(currentAge, 400);
                int checks = (effectiveAge - 100) / 10;

                for (int i = 0; i < checks; i++)
                {
                    if (_random.NextDouble() < 0.5) bonusPoints++;
                }
            }

            // ---Ages 400+---
            if (currentAge > 400)
            {
                int checks = (currentAge - 400) / 50;

                for (int i = 0; i < checks; i++)
                {
                    if (_random.NextDouble() < 0.5) bonusPoints++;
                }
            }
            // ---Ages 1000+---
            if (currentAge > 1000)
            {
                int checks = (currentAge - 1000) / 100;

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

                character.DebugLog.Add($"[Evolution] Gained Background point: {selectedBg.Name} -> {character.Backgrounds[id]} (Age: {currentAge})");
            }
        }

        public void ApplyHumanityDegeneration(Character character)
        {
            int currentAge = character.Age ?? 0;

            if (currentAge <= 50) return;

            int decayCycles = (currentAge - 50) / 50;

            for (int i = 0; i < decayCycles; i++)
            {
                if (character.Humanity <= 3) break;

                double chanceToLose = character.Humanity / 10.0;

                if (character.AgeCategory == "Elder") chanceToLose += 0.10;
                if (character.AgeCategory == "Methuselah") chanceToLose += 0.10;
                if (character.AgeCategory == "Ancilla") chanceToLose += 0.05;

                chanceToLose = Math.Min(chanceToLose, 0.95);

                if (_random.NextDouble() < chanceToLose)
                {
                    character.Humanity--;
                    character.DebugLog.Add($"[Decay] Humanity eroded to {character.Humanity} (Cycle {i + 1}/{decayCycles})");
                }
            }
        }
    }
}