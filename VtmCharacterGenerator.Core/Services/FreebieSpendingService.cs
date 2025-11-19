using System;
using System.Collections.Generic;
using System.Linq;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Enums;
using VtmCharacterGenerator.Core.Models;
using VtmCharacterGenerator.Core.Services.Strategies;

namespace VtmCharacterGenerator.Core.Services
{
    public class FreebieSpendingService
    {
        private readonly GameDataProvider _dataProvider;
        private readonly TraitManagerService _traitManager;
        private readonly AffinityProcessorService _affinityProcessor;
        private readonly ITraitCostStrategy _costStrategy;
        private readonly Random _random = new Random();

        // Base probabilities for each category
        private readonly Dictionary<TraitType, double> _baseWeights = new Dictionary<TraitType, double>
        {
            { TraitType.Background, 15 },
            { TraitType.Ability, 15 },
            { TraitType.Willpower, 12 },
            { TraitType.Virtue, 12 },
            { TraitType.Humanity, 12 },
            { TraitType.Attribute, 9 },
            { TraitType.Merit, 9 },
            { TraitType.Discipline, 6 }
        };

        public FreebieSpendingService(
            GameDataProvider dataProvider,
            TraitManagerService traitManager,
            AffinityProcessorService affinityProcessor)
        {
            _dataProvider = dataProvider;
            _traitManager = traitManager;
            _affinityProcessor = affinityProcessor;
            _costStrategy = new FreebiePointCostStrategy();
        }

        public void DistributeFreebiePoints(Character character, Dictionary<string, int> affinityProfile)
        {
            int currentPoints = 15;

            // This ensures every character has some unique quirk before spending starts.
            ApplyMandatoryFlavor(character, affinityProfile, ref currentPoints);

            // Tracks how many times we bought from a category to apply diminishing returns (-3, -4, -5).
            var purchaseCounts = new Dictionary<TraitType, int>();
            foreach (TraitType type in Enum.GetValues(typeof(TraitType)))
            {
                purchaseCounts[type] = 0;
            }

            while (currentPoints > 0)
            {
  
                var weightedCategories = CalculateCategoryWeights(currentPoints, purchaseCounts, affinityProfile);

                if (weightedCategories.Count == 0) break;

                TraitType selectedCategory = SelectWeightedCategory(weightedCategories);

                // Note: The affinity profile is UPDATED inside these methods if a purchase is successful.
                bool purchaseSuccess = ExecutePurchase(character, selectedCategory, affinityProfile, ref currentPoints);

                if (purchaseSuccess)
                {
                    purchaseCounts[selectedCategory]++;
                }
            }
        }

        private Dictionary<TraitType, double> CalculateCategoryWeights(int currentPoints, Dictionary<TraitType, int> purchaseCounts, Dictionary<string, int> affinityProfile)
        {
            var weightedCategories = new Dictionary<TraitType, double>();

            foreach (var entry in _baseWeights)
            {
                TraitType type = entry.Key;
                double weight = entry.Value;

                // A: Affordability Check
                int estimatedCost = _costStrategy.GetCost(type);
                if (type == TraitType.Merit) estimatedCost = 1; // Minimum merit cost.

                if (currentPoints < estimatedCost) continue;

                int count = purchaseCounts[type];
                int penalty = 0;
                if (count >= 1) penalty += 3;
                if (count >= 2) penalty += 4;
                if (count >= 3) penalty += 5; // Max penalty

                weight -= penalty;

                // Looks for tags like "spending:attribute"
                string affinityTag = $"spending:{type.ToString().ToLower()}";
                if (affinityProfile.ContainsKey(affinityTag))
                {
                    weight *= (1 + (affinityProfile[affinityTag] / 100.0));
                }

                // Ensure weight is at least 1
                if (weight < 1) weight = 1;

                weightedCategories[type] = weight;
            }

            return weightedCategories;
        }

        private void ApplyMandatoryFlavor(Character character, Dictionary<string, int> affinityProfile, ref int points)
        {
           
            if (_dataProvider.Flaws != null && _dataProvider.Flaws.Any())
            {
                var flaw = _affinityProcessor.GetWeightedRandom(_dataProvider.Flaws, affinityProfile);

                if (flaw != null && _traitManager.TryAddFlaw(character, flaw, ref points))
                {
                    _affinityProcessor.ProcessAffinities(affinityProfile, flaw.Affinities);
                }
            }

           
            if (_dataProvider.Merits != null && _dataProvider.Merits.Any())
            {
                int budget = points;
                var affordableMerits = _dataProvider.Merits.Where(m => m.Cost <= budget).ToList();

                if (affordableMerits.Any())
                {
                    var merit = _affinityProcessor.GetWeightedRandom(affordableMerits, affinityProfile);

                    if (merit != null && _traitManager.TryAddMerit(character, merit, ref points))
                    {
                        _affinityProcessor.ProcessAffinities(affinityProfile, merit.Affinities);
                    }
                }
            }
        }

        private TraitType SelectWeightedCategory(Dictionary<TraitType, double> weightedCategories)
        {
            double totalWeight = weightedCategories.Values.Sum();
            double randomValue = _random.NextDouble() * totalWeight;

            foreach (var pair in weightedCategories)
            {
                if (randomValue < pair.Value) return pair.Key;
                randomValue -= pair.Value;
            }

            return weightedCategories.Keys.Last();
        }

        private bool ExecutePurchase(Character character, TraitType type, Dictionary<string, int> affinityProfile, ref int currentPoints)
        {
            switch (type)
            {
                case TraitType.Attribute:
                    return BuyAttribute(character, affinityProfile, ref currentPoints);
                case TraitType.Ability:
                    return BuyAbility(character, affinityProfile, ref currentPoints);
                case TraitType.Discipline:
                    return BuyDiscipline(character, affinityProfile, ref currentPoints);
                case TraitType.Background:
                    return BuyBackground(character, affinityProfile, ref currentPoints);
                case TraitType.Virtue:
                    return BuyVirtue(character, affinityProfile, ref currentPoints);
                case TraitType.Willpower:
                    return _traitManager.TryIncreaseTrait(character, TraitType.Willpower, "willpower", _costStrategy, ref currentPoints);
                case TraitType.Humanity:
                    return _traitManager.TryIncreaseTrait(character, TraitType.Humanity, "humanity", _costStrategy, ref currentPoints);
                case TraitType.Merit:
                    return BuyMerit(character, affinityProfile, ref currentPoints);
                default:
                    return false;
            }
        }

        // Specific Buying Logic 

        private bool BuyAttribute(Character character, Dictionary<string, int> affinityProfile, ref int currentPoints)
        {
            var allAttributes = _dataProvider.AttributeCategories.SelectMany(c => c.Attributes).ToList();

            var candidates = allAttributes
                .Where(a => !character.Attributes.ContainsKey(a.Id) || character.Attributes[a.Id] < 5)
                .ToList();

            if (!candidates.Any()) return false;

            var selected = _affinityProcessor.GetWeightedRandom(candidates, affinityProfile);
            if (selected == null) return false;

            if (_traitManager.TryIncreaseTrait(character, TraitType.Attribute, selected.Id, _costStrategy, ref currentPoints))
            {
                _affinityProcessor.ProcessAffinities(affinityProfile, selected.Affinities);
                return true;
            }
            return false;
        }

        private bool BuyAbility(Character character, Dictionary<string, int> affinityProfile, ref int currentPoints)
        {
            var candidates = _dataProvider.Abilities
                .Where(a => !character.Abilities.ContainsKey(a.Id) || character.Abilities[a.Id] < 5)
                .ToList();

            if (!candidates.Any()) return false;

            var selected = _affinityProcessor.GetWeightedRandom(candidates, affinityProfile);
            if (selected == null) return false;

            if (_traitManager.TryIncreaseTrait(character, TraitType.Ability, selected.Id, _costStrategy, ref currentPoints))
            {
                _affinityProcessor.ProcessAffinities(affinityProfile, selected.Affinities);
                return true;
            }
            return false;
        }

        private bool BuyDiscipline(Character character, Dictionary<string, int> affinityProfile, ref int currentPoints)
        {
            var knownIds = character.Disciplines.Keys.ToList();
            var candidates = _dataProvider.Disciplines
                .Where(d => knownIds.Contains(d.Id) && character.Disciplines[d.Id] < 5)
                .ToList();

            if (!candidates.Any()) return false;

            var selected = _affinityProcessor.GetWeightedRandom(candidates, affinityProfile);
            if (selected == null) return false;

            if (_traitManager.TryIncreaseTrait(character, TraitType.Discipline, selected.Id, _costStrategy, ref currentPoints))
            {
                _affinityProcessor.ProcessAffinities(affinityProfile, selected.Affinities);
                return true;
            }
            return false;
        }

        private bool BuyBackground(Character character, Dictionary<string, int> affinityProfile, ref int currentPoints)
        {
            var candidates = _dataProvider.Backgrounds
                .Where(b => !character.Backgrounds.ContainsKey(b.Id) || character.Backgrounds[b.Id] < 5)
                .ToList();

            if (!candidates.Any()) return false;

            var selected = _affinityProcessor.GetWeightedRandom(candidates, affinityProfile);
            if (selected == null) return false;

            if (_traitManager.TryIncreaseTrait(character, TraitType.Background, selected.Id, _costStrategy, ref currentPoints))
            {
                _affinityProcessor.ProcessAffinities(affinityProfile, selected.Affinities);
                return true;
            }
            return false;
        }

        private bool BuyVirtue(Character character, Dictionary<string, int> affinityProfile, ref int currentPoints)
        {
            var candidates = _dataProvider.Virtues
                .Where(v => !character.Virtues.ContainsKey(v.Id) || character.Virtues[v.Id] < 5)
                .ToList();

            if (!candidates.Any()) return false;

            var selected = _affinityProcessor.GetWeightedRandom(candidates, affinityProfile);
            if (selected == null) return false;

            if (_traitManager.TryIncreaseTrait(character, TraitType.Virtue, selected.Id, _costStrategy, ref currentPoints))
            {
                _affinityProcessor.ProcessAffinities(affinityProfile, selected.Affinities);
                return true;
            }
            return false;
        }

        private bool BuyMerit(Character character, Dictionary<string, int> affinityProfile, ref int currentPoints)
        {
            int budget = currentPoints;
            var candidates = _dataProvider.Merits
                .Where(m => m.Cost <= budget && !character.Merits.Exists(x => x.Id == m.Id))
                .ToList();

            if (!candidates.Any()) return false;

            var selected = _affinityProcessor.GetWeightedRandom(candidates, affinityProfile);
            if (selected == null) return false;

            if (_traitManager.TryAddMerit(character, selected, ref currentPoints))
            {
                _affinityProcessor.ProcessAffinities(affinityProfile, selected.Affinities);
                return true;
            }
            return false;
        }
    }
}