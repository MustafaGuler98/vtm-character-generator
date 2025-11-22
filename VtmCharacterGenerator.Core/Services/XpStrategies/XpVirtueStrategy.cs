using System;
using System.Collections.Generic;
using System.Linq;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Enums;
using VtmCharacterGenerator.Core.Models;
using VtmCharacterGenerator.Core.Services.Strategies;

namespace VtmCharacterGenerator.Core.Services.XpStrategies
{
    public class XpVirtueStrategy : BaseXpStrategy
    {
        private readonly GameDataProvider _dataProvider;
        private readonly ITraitCostStrategy _costStrategy;
        private readonly Random _random = new Random();

        public override TraitType Type => TraitType.Virtue;
        public override int MinRequiredXp => 2;

        public XpVirtueStrategy(
            GameDataProvider dataProvider,
            ITraitCostStrategy costStrategy)
        {
            _dataProvider = dataProvider;
            _costStrategy = costStrategy;
        }

        public override bool TrySpendXp(Character character, Dictionary<string, int> affinityProfile, int budget, ref int spentXp)
        {
            // We manually calculate selection weights to implement a 'diminishing returns' logic where higher ratings are exponentially harder to increase. 
            // Specific penalties (-20 at rating 3, -40 at rating 4) are applied to the base affinity score, ensuring that while character tendencies (affinities) matter, 
            // achieving perfection in a Virtue is rare. A minimum score of 1 is enforced to maintain a non-zero probability for any valid purchase.

            var candidates = new Dictionary<Virtue, int>();
            int totalWeight = 0;

            foreach (var virtue in _dataProvider.Virtues)
            {
                if (!character.Virtues.ContainsKey(virtue.Id)) continue;
                int currentRating = character.Virtues[virtue.Id];

                // Hard Cap: Virtues cannot exceed 5, regardless of Generation.
                if (currentRating >= 5) continue;

                int cost = _costStrategy.GetCost(TraitType.Virtue, currentRating);
                if (cost > budget) continue;

                int score = 0;
                if (virtue.Tags != null)
                {
                    foreach (var tag in virtue.Tags)
                    {
                        string key = tag.Trim().ToLowerInvariant();
                        if (affinityProfile.TryGetValue(key, out int val))
                        {
                            score += val;
                        }
                    }
                }
                // Probably i should change this to a multiplier system later
                if (currentRating == 3) score -= 20;
                else if (currentRating == 4) score -= 40;

                // Safety Floor
                if (score < 1) score = 1;

                candidates.Add(virtue, score);
                totalWeight += score;
            }

            if (candidates.Count == 0)
            {
                _isAvailable = false;
                return false;
            }
            int roll = _random.Next(0, totalWeight);
            Virtue selectedVirtue = null;

            foreach (var candidate in candidates)
            {
                if (roll < candidate.Value)
                {
                    selectedVirtue = candidate.Key;
                    break;
                }
                roll -= candidate.Value;
            }

            if (selectedVirtue == null) selectedVirtue = candidates.Keys.Last();

            string id = selectedVirtue.Id;
            int finalCost = _costStrategy.GetCost(TraitType.Virtue, character.Virtues[id]);

            character.Virtues[id]++;
            spentXp += finalCost;

            character.DebugLog.Add($"[XP] Increased Virtue {selectedVirtue.Name} to {character.Virtues[id]} (Cost: {finalCost})");

            return true;
        }
    }
}