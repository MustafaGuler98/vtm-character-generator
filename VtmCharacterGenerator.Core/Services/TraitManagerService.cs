using System.Collections.Generic;
using VtmCharacterGenerator.Core.Enums;
using VtmCharacterGenerator.Core.Models;
using VtmCharacterGenerator.Core.Services.Strategies;

namespace VtmCharacterGenerator.Core.Services
{
    public class TraitManagerService
    {
        public bool TryIncreaseTrait(Character character, TraitType type, string traitId, ITraitCostStrategy costStrategy, ref int currentPoints)
        {
            int cost = costStrategy.GetCost(type);

            if (currentPoints < cost)
            {
                return false;
            }

            bool success = false;

            switch (type)
            {
                // For Freebie points, up to 5 or 10 (Humanity/Willpower)
                case TraitType.Attribute:
                    success = TryIncreaseDictionaryTrait(character.Attributes, traitId, 5);
                    break;

                case TraitType.Ability:
                    
                    success = TryIncreaseDictionaryTrait(character.Abilities, traitId, 5);
                    break;

                case TraitType.Discipline:
                    success = TryIncreaseDictionaryTrait(character.Disciplines, traitId, 5);
                    break;

                case TraitType.Background:
                    success = TryIncreaseDictionaryTrait(character.Backgrounds, traitId, 5);
                    break;

                case TraitType.Virtue:
                    success = TryIncreaseDictionaryTrait(character.Virtues, traitId, 5);
                    break;

                case TraitType.Humanity:
                    if (character.Humanity < 10)
                    {
                        character.Humanity++;
                        success = true;
                    }
                    break;

                case TraitType.Willpower:
                    if (character.Willpower < 10)
                    {
                        character.Willpower++;
                        success = true;
                    }
                    break;

                default:
                    // Default case: unsupported trait type
                    success = false;
                    break;
            }

            if (success)
            {
                currentPoints -= cost;
                character.DebugLog.Add($"[Buy] {type}: '{traitId}' (+1 dot) - Cost: {cost}");
            }

            return success;
        }

        public bool TryAddMerit(Character character, Merit merit, ref int currentPoints)
        {
            if (character.Merits.Exists(m => m.Id == merit.Id))
            {
                return false;
            }

            if (currentPoints < merit.Cost)
            {
                return false;
            }

            character.Merits.Add(merit);
            currentPoints -= merit.Cost;
            character.DebugLog.Add($"[Merit] Added '{merit.Name}' - Cost: {merit.Cost}");

            return true;
        }

        public bool TryAddFlaw(Character character, Flaw flaw, ref int currentPoints)
        {
            if (character.Flaws.Exists(f => f.Id == flaw.Id))
            {
                return false;
            }

            // We do not check for cost limits here because flaws GIVE points.
            character.Flaws.Add(flaw);
            currentPoints += flaw.Cost;
            character.DebugLog.Add($"[Flaw] Added '{flaw.Name}' - Gained: {flaw.Cost}");

            return true;
        }

        // --- Helper Method ---

  
        private bool TryIncreaseDictionaryTrait(Dictionary<string, int> traits, string key, int maxLimit)
        {
            // Initialize if it doesn't exist (e.g., a new Discipline)
            if (!traits.ContainsKey(key))
            {
                traits[key] = 0;
            }

            // Check the cap (Rule: Cannot exceed Generation limit)
            if (traits[key] >= maxLimit)
            {
                return false;
            }

            traits[key]++;
            return true;
        }
    }
}