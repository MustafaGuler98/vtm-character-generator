using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Models;
using VtmCharacterGenerator.Core.Services;

public static class AffinityDistributionTest
{
    private class TestCase
    {
        public string Name { get; set; }
        public string ClanId { get; set; }
        public string ConceptId { get; set; }
        public string NatureId { get; set; }
        public string ExpectedPrimaryCategory { get; set; }  // What we EXPECT to win most
        public double MinExpectedWinRate { get; set; }      // Minimum acceptable win rate (e.g., 0.50 = 50%)
        public string Description { get; set; }
    }

    public static void Run(int iterations = 10000)
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     COMPREHENSIVE AFFINITY VALIDATION TEST                         ║");
        Console.WriteLine("║     Testing 20 Character Combinations                              ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝\n");

        // Setup
        string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        DirectoryInfo dirInfo = new DirectoryInfo(exePath);
        while (dirInfo != null && !dirInfo.GetFiles("*.sln").Any()) dirInfo = dirInfo.Parent;
        string solutionRoot = dirInfo?.FullName ?? ".";
        string gameDataPath = Path.Combine(solutionRoot, "GameData");

        var dataProvider = new GameDataProvider(gameDataPath);
        var affinityProcessor = new AffinityProcessorService();
        var attributeService = new AttributeService(dataProvider, affinityProcessor);

        // Define 20 test cases with expected outcomes
        var testCases = new List<TestCase>
        {
            new TestCase
            {
                Name = "Pure Gangrel",
                ClanId = "gangrel",
                ConceptId = null,
                NatureId = null,
                ExpectedPrimaryCategory = "Physical",
                MinExpectedWinRate = 0.60,
                Description = "Gangrel alone should strongly favor Physical"
            },
            new TestCase
            {
                Name = "Gangrel + Soldier Concept",
                ClanId = "gangrel",
                ConceptId = "soldier",
                NatureId = null,
                ExpectedPrimaryCategory = "Physical",
                MinExpectedWinRate = 0.70,
                Description = "Both have Physical tags - should be very strong"
            },
            new TestCase
            {
                Name = "Gangrel + Soldier + Bravo Nature",
                ClanId = "gangrel",
                ConceptId = "soldier",
                NatureId = "bravo",
                ExpectedPrimaryCategory = "Physical",
                MinExpectedWinRate = 0.75,
                Description = "Triple Physical synergy - overwhelming"
            },
            new TestCase
            {
                Name = "Gangrel + Intellectual Concept",
                ClanId = "gangrel",
                ConceptId = "intellectual",
                NatureId = null,
                ExpectedPrimaryCategory = "Physical",
                MinExpectedWinRate = 0.45,
                Description = "Conflicting: Physical clan vs Mental concept"
            },
            new TestCase
            {
                Name = "Tremere (Pure Mental Clan)",
                ClanId = "tremere",
                ConceptId = null,
                NatureId = null,
                ExpectedPrimaryCategory = "Mental",
                MinExpectedWinRate = 0.60,
                Description = "Tremere should strongly favor Mental"
            },
            new TestCase
            {
                Name = "Tremere + Intellectual",
                ClanId = "tremere",
                ConceptId = "intellectual",
                NatureId = null,
                ExpectedPrimaryCategory = "Mental",
                MinExpectedWinRate = 0.70,
                Description = "Double Mental synergy"
            },
            new TestCase
            {
                Name = "Tremere + Intellectual + Scientist",
                ClanId = "tremere",
                ConceptId = "intellectual",
                NatureId = "scientist",
                ExpectedPrimaryCategory = "Mental",
                MinExpectedWinRate = 0.75,
                Description = "Triple Mental synergy - overwhelming"
            },
            new TestCase
            {
                Name = "Toreador (Social Clan)",
                ClanId = "toreador",
                ConceptId = null,
                NatureId = null,
                ExpectedPrimaryCategory = "Social",
                MinExpectedWinRate = 0.60,
                Description = "Toreador should favor Social"
            },
            new TestCase
            {
                Name = "Toreador + Artist",
                ClanId = "toreador",
                ConceptId = "artist",
                NatureId = null,
                ExpectedPrimaryCategory = "Social",
                MinExpectedWinRate = 0.70,
                Description = "Social + Social synergy"
            },
            new TestCase
            {
                Name = "Toreador + Artist + Gallant",
                ClanId = "toreador",
                ConceptId = "artist",
                NatureId = "gallant",
                ExpectedPrimaryCategory = "Social",
                MinExpectedWinRate = 0.75,
                Description = "Triple Social synergy"
            },
            new TestCase
            {
                Name = "Ventrue (Social/Mental Clan)",
                ClanId = "ventrue",
                ConceptId = null,
                NatureId = null,
                ExpectedPrimaryCategory = "Social",
                MinExpectedWinRate = 0.50,
                Description = "Ventrue has both Social and Mental - should lean Social"
            },
            new TestCase
            {
                Name = "Ventrue + Politician",
                ClanId = "ventrue",
                ConceptId = "politician",
                NatureId = null,
                ExpectedPrimaryCategory = "Social",
                MinExpectedWinRate = 0.65,
                Description = "Strong Social synergy"
            },
            new TestCase
            {
                Name = "Nosferatu (Physical/Mental)",
                ClanId = "nosferatu",
                ConceptId = null,
                NatureId = null,
                ExpectedPrimaryCategory = "Physical",
                MinExpectedWinRate = 0.45,
                Description = "Split between Physical and Mental"
            },
            new TestCase
            {
                Name = "Nosferatu + Hacker",
                ClanId = "nosferatu",
                ConceptId = "hacker",
                NatureId = null,
                ExpectedPrimaryCategory = "Mental",
                MinExpectedWinRate = 0.55,
                Description = "Hacker tips the balance to Mental"
            },
            new TestCase
            {
                Name = "Brujah (Physical/Social)",
                ClanId = "brujah",
                ConceptId = null,
                NatureId = null,
                ExpectedPrimaryCategory = "Physical",
                MinExpectedWinRate = 0.50,
                Description = "Brujah has Physical edge"
            },
            new TestCase
            {
                Name = "Brujah + Activist",
                ClanId = "brujah",
                ConceptId = "activist",
                NatureId = null,
                ExpectedPrimaryCategory = "Social",
                MinExpectedWinRate = 0.50,
                Description = "Activist should shift toward Social"
            },
            new TestCase
            {
                Name = "Brujah + Soldier",
                ClanId = "brujah",
                ConceptId = "soldier",
                NatureId = null,
                ExpectedPrimaryCategory = "Physical",
                MinExpectedWinRate = 0.65,
                Description = "Soldier reinforces Physical"
            },
            new TestCase
            {
                Name = "Assamite + Martial Artist",
                ClanId = "assamite",
                ConceptId = "martial_artist",
                NatureId = null,
                ExpectedPrimaryCategory = "Physical",
                MinExpectedWinRate = 0.70,
                Description = "Combat-focused synergy"
            },
            new TestCase
            {
                Name = "Giovanni (Balanced Clan)",
                ClanId = "giovanni",
                ConceptId = null,
                NatureId = null,
                ExpectedPrimaryCategory = "Mental",
                MinExpectedWinRate = 0.45,
                Description = "Giovanni has all three - slight Mental edge"
            },
            new TestCase
            {
                Name = "Malkavian + Scientist",
                ClanId = "malkavian",
                ConceptId = "intellectual",
                NatureId = "scientist",
                ExpectedPrimaryCategory = "Mental",
                MinExpectedWinRate = 0.70,
                Description = "Mental-focused Malkavian"
            }
        };

        Console.WriteLine($"Testing {testCases.Count} character combinations with {iterations:N0} iterations each");
        Console.WriteLine($"Each test checks if affinities produce expected attribute distributions\n");
        Console.WriteLine("═══════════════════════════════════════════════════════════════════════\n");

        int passedTests = 0;
        int failedTests = 0;

        foreach (var testCase in testCases)
        {
            var result = RunSingleTest(testCase, dataProvider, affinityProcessor, attributeService, iterations);
            
            bool passed = result.WinRate >= testCase.MinExpectedWinRate;
            
            if (passed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("✓ PASS");
                passedTests++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("✗ FAIL");
                failedTests++;
            }
            Console.ResetColor();
            
            Console.WriteLine($" │ {testCase.Name}");
            Console.WriteLine($"       │ {testCase.Description}");
            Console.WriteLine($"       │ Expected: {testCase.ExpectedPrimaryCategory} ≥{testCase.MinExpectedWinRate:P0} " +
                            $"│ Got: {result.WinnerCategory} {result.WinRate:P1}");
            
            // Show breakdown
            Console.Write($"       │ Distribution: ");
            foreach (var cat in new[] { "Physical", "Mental", "Social" })
            {
                var pct = result.Results[cat] / (double)iterations;
                if (cat == result.WinnerCategory)
                {
                    Console.ForegroundColor = passed ? ConsoleColor.Green : ConsoleColor.Yellow;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                Console.Write($"{cat}:{pct:P0} ");
                Console.ResetColor();
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        // Summary
        Console.WriteLine("═══════════════════════════════════════════════════════════════════════");
        Console.WriteLine("\n📊 TEST SUMMARY:");
        Console.WriteLine("───────────────────────────────────────────────────────────────────────");
        
        double successRate = (double)passedTests / testCases.Count * 100;
        
        Console.ForegroundColor = successRate >= 80 ? ConsoleColor.Green : 
                                  successRate >= 60 ? ConsoleColor.Yellow : ConsoleColor.Red;
        Console.WriteLine($"Passed: {passedTests}/{testCases.Count} ({successRate:F1}%)");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Failed: {failedTests}/{testCases.Count}");
        Console.ResetColor();

        Console.WriteLine("\n💡 INTERPRETATION:");
        Console.WriteLine("───────────────────────────────────────────────────────────────────────");
        
        if (successRate >= 80)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ EXCELLENT: Your affinity system is working as designed!");
            Console.WriteLine("  Current base affinity values (100 for clan tags) are well-tuned.");
        }
        else if (successRate >= 60)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠ MODERATE: Affinity system is partially working");
            Console.WriteLine("  Some combinations aren't producing expected results.");
            Console.WriteLine("  Consider adjusting base affinity values or reviewing tag assignments.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ POOR: Affinity system needs significant tuning");
            Console.WriteLine("  Base affinity values may be too low (increase from 100 to 150-200)");
            Console.WriteLine("  OR the base score (50) in GetWeightedRandom may be too high (reduce to 10-25)");
            Console.WriteLine("  OR tag assignments don't match attribute categories correctly.");
        }
        Console.ResetColor();

        Console.WriteLine("\n\n✅ Test complete! Press any key to exit...");
        Console.ReadKey();
    }

    private class TestResult
    {
        public Dictionary<string, int> Results { get; set; }
        public string WinnerCategory { get; set; }
        public double WinRate { get; set; }
    }

    private static TestResult RunSingleTest(
        TestCase testCase,
        GameDataProvider dataProvider,
        AffinityProcessorService affinityProcessor,
        AttributeService attributeService,
        int iterations)
    {
        var results = new Dictionary<string, int>
        {
            ["Physical"] = 0,
            ["Mental"] = 0,
            ["Social"] = 0,
            ["Tie"] = 0
        };

        // Build persona from test case
        var persona = new Persona();
        
        if (!string.IsNullOrEmpty(testCase.ClanId))
            persona.Clan = dataProvider.Clans?.FirstOrDefault(c => c.Id == testCase.ClanId);
        
        if (!string.IsNullOrEmpty(testCase.ConceptId))
            persona.Concept = dataProvider.Concepts?.FirstOrDefault(c => c.Id == testCase.ConceptId);
        
        if (!string.IsNullOrEmpty(testCase.NatureId))
            persona.Nature = dataProvider.Natures?.FirstOrDefault(n => n.Id == testCase.NatureId);

        var affinityProfile = affinityProcessor.BuildAffinityProfile(persona);

        for (int i = 0; i < iterations; i++)
        {
            var attributes = attributeService.DistributeAttributes(affinityProfile);

            var categoryTotals = new Dictionary<string, int>();
            foreach (var cat in dataProvider.AttributeCategories)
            {
                categoryTotals[cat.Name] = cat.Attributes.Sum(a => attributes.ContainsKey(a.Id) ? attributes[a.Id] : 0);
            }

            var maxTotal = categoryTotals.Values.Max();
            var winners = categoryTotals.Where(kv => kv.Value == maxTotal).Select(kv => kv.Key).ToList();

            if (winners.Count == 1)
                results[winners[0]]++;
            else
                results["Tie"]++;
        }

        var winner = results.Where(r => r.Key != "Tie").OrderByDescending(r => r.Value).First();
        
        return new TestResult
        {
            Results = results,
            WinnerCategory = winner.Key,
            WinRate = (double)winner.Value / iterations
        };
    }
}