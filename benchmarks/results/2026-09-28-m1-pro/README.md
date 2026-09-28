# Consolidated plist benchmark results

2026-09-28: **115/115 benchmarks passed in 23m 49s**, including the generated build. Exit code 0.

Apple M1 Pro; macOS 27.0 ARM64; .NET 10.0.12; BenchmarkDotNet 0.15.8; Release; concurrent workstation GC. One launch, six warmup iterations, fifteen measured iterations targeting 500 ms. The operating system denied high priority, so measurements ran at normal priority.

The matrix was reduced from 426 timed cases to 115. Every model family retains small and large XML/binary round trips. The basic family also retains individual serialization, deserialization, save/load, and XML string timings. Boundary cases, imported conversions, and expected errors remain individually measured. All 426 operation-level correctness validators and all 180 unit tests passed.

Measurements use warmed metadata caches and MemoryStream, with setup and validation outside timing. Small means one model in an array; large means 1,000 models in an array. A round trip serializes the models, writes bytes, reads the bytes, and deserializes the resulting nodes. Every table reports one complete operation, not one model inside a large batch.

Binary had lower mean latency in nine of ten small model families and allocated less in all ten. XML was faster for the small string-heavy payload family. For 1,000-model batches, XML had lower mean latency for collections, shared/nested graphs, and resolvers; binary had lower mean latency for the other seven families. Format performance therefore depends on the model.

## One model

| Family | Binary mean, µs | Error | Binary allocation, KiB | XML mean, µs | Error | XML allocation, KiB |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| basic | 3.114 | ±1.7% | 6.62 | 4.315 | ±1.0% | 12.49 |
| collections | 19.517 | ±2.4% | 31.25 | 22.678 | ±2.5% | 36.29 |
| members | 4.067 | ±0.9% | 8.27 | 5.394 | ±0.5% | 14.08 |
| nested-shared | 8.222 | ±0.5% | 16.49 | 11.495 | ±1.4% | 24.38 |
| nodes | 4.663 | ±1.4% | 10.76 | 7.316 | ±13.3% | 16.28 |
| nullable | 11.542 | ±0.9% | 18.11 | 14.486 | ±0.9% | 24.02 |
| payloads | 17.939 | ±1.3% | 26.91 | 13.440 | ±1.3% | 47.89 |
| resolvers | 10.393 | ±1.7% | 19.55 | 13.365 | ±2.3% | 26.47 |
| scalars | 11.321 | ±1.7% | 19.57 | 13.883 | ±0.6% | 24.96 |
| untyped | 5.070 | ±0.8% | 11.09 | 6.727 | ±0.4% | 16.10 |

## 1,000 models

| Family | Binary mean, ms | Error | Binary allocation, MiB | XML mean, ms | Error | XML allocation, MiB |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| basic | 2.838 | ±1.7% | 3.79 | 3.367 | ±2.2% | 3.78 |
| collections | 37.478 | ±8.0% | 25.42 | 33.929 | ±5.4% | 23.65 |
| members | 3.977 | ±5.9% | 5.04 | 5.272 | ±4.0% | 4.97 |
| nested-shared | 15.289 | ±2.0% | 14.84 | 14.059 | ±0.5% | 12.97 |
| nodes | 5.002 | ±2.1% | 6.71 | 7.332 | ±1.9% | 7.38 |
| nullable | 14.014 | ±10.3% | 12.28 | 16.483 | ±1.7% | 12.06 |
| payloads | 14.147 | ±1.2% | 13.55 | 15.876 | ±1.5% | 19.53 |
| resolvers | 19.519 | ±1.8% | 17.34 | 16.544 | ±1.7% | 15.89 |
| scalars | 12.861 | ±0.9% | 13.33 | 16.969 | ±1.7% | 14.12 |
| untyped | 6.612 | ±1.3% | 7.23 | 6.995 | ±1.0% | 6.61 |

## Selected boundaries

| Case | Mean, µs | Error | Allocation, KiB |
| --- | ---: | ---: | ---: |
| large-data/1MiB/Binary/roundtrip | 262.149 | ±2.5% | 4099.92 |
| large-data/1MiB/Xml/roundtrip | 3534.978 | ±1.8% | 16095.69 |
| nesting/512/Binary/roundtrip | 265.959 | ±0.4% | 516.04 |
| nesting/512/Xml/roundtrip | 889.210 | ±2.8% | 1504.16 |
| reference-widths/32768/Binary/roundtrip | 23943.010 | ±2.7% | 22503.13 |
| reference-widths/32768/Xml/roundtrip | 22825.475 | ±2.7% | 18568.71 |

## Precision and memory limits

Error is the half-width of BenchmarkDotNet's 99.9% confidence interval, expressed as a percentage of the mean. The fixed sampling budget does not guarantee a particular precision. Three cases exceeded ±10%:

- `nodes/small/Xml/roundtrip`: ±13.3%.
- `nullable/large/Binary/roundtrip`: ±10.3%.
- `rejections/object-cycle`: ±14.3%.

No case exceeded ±20%. BenchmarkDotNet also flagged four cases with an observed iteration below 100 ms and two cases with multimodal distributions. Use a focused longer run before drawing conclusions about small differences or those noisy cases.

MemoryDiagnoser reports managed allocation and GC activity, not retained memory or proof of no leaks. The passing unit suite includes separate object-retention and collectible-type tests. These are current-state measurements, not a before/after speedup comparison; historical stopwatch results used different workloads and runtime settings.

The initial 250 ms / three-warmup / ten-measurement trial finished in 7m 42s but had wide confidence intervals in many cases. It was superseded by this run. The original 426-case default run was stopped at the user's request and is incomplete.

## Source identity

This run measured an uncommitted working tree based on `8b9112fad042d8e3de5ab364708851821f9c1552`, not that commit alone or a published package release. [Metadata](metadata.json) records source hashes, runtime, SDK, dependencies, commands, and validation. [source.patch](source.patch) captures the source and project changes against that commit, including new benchmark and retention-test files. Apply it only to a separate clean checkout of the base commit, outside the project folder. The archived source files match the measured run; documentation was updated afterward.

## Reproduce

```sh
dotnet run --project benchmarks/PlistSerializer.Performance.csproj -c Release -- --filter '*' --exporters JSON
dotnet run --project benchmarks/PlistSerializer.Performance.csproj -c Release -- --validate
dotnet test
```

[All 115 results](report.md) · [Measurements in fixed units](measurements.csv) · [Full BenchmarkDotNet JSON](benchmarkdotnet.json) · [Metadata and hashes](metadata.json)
