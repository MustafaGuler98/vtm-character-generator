using System.Collections.Generic;
using System.Linq;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class AffinityProcessorService
    {
        private readonly Random _random = new Random();

        public Dictionary<string, int> BuildAffinityProfile(Persona persona)
        {
            var profile = new Dictionary<string, int>();

       
            ProcessAffinities(profile, persona.Concept?.Affinities);
            ProcessAffinities(profile, persona.Clan?.Affinities);
            ProcessAffinities(profile, persona.Nature?.Affinities);
            ProcessAffinities(profile, persona.Demeanor?.Affinities);

            return profile;
        }

        private void ProcessAffinities(Dictionary<string, int> profile, List<Affinity> affinities)
        {
            if (affinities == null) return;

            foreach (var affinity in affinities)
            {
                if (!string.IsNullOrEmpty(affinity.Tag))
                {
                    if (!profile.ContainsKey(affinity.Tag))
                    {
                        profile[affinity.Tag] = 0;
                    }
                    profile[affinity.Tag] += affinity.Value;
                }
            }
        }

      
        public T GetWeightedRandom<T>(List<T> items, Dictionary<string, int> affinityProfile) where T : IHasTags
        {
            if (items == null || !items.Any())
            {
                return default(T);
            }

            var weightedList = new List<(T item, int score)>();
            int totalScore = 0;

            foreach (var item in items)
            {
                int currentScore = 100; // Base score for every item to have a chance

                foreach (var tag in item.Tags)
                {
                    if (affinityProfile.ContainsKey(tag))
                    {
                        currentScore += affinityProfile[tag];
                    }
                }

                // Ensure score is not negative to avoid issues with random selection
                currentScore = Math.Max(1, currentScore);

                weightedList.Add((item, currentScore));
                totalScore += currentScore;
            }

           
            int randomNumber = _random.Next(0, totalScore);
            foreach (var entry in weightedList)
            {
                if (randomNumber < entry.score)
                {
                    return entry.item;
                }
                randomNumber -= entry.score;
            }

            return items.LastOrDefault();
        }
    }
}