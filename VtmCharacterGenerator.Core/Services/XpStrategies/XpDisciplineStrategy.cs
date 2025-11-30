using System;
using System.Collections.Generic;
using System.Linq;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Enums;
using VtmCharacterGenerator.Core.Models;
using VtmCharacterGenerator.Core.Services.Strategies;

namespace VtmCharacterGenerator.Core.Services.XpStrategies
{
    public class XpDisciplineStrategy : BaseXpStrategy
    {
        private readonly GameDataProvider _dataProvider;
        private readonly ITraitCostStrategy _costStrategy;
        private readonly AffinityProcessorService _affinityProcessor;
        private readonly Random _random = new Random();

        public override TraitType Type => TraitType.Discipline;

        public override int MinRequiredXp => 4;

        private readonly HashSet<string> _bloodMagicIds = new HashSet<string> { "thaumaturgy", "necromancy" };
        private readonly HashSet<string> _rareDisciplines = new HashSet<string>
        {
            "chimerstry",
            "dementation",
            "necromancy",
            "obtenebration",
            "serpentis",
            "thaumaturgy",
            "vicissitude"
        };

        public XpDisciplineStrategy(
            GameDataProvider dataProvider,
            ITraitCostStrategy costStrategy,
            AffinityProcessorService affinityProcessor)
        {
            _dataProvider = dataProvider;
            _costStrategy = costStrategy;
            _affinityProcessor = affinityProcessor;
        }

        public override bool TrySpendXp(Character character, Dictionary<string, int> affinityProfile, int budget, ref int spentXp)
        {
            var upgradeCandidates = new List<Discipline>();
            var newCandidates = new List<Discipline>();

            foreach (var disc in _dataProvider.Disciplines)
            {
                string id = disc.Id;
                int currentRating = character.Disciplines.ContainsKey(id) ? character.Disciplines[id] : 0;
                bool isClan = character.Clan != null && character.Clan.Disciplines.Contains(id);

                if (currentRating > 0)
                {
                    if (currentRating >= character.MaxTraitRating) continue;

                    int estimatedCost = _costStrategy.GetCost(TraitType.Discipline, currentRating, isClan);
                    if (estimatedCost <= budget)
                    {
                        upgradeCandidates.Add(disc);
                    }
                }
                else
                {
                    int estimatedCost = _costStrategy.GetCost(TraitType.Discipline, 0, isClan);
                    if (estimatedCost <= budget)
                    {
                        // Filter rare non-clan disciplines
                        if (_rareDisciplines.Contains(id) && !isClan)
                        {
                            if (_random.NextDouble() > 0.20) continue;
                        }

                        newCandidates.Add(disc);
                    }
                }
            }

            bool preferUpgrade = false;

            if (upgradeCandidates.Count > 0 && newCandidates.Count > 0)
            {
                preferUpgrade = _random.NextDouble() < 0.85;
            }
            else if (upgradeCandidates.Count > 0)
            {
                preferUpgrade = true;
            }
            else if (newCandidates.Count > 0)
            {
                preferUpgrade = false;
            }
            else
            {
                _isAvailable = false;
                return false;
            }

            Discipline selectedDiscipline = null;

            if (preferUpgrade)
            {
                selectedDiscipline = _affinityProcessor.GetWeightedRandom(upgradeCandidates, affinityProfile);
            }
            else
            {
                selectedDiscipline = _affinityProcessor.GetWeightedRandom(newCandidates, affinityProfile);
            }

            if (selectedDiscipline == null) return false;

            if (_bloodMagicIds.Contains(selectedDiscipline.Id))
            {
                return HandleBloodMagic(character, selectedDiscipline.Id, character.Clan.Disciplines.Contains(selectedDiscipline.Id), budget, ref spentXp);
            }
            else
            {
                return HandleStandardDiscipline(character, selectedDiscipline.Id, character.Clan.Disciplines.Contains(selectedDiscipline.Id), budget, ref spentXp);
            }
        }

        private bool HandleStandardDiscipline(Character character, string discId, bool isClan, int budget, ref int spentXp)
        {
            int current = character.Disciplines.ContainsKey(discId) ? character.Disciplines[discId] : 0;

            int cost = _costStrategy.GetCost(TraitType.Discipline, current, isClan);

            if (cost > budget) return false;
            if (current >= character.MaxTraitRating) return false;

            if (!character.Disciplines.ContainsKey(discId)) character.Disciplines[discId] = 0;
            character.Disciplines[discId]++;
            spentXp += cost;

            character.DebugLog.Add($"[XP] Increased Discipline {discId} to {character.Disciplines[discId]} (Cost: {cost})");
            return true;
        }

        private bool HandleBloodMagic(Character character, string primaryId, bool isClan, int budget, ref int spentXp)
        {
            int primaryRating = character.Disciplines.ContainsKey(primaryId) ? character.Disciplines[primaryId] : 0;

            if (primaryRating == 0)
            {
                int newCost = _costStrategy.GetCost(TraitType.Discipline, 0, isClan);

                if (newCost > budget) return false;

                character.Disciplines[primaryId] = 1;
                spentXp += newCost;
                character.DebugLog.Add($"[XP] Learned {primaryId} (1) (Cost: {newCost})");
                return true;
            }

            var secondaries = character.Disciplines.Keys
                .Where(k => k.StartsWith($"{primaryId} ("))
                .ToList();

            // 0 = Upgrade Primary, 1 = Upgrade Secondary, 2 = New Path
            var chances = new Dictionary<int, int>();

            if (secondaries.Count == 0)
            {
                // Scenario A: Only Primary exists
                if (primaryRating == 1) { chances[0] = 100; chances[2] = 0; }
                else if (primaryRating == 2) { chances[0] = 95; chances[2] = 5; }
                else if (primaryRating == 3) { chances[0] = 85; chances[2] = 15; }
                else if (primaryRating == 4) { chances[0] = 60; chances[2] = 40; }
                else { chances[0] = 25; chances[2] = 75; }
            }
            else
            {
                // Scenario B: Secondaries exist
                if (primaryRating == 2) { chances[0] = 95; chances[1] = 4; chances[2] = 1; }
                else if (primaryRating == 3) { chances[0] = 60; chances[1] = 30; chances[2] = 10; }
                else if (primaryRating == 4) { chances[0] = 50; chances[1] = 35; chances[2] = 15; }
                else { chances[0] = 20; chances[1] = 60; chances[2] = 20; }
            }

            var availableActions = chances.Where(x => x.Value > 0).Select(x => x.Key).ToList();

            while (availableActions.Count > 0)
            {
                int totalWeight = availableActions.Sum(a => chances[a]);
                int roll = _random.Next(0, totalWeight);
                int selectedAction = -1;

                foreach (var action in availableActions)
                {
                    if (roll < chances[action]) { selectedAction = action; break; }
                    roll -= chances[action];
                }

                if (selectedAction == -1) selectedAction = availableActions.Last();

                bool success = false;
                switch (selectedAction)
                {
                    case 0: success = TryUpgradePrimary(character, primaryId, isClan, budget, ref spentXp); break;
                    case 1: success = TryUpgradeSecondary(character, secondaries, primaryRating, budget, ref spentXp); break;
                    case 2: success = TryBuyNewPath(character, primaryId, secondaries, primaryRating, budget, ref spentXp); break;
                }

                if (success) return true;

                availableActions.Remove(selectedAction);
            }

            return false;
        }

        private bool TryUpgradePrimary(Character character, string id, bool isClan, int budget, ref int spentXp)
        {
            int current = character.Disciplines[id];

            if (current >= character.MaxTraitRating) return false;

            int cost = _costStrategy.GetCost(TraitType.Discipline, current, isClan);
            if (cost > budget) return false;

            character.Disciplines[id]++;
            spentXp += cost;
            character.DebugLog.Add($"[XP] Upgraded Primary {id} to {character.Disciplines[id]} (Cost: {cost})");

            // Level 5+ Bonus
            if (character.Disciplines[id] > 5)
            {
                ApplyHighLevelBonus(character, id);
            }

            return true;
        }

        private bool TryUpgradeSecondary(Character character, List<string> secondaries, int primaryRating, int budget, ref int spentXp)
        {
            if (secondaries.Count == 0) return false;

            var candidates = secondaries.OrderBy(x => _random.Next()).ToList();

            foreach (var secId in candidates)
            {
                int current = character.Disciplines[secId];

                if (current >= primaryRating) continue;
                if (current >= 5) continue;

                int cost = _costStrategy.GetCost(TraitType.Discipline, current, false, true);

                if (cost <= budget)
                {
                    character.Disciplines[secId]++;
                    spentXp += cost;
                    character.DebugLog.Add($"[XP] Upgraded Secondary {secId} to {character.Disciplines[secId]} (Cost: {cost})");
                    return true;
                }
            }
            return false;
        }

        private bool TryBuyNewPath(Character character, string primaryId, List<string> secondaries, int primaryRating, int budget, ref int spentXp)
        {
            if (primaryRating <= 1) return false;

            int cost = _costStrategy.GetCost(TraitType.Discipline, 0, false, true);
            if (cost > budget) return false;

            // Determine Name: "thaumaturgy (2)", "thaumaturgy (3)"...
            int index = 2;
            while (character.Disciplines.ContainsKey($"{primaryId} ({index})"))
            {
                index++;
            }
            string newName = $"{primaryId} ({index})";

            character.Disciplines[newName] = 1;
            spentXp += cost;
            character.DebugLog.Add($"[XP] Acquired New Path {newName} (Cost: {cost})");

            return true;
        }

        private void ApplyHighLevelBonus(Character character, string primaryId)
        {
            var secondaries = character.Disciplines.Keys
                .Where(k => k.StartsWith($"{primaryId} ("))
                .ToList();

            var validSecondaries = secondaries.Where(s => character.Disciplines[s] < 5).ToList();

            if (validSecondaries.Any())
            {
                string target = validSecondaries[_random.Next(validSecondaries.Count)];
                character.Disciplines[target]++;
                character.DebugLog.Add($"[Bonus] {target} increased freely due to Master Level mastery.");
            }
            else
            {
                int index = 2;
                while (character.Disciplines.ContainsKey($"{primaryId} ({index})"))
                {
                    index++;
                }
                string newName = $"{primaryId} ({index})";
                character.Disciplines[newName] = 1;
                character.DebugLog.Add($"[Bonus] {newName} unlocked freely due to Master Level mastery.");
            }
        }
    }
}