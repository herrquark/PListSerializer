using System.Globalization;
using BenchmarkDotNet.Attributes;

namespace PlistSerializer.Performance;

[MemoryDiagnoser]
public class PlistBenchmarks
{
    private BenchmarkCase _benchmark;

    [ParamsSource(nameof(Cases))]
    public string Case { get; set; }

    public IEnumerable<string> Cases => BenchmarkCatalog.Create().Select(benchmark => benchmark.Name);

    [GlobalSetup]
    public void Setup()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        _benchmark = BenchmarkCatalog.Create().First(benchmark => benchmark.Name == Case);
        _benchmark.Validate(_benchmark.Run());
    }

    [Benchmark]
    public object Execute() => _benchmark.Run();
}
