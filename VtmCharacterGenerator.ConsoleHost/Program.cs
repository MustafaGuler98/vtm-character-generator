using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Services; 
using System;
using System.IO;

Console.WriteLine("VTM Character Generator - Service Test");
Console.WriteLine("--------------------------------------");

try
{

    string executionPath = AppDomain.CurrentDomain.BaseDirectory;
    var dataProvider = new GameDataProvider(executionPath);
    Console.WriteLine($"[INFO] Data loaded successfully. {dataProvider.Clans.Count} clans available.");


    var generator = new CharacterGeneratorService(dataProvider);

 
    var newCharacter = generator.GenerateCharacter();


    Console.WriteLine("\n[SUCCESS] New character generated!");
    Console.WriteLine($"Clan: {newCharacter.Clan.Name} ({newCharacter.Clan.Nickname})");
    Console.WriteLine($"Clan Weakness: {newCharacter.Clan.Weakness}");
    
    
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[FATAL ERROR] An unexpected error occurred: {ex.Message}");
    Console.ResetColor();
}

Console.WriteLine("\nTest complete. Press any key to exit.");
Console.ReadKey();