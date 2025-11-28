using System;
using System.Collections.Generic;
using System.Linq;
using VtmCharacterGenerator.Core.Enums;
using VtmCharacterGenerator.Core.Models;
using VtmCharacterGenerator.Core.Services.XpStrategies;

namespace VtmCharacterGenerator.Core.Services
{
    public class XpSpendingService
    {
        private readonly IEnumerable<IXpStrategy> _strategies;
        private readonly Random _random = new Random();

        public XpSpendingService(IEnumerable<IXpStrategy> strategies)
        {
            _strategies = strategies;
        }

        public void DistributeXp(Character character, Dictionary<string, int> affinityProfile)
        {
            foreach (var strategy in _strategies)
            {
                strategy.ResetAvailability();
            }

            // Not sure if needed, but just in case
            int consecutiveFailures = 0;

            while (character.SpentExperience < character.TotalExperience)
            {
                int remainingXp = character.TotalExperience - character.SpentExperience;

                int localSpentXp = character.SpentExperience;

                var weights = GetTierWeights(character.SpentExperience);
                ApplyTagModifiers(weights, affinityProfile);
                ApplyHardLogicModifiers(weights, character);

                var validStrategies = _strategies
                    .Where(s => weights.ContainsKey(s.Type) &&
                                s.IsAvailable &&
                                s.MinRequiredXp <= remainingXp)
                    .ToList();

                if (!validStrategies.Any())
                {
                    break;
                }

                var selectedStrategy = SelectStrategy(validStrategies, weights);

                bool success = selectedStrategy.TrySpendXp(
                    character,
                    affinityProfile,
                    remainingXp,
                    ref localSpentXp
                );

                character.SpentExperience = localSpentXp;

                if (success)
                {
                    consecutiveFailures = 0;
                }
                else
                {
                    consecutiveFailures++;

                    if (consecutiveFailures >= 10)
                    {
                        character.DebugLog.Add("[XP] Stopped early due to consecutive failures.");
                        break;
                    }
                }
            }
        }

        private Dictionary<TraitType, double> GetTierWeights(int spentXp)
        {
            if (spentXp < 75)
            {
                return new Dictionary<TraitType, double>
                {
                    { TraitType.Discipline, 16 },
                    { TraitType.Attribute, 10 },
                    { TraitType.Ability, 20 },
                    { TraitType.Willpower, 10 },
                    { TraitType.Virtue, 3 },
                    { TraitType.Humanity, 8 }
                };
            }
            else if (spentXp < 250)
            {
                return new Dictionary<TraitType, double>
                {
                    { TraitType.Discipline, 14 },
                    { TraitType.Attribute, 8 },
                    { TraitType.Ability, 20 },
                    { TraitType.Willpower, 4 },
                    { TraitType.Virtue, 3 },
                    { TraitType.Humanity, 2 }
                };
            }
            else
            {
                return new Dictionary<TraitType, double>
                {
                    { TraitType.Discipline, 10 },
                    { TraitType.Attribute, 4 },
                    { TraitType.Ability, 10 },
                    { TraitType.Willpower, 2 },
                    { TraitType.Virtue, 1 },
                    { TraitType.Humanity, 1 }
                };
            }
        }

        private void ApplyTagModifiers(Dictionary<TraitType, double> weights, Dictionary<string, int> affinityProfile)
        {
            foreach (var type in weights.Keys.ToList())
            {
                string tag = $"xp{type.ToString().ToLower()}";

                if (affinityProfile.TryGetValue(tag, out int modifier))
                {
                    // Rule: Maximum %50 bonus or penalty
                    double percent = Math.Clamp(modifier, -50, 50) / 100.0;

                    weights[type] *= (1.0 + percent);

                    // Cant go below 1 weight
                    if (weights[type] < 1) weights[type] = 1;
                }
            }
        }

        private void ApplyHardLogicModifiers(Dictionary<TraitType, double> weights, Character character)
        {
            // As Willpower increases, the probability of selecting it again decreases.
            if (weights.ContainsKey(TraitType.Willpower))
            {
                int current = character.Willpower;
                double penalty = 0;

                if (current == 5) penalty = -2;
                else if (current == 6) penalty = -4;
                else if (current == 7) penalty = -6;
                else if (current >= 8) penalty = -8;

                if (penalty != 0)
                {
                    weights[TraitType.Willpower] += penalty;

                    // Safety floor
                    if (weights[TraitType.Willpower] < 1) weights[TraitType.Willpower] = 1;
                }
            }
        }

        private IXpStrategy SelectStrategy(List<IXpStrategy> candidates, Dictionary<TraitType, double> weights)
        {
            double totalWeight = candidates.Sum(s => weights[s.Type]);
            double roll = _random.NextDouble() * totalWeight;

            foreach (var strategy in candidates)
            {
                double w = weights[strategy.Type];
                if (roll < w) return strategy;
                roll -= w;
            }
            return candidates.Last();
        }
    }
}