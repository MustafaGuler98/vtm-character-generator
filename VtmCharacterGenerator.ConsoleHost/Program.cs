using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection; // Required for Assembly class
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Models;
using VtmCharacterGenerator.Core.Services;

// --- Test Setup ---
Console.WriteLine("--- Elysium Project: Persona Generation Test ---");

// A more robust way to find the project root directory
static string GetProjectRoot()
{
    string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    DirectoryInfo dirInfo = new DirectoryInfo(exePath);
    while (dirInfo != null && !dirInfo.GetFiles("*.sln").Any())
    {
        dirInfo = dirInfo.Parent;
    }
    return dirInfo.FullName;
}



string solutionRoot = GetProjectRoot();
string gameDataPath = Path.Combine(solutionRoot, "GameData");

Console.WriteLine($"[DEBUG] Found solution root at: {solutionRoot}");
Console.WriteLine($"[DEBUG] Attempting to load data from: {gameDataPath}");

try
{
    var dataProvider = new GameDataProvider(gameDataPath);
    var affinityProcessor = new AffinityProcessorService();
    var personaService = new PersonaService(dataProvider, affinityProcessor);

    // We will change here when we use specific input.
    var inputPersona = new Persona();

    Console.WriteLine("\n[STARTING] Generating a new random persona...");

 
    Persona finalPersona = personaService.CompletePersona(inputPersona);

    
    Console.WriteLine("\n--- FINAL PERSONA GENERATED ---");
    Console.WriteLine($"Concept: {finalPersona.Concept.Name}");
    Console.WriteLine($"Clan: {finalPersona.Clan.Name}");
    Console.WriteLine($"Nature: {finalPersona.Nature.Name}");
    Console.WriteLine($"Demeanor: {finalPersona.Demeanor.Name}");

    // 5. Build and display the final Affinity Profile for inspection
    var finalProfile = affinityProcessor.BuildAffinityProfile(finalPersona);
    Console.WriteLine("\n--- FINAL AFFINITY PROFILE ---");
    PrintAffinityProfile(finalProfile);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[FATAL ERROR] An error occurred: {ex.Message}");
    Console.ResetColor();
    Console.WriteLine("Possible cause: Make sure your GameData folder and JSON files exist at the solution level.");
}

AffinityDistributionTest.Run(iterations: 10000);

Console.WriteLine("\nTest complete. Press any key to exit.");
Console.ReadKey();


// --- Helper function to print the dictionary nicely ---
static void PrintAffinityProfile(Dictionary<string, int> profile)
{
    if (profile == null || !profile.Any())
    {
        Console.WriteLine("Profile is empty.");
        return;
    }

    foreach (var entry in profile.OrderByDescending(kvp => Math.Abs(kvp.Value)))
    {
        Console.ForegroundColor = entry.Value > 0 ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"{entry.Key}: {entry.Value:+#;-#;0}");
    }
    Console.ResetColor();
}