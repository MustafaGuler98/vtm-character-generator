using System.Collections.Generic;
using System.Linq;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class AffinityProcessorService
    {
        private readonly Random _random = new Random();
        private readonly int _baseScore;
        private readonly int _minAffinityClamp;  
        private readonly int _maxAffinityClamp;

        // Make baseScore configurable; default 25
        public AffinityProcessorService(int baseScore = 25, int minAffinityClamp = -1000, int maxAffinityClamp = 100000)
        {
            _baseScore = baseScore;
            _minAffinityClamp = minAffinityClamp;
            _maxAffinityClamp = maxAffinityClamp;
        }

        public Dictionary<string, int> BuildAffinityProfile(Persona persona)
        {
            var profile = new Dictionary<string, int>();

       
            ProcessAffinities(profile, persona.Concept?.Affinities);
            ProcessAffinities(profile, persona.Clan?.Affinities);
            ProcessAffinities(profile, persona.Nature?.Affinities);
            ProcessAffinities(profile, persona.Demeanor?.Affinities);

            return profile;
        }

        public void ProcessAffinities(Dictionary<string, int> profile, List<Affinity> affinities)
        {
            if (affinities == null) return;

            foreach (var affinity in affinities)
            {
                if (string.IsNullOrEmpty(affinity.Tag)) continue;
                var key = affinity.Tag.Trim().ToLowerInvariant();
                if (!profile.ContainsKey(key)) profile[key] = 0;
                profile[key] += affinity.Value;

                // optional: clamp per-tag to avoid insane values in JSON
                profile[key] = Math.Clamp(profile[key], _minAffinityClamp, _maxAffinityClamp);
            }
        }

      
        public T GetWeightedRandom<T>(List<T> items, Dictionary<string, int> affinityProfile) where T : IHasTags
        {
            if (items == null || !items.Any()) return default;

            var profile = affinityProfile == null
                ? new Dictionary<string,int>()
                : affinityProfile.ToDictionary(k => k.Key.Trim().ToLowerInvariant(), v => Math.Clamp(v.Value, _minAffinityClamp, _maxAffinityClamp));

            var weightedList = new List<(T item, int score)>();
            int totalScore = 0;

            foreach (var item in items)
            {
                int currentScore = _baseScore;

                if (item.Tags != null)
                {
                    foreach (var tag in item.Tags)
                    {
                        if (string.IsNullOrEmpty(tag)) continue;
                        var tagKey = tag.Trim().ToLowerInvariant();
                        if (profile.ContainsKey(tagKey))
                        {
                            currentScore += profile[tagKey];
                        }
                    }
                }

                // Ensure no item gets less than the base score (prevents negatives zeroing out chance)
                currentScore = Math.Max(_baseScore, currentScore);

                weightedList.Add((item, currentScore));
                totalScore += currentScore;
            }

            // defensive: if totalScore ever becomes zero (shouldn't with base clamp), fallback
            if (totalScore <= 0) return items.FirstOrDefault();

            int randomNumber = _random.Next(0, totalScore);
            foreach (var entry in weightedList)
            {
                if (randomNumber < entry.score) return entry.item;
                randomNumber -= entry.score;
            }

            return items.LastOrDefault();
        }
    }
}