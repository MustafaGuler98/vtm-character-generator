using System;
using System.Collections.Generic;
using System.Linq;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class NameGeneratorService
    {
        private readonly GameDataProvider _dataProvider;
        private readonly AffinityProcessorService _affinityProcessor;
        private readonly Random _random = new Random();

        public NameGeneratorService(GameDataProvider dataProvider, AffinityProcessorService affinityProcessor)
        {
            _dataProvider = dataProvider;
            _affinityProcessor = affinityProcessor;
        }

        public string GenerateName(Character character, Dictionary<string, int> affinityProfile)
        {
            if (!_dataProvider.NamePacks.Any()) return "Unknown Kindred";

            string nativeEra = DetermineEraByAge(character.Age);
            string targetEra = SelectTargetEra(nativeEra);

            NamePack selectedFirstNamePack = SelectFirstNamePack(targetEra, affinityProfile);

            if (selectedFirstNamePack == null) return "Nameless";

            NamePack selectedLastNamePack = SelectLastNamePack(selectedFirstNamePack, targetEra);

            string firstName = GetRandomValue(selectedFirstNamePack);
            string lastName = selectedLastNamePack != null ? GetRandomValue(selectedLastNamePack) : "";

            return $"{firstName} {lastName}".Trim();
        }

        private string DetermineEraByAge(int age)
        {
            if (age >= 1525) return "Ancient";
            if (age >= 525) return "Medieval";
            if (age >= 125) return "EarlyModern";
            return "Modern";
        }

        private string SelectTargetEra(string nativeEra)
        {
            var allEras = new List<string> { "Ancient", "Medieval", "EarlyModern", "Modern" };
            int roll = _random.Next(1, 101);

            // 97% chance to stick with the native era to maintain thematic consistency.
            if (roll <= 97)
            {
                return nativeEra;
            }

            // The remaining 3% represents anomalies (e.g. an Elder using a modern alias).
            var otherEras = allEras.Where(e => e != nativeEra).ToList();
            if (otherEras.Any())
            {
                return otherEras[_random.Next(otherEras.Count)];
            }

            return nativeEra;
        }

        private NamePack SelectFirstNamePack(string era, Dictionary<string, int> affinityProfile)
        {
            var candidates = _dataProvider.NamePacks
                .Where(p => p.Era == era && p.Type == "FirstName")
                .ToList();

            if (!candidates.Any()) return null;

            var weightedCandidates = new Dictionary<NamePack, int>();
            int totalWeight = 0;

            foreach (var pack in candidates)
            {
                int score = 100;

                foreach (var tag in pack.Tags)
                {
                    string key = tag.Trim().ToLowerInvariant();
                    if (affinityProfile.TryGetValue(key, out int value))
                    {
                        score += value;
                    }
                }

                // We ensure the score never drops below 1 so that every pack has at least "one ticket" in the lottery.
                if (score < 1) score = 1;

                weightedCandidates[pack] = score;
                totalWeight += score;
            }

            int choice = _random.Next(0, totalWeight);
            foreach (var kvp in weightedCandidates)
            {
                if (choice < kvp.Value)
                    return kvp.Key;
                choice -= kvp.Value;
            }

            return candidates.Last();
        }

        private NamePack SelectLastNamePack(NamePack firstNamePack, string era)
        {
            int roll = _random.Next(1, 101);

            // We prioritize the specific last name pack linked in the JSON to ensure cultural matching (e.g. Roman Nomen + Cognomen).
            if (roll <= 97 && !string.IsNullOrEmpty(firstNamePack.LinkedLastNameId))
            {
                var linkedPack = _dataProvider.NamePacks
                    .FirstOrDefault(p => p.Id == firstNamePack.LinkedLastNameId);

                if (linkedPack != null) return linkedPack;
            }

            //Same Era Random (2% or fallback)
            if (roll <= 99)
            {
                var eraPacks = _dataProvider.NamePacks
                    .Where(p => p.Type == "LastName" && p.Era == era)
                    .ToList();

                if (eraPacks.Any())
                    return eraPacks[_random.Next(eraPacks.Count)];
            }
            // Complete chaos option for unique results. Global Random (1%)
            var allLastNamePacks = _dataProvider.NamePacks
                .Where(p => p.Type == "LastName")
                .ToList();

            if (allLastNamePacks.Any())
                return allLastNamePacks[_random.Next(allLastNamePacks.Count)];

            return null;
        }

        private string GetRandomValue(NamePack pack)
        {
            if (pack == null || !pack.Values.Any()) return "";
            return pack.Values[_random.Next(pack.Values.Count)];
        }
    }
}