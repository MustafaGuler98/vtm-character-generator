using BenchmarkDotNet.Attributes;
using VtmCharacterGenerator.Core.Data;
using Microsoft.VSDiagnostics;

namespace VtmCharacterGenerator.Core.Benchmarks
{
    [CPUUsageDiagnoser]
    public class GameDataProviderBenchmarks
    {
        private string _dataPath;
        [GlobalSetup]
        public void Setup()
        {
            // Assuming GameData is in the project root
            _dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameData");
        }

        [Benchmark]
        public GameDataProvider LoadGameData()
        {
            return new GameDataProvider(_dataPath);
        }
    }
}