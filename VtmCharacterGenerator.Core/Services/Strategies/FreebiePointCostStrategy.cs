using VtmCharacterGenerator.Core.Enums;

namespace VtmCharacterGenerator.Core.Services.Strategies
{
    public class FreebiePointCostStrategy : ITraitCostStrategy
    {
        public int GetCost(TraitType type, int currentRating = 0, bool isClanTrait = false)
        {
            return type switch
            {
                TraitType.Attribute => 5,
                TraitType.Ability => 2,
                TraitType.Discipline => 7,
                TraitType.Background => 1,
                TraitType.Virtue => 2,
                TraitType.Humanity => 2,
                TraitType.Willpower => 1,

                TraitType.Merit => 0,
                TraitType.Flaw => 0,  

                _ => 0
            };
        }
    }
}