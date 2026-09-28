# Benchmark history

Keep completed, useful runs here so performance measurements survive cleanup of `BenchmarkDotNet.Artifacts/`. Each dated directory is an immutable baseline. Add a new directory for a later run instead of replacing previous measurements. These files are intended to be committed with the corresponding code.

| Baseline | Source | Machine / runtime | Suite | Elapsed |
| --- | --- | --- | ---: | ---: |
| [2026-09-28](2026-09-28-m1-pro/README.md) | Performance changes on top of `8b9112f`; source patch included | Apple M1 Pro ARM64 / .NET 10.0.12 | 115 cases, suite v1 | 23m 49s |

Each baseline contains:

- `README.md`: findings, representative results, validation, and limitations.
- `report.md`: BenchmarkDotNet's table for every case.
- `measurements.csv`: every case in fixed units, including timing uncertainty, allocation, and GC counts.
- `benchmarkdotnet.json`: the original full export, including raw measurements and statistics.
- `metadata.json`: source identity, environment, sampling settings, commands, resolved package versions, validation results, and SHA-256 hashes.
- `source.patch`: changes against the recorded base commit when the measured tree was uncommitted. A clean-tree baseline can identify its source by commit alone.

## Comparing runs

Match rows by the full `case` name. In `measurements.csv`, time columns use nanoseconds, allocation uses bytes per operation, and GC columns use collections per 1,000 operations. `error_99_9_percent_ns` is the half-width of the 99.9% confidence interval in nanoseconds; it is not a percentage of the mean. The retained measurement count can be smaller than the configured count because BenchmarkDotNet excludes outliers.

Calculate latency change as `100 * (new_mean_ns / baseline_mean_ns - 1)` and allocation change as `new_allocated_bytes_per_operation - baseline_allocated_bytes_per_operation`. Positive changes mean more time or allocation. Inspect confidence intervals and raw distributions before treating a small timing change as a regression; interval overlap alone is not a significance test. Rerun noisy or important cases with longer sampling when needed.

Compare the same workloads, sizes, formats, sampling settings, architecture, machine, runtime, SDK, GC, and culture. Library source changes are expected; benchmark workload changes must be called out. Increment `suite_version` when workload semantics or measurement settings change, and keep unchanged cases identifiable. Record added and removed cases separately. If changing the runtime or machine, measure the old and new library revisions on the same new environment before attributing differences to the code.

Small cases process one model; large cases process 1,000 models. Do not average unlike cases or compare a whole large batch directly with one small operation. Allocation and GC measurements do not establish whether objects are retained: run the separate memory tests too.

The historical stopwatch numbers in the parent README use different workloads and runtime settings. They are context, not a comparable BenchmarkDotNet baseline.

## Saving the next baseline

Run the complete suite into a new temporary artifact directory and request the full JSON export:

```sh
dotnet run --project benchmarks/PlistSerializer.Performance.csproj -c Release -- --filter '*' --exporters JSON --artifacts BenchmarkDotNet.Artifacts/candidate
dotnet run --project benchmarks/PlistSerializer.Performance.csproj -c Release -- --validate
dotnet test
```

After a successful run, copy its full JSON and Markdown report into a new dated directory here, export all cases to the same fixed-unit CSV schema, and record the source commit, dirty-tree changes if any, environment, settings, elapsed time, warnings, and actual validation results. Preserve the original JSON without editing it. Include source hashes and a patch for uncommitted code so the base commit is never mistaken for the measured revision. Add the new baseline to the table above and commit it with its source changes. Keep interrupted runs and exploratory short runs in the ignored artifacts directory.
