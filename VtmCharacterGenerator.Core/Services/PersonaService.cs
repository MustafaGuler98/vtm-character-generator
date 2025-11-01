using System.Linq;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class PersonaService
    {
        private readonly GameDataProvider _dataProvider;
        private readonly AffinityProcessorService _affinityProcessor;

        public PersonaService(GameDataProvider dataProvider, AffinityProcessorService affinityProcessor)
        {
            _dataProvider = dataProvider;
            _affinityProcessor = affinityProcessor;
        }

        public Persona CompletePersona(Persona inputPersona)
        {
            // Shallow copy so we don't mutate caller object
            var finalPersona = new Persona
            {
                Concept = inputPersona?.Concept,
                Clan = inputPersona?.Clan,
                Nature = inputPersona?.Nature,
                Demeanor = inputPersona?.Demeanor
            };

            // Helper to merge affinity dictionaries into running profile
            static void MergeInto(Dictionary<string, int> target, Dictionary<string, int> source)
            {
                if (source == null) return;
                foreach (var kv in source)
                {
                    var k = kv.Key?.Trim().ToLowerInvariant() ?? "";
                    if (!target.ContainsKey(k)) target[k] = 0;
                    target[k] += kv.Value;
                }
            }

            var runningAffinities = new Dictionary<string, int>();

            // Pre-add affinities from any user-selected fields. This makes user selections influence every later pick.
            if (finalPersona.Concept != null)
            {
                var a = _affinityProcessor.BuildAffinityProfile(new Persona { Concept = finalPersona.Concept });
                MergeInto(runningAffinities, a);
            }

            if (finalPersona.Clan != null)
            {
                var a = _affinityProcessor.BuildAffinityProfile(new Persona { Clan = finalPersona.Clan });
                MergeInto(runningAffinities, a);
            }

            if (finalPersona.Nature != null)
            {
                var a = _affinityProcessor.BuildAffinityProfile(new Persona { Nature = finalPersona.Nature });
                MergeInto(runningAffinities, a);
            }

            // Defensive: user-provided Demeanor must not equal Nature
            if (finalPersona.Demeanor != null && finalPersona.Nature != null && finalPersona.Demeanor.Id == finalPersona.Nature.Id)
            {
                finalPersona.Demeanor = null;
            }

            if (finalPersona.Demeanor != null)
            {
                var a = _affinityProcessor.BuildAffinityProfile(new Persona { Demeanor = finalPersona.Demeanor });
                MergeInto(runningAffinities, a);
            }

            if (finalPersona.Concept == null)
            {
                if (_dataProvider.Concepts != null && _dataProvider.Concepts.Any())
                {
                    var chosen = _affinityProcessor.GetWeightedRandom(_dataProvider.Concepts, runningAffinities);
                    if (chosen != null)
                    {
                        finalPersona.Concept = chosen;
                        // merge affinities from chosen concept into running profile
                        var a = _affinityProcessor.BuildAffinityProfile(new Persona { Concept = chosen });
                        MergeInto(runningAffinities, a);
                    }
                }
            }

            if (finalPersona.Clan == null)
            {
                if (_dataProvider.Clans != null && _dataProvider.Clans.Any())
                {
                    var chosen = _affinityProcessor.GetWeightedRandom(_dataProvider.Clans, runningAffinities);
                    if (chosen != null)
                    {
                        finalPersona.Clan = chosen;
                        var a = _affinityProcessor.BuildAffinityProfile(new Persona { Clan = chosen });
                        MergeInto(runningAffinities, a);
                    }
                }
            }

            if (finalPersona.Nature == null)
            {
                if (_data_providerAvailable(_dataProvider.Natures))
                {
                    var chosen = _affinityProcessor.GetWeightedRandom(_dataProvider.Natures, runningAffinities);
                    if (chosen != null)
                    {
                        finalPersona.Nature = chosen;
                        var a = _affinityProcessor.BuildAffinityProfile(new Persona { Nature = chosen });
                        MergeInto(runningAffinities, a);
                    }
                }
            }

            // Enforce rule: Nature and Demeanor cannot be the same
            if (finalPersona.Demeanor != null && finalPersona.Nature != null && finalPersona.Demeanor.Id == finalPersona.Nature.Id)
            {
                finalPersona.Demeanor = null;
            }

            if (finalPersona.Demeanor == null)
            {
                var availableDemeanors = _dataProvider.Natures?
                    .Where(n => finalPersona.Nature == null || n.Id != finalPersona.Nature.Id)
                    .ToList() ?? new System.Collections.Generic.List<Nature>();

                if (availableDemeanors.Any())
                {
                    var chosen = _affinityProcessor.GetWeightedRandom(availableDemeanors, runningAffinities);
                    if (chosen != null)
                    {
                        finalPersona.Demeanor = chosen;
                        var a = _affinityProcessor.BuildAffinityProfile(new Persona { Demeanor = chosen });
                        MergeInto(runningAffinities, a);
                    }
                }
                else
                {
                    finalPersona.Demeanor = null;
                }
            }

            return finalPersona;
        }

        // small helper to avoid repeated null/empty checks
        private static bool _data_providerAvailable<T>(System.Collections.Generic.List<T> list)
            => list != null && list.Any();
    }
}