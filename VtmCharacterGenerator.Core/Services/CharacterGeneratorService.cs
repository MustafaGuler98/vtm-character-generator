using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class CharacterGeneratorService
    {
        private readonly GameDataProvider _dataProvider;
        private readonly Random _random = new Random();

        public CharacterGeneratorService(GameDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public Character GenerateCharacter()
        {
            var character = new Character();

          
            int clanIndex = _random.Next(0, _dataProvider.Clans.Count);
            character.Clan = _dataProvider.Clans[clanIndex];

            // TODO: Generate other character parts like attributes, abilities etc.

            return character;
        }
    }
}