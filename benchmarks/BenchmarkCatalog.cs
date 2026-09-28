namespace PlistSerializer.Performance;

internal static class BenchmarkCatalog
{
    public static IEnumerable<BenchmarkCase> Create()
        => AllCases().Where(benchmark =>
            benchmark.Name.StartsWith("basic/", StringComparison.Ordinal) ||
            benchmark.Name.EndsWith("/roundtrip", StringComparison.Ordinal) ||
            benchmark.Name.EndsWith("/load-deserialize", StringComparison.Ordinal) ||
            benchmark.Name.StartsWith("rejections/", StringComparison.Ordinal));

    private static IEnumerable<BenchmarkCase> AllCases()
        => ModelBenchmarks.Create().Concat(EdgeBenchmarks.Create());

    public static void ValidateAll()
    {
        var count = 0;
        foreach (var benchmark in AllCases())
        {
            try
            {
                benchmark.Validate(benchmark.Run());
            }
            catch (Exception error)
            {
                throw new InvalidOperationException($"Validation failed: {benchmark.Name}", error);
            }

            Console.WriteLine($"PASS {benchmark.Name}");
            count++;
        }

        Console.WriteLine($"{count} cases validated.");
    }
}
