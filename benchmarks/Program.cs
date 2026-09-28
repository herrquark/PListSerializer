using System.Globalization;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;
using PlistSerializer.Performance;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

if (args.SequenceEqual(["--list-cases"]))
{
    foreach (var benchmark in BenchmarkCatalog.Create())
        Console.WriteLine(benchmark.Name);

    return 0;
}

if (args.Length > 0 && args[0] == "--validate")
{
    if (args.Length == 3 && args[1] == "--culture")
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(args[2]);
    else if (args.Length != 1)
        throw new ArgumentException("Usage: --validate [--culture name]");

    BenchmarkCatalog.ValidateAll();
    return 0;
}

var job = Job.Default
    .WithId("Suite")
    .WithLaunchCount(1)
    .WithWarmupCount(6)
    .WithIterationCount(15)
    .WithIterationTime(TimeInterval.FromMilliseconds(500))
    .AsDefault();
var config = DefaultConfig.Instance
    .AddJob(job)
    .WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(80));
var summaries = BenchmarkSwitcher.FromAssembly(typeof(PlistBenchmarks).Assembly).Run(args, config);

return summaries.Any(summary => summary.HasCriticalValidationErrors || summary.Reports.Any(report => !report.Success))
    ? 1
    : 0;
