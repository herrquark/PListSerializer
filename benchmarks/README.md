# Plist performance checks

The solution includes a BenchmarkDotNet executable with `MemoryDiagnoser`. Each named case reports timing, allocated bytes per operation, and GC collections. Setup creates the input and checks the expected result outside the timed method. The timed method runs the library operation and returns its result to BenchmarkDotNet. Stream workloads use `MemoryStream`, so disk latency is excluded.

Run from the repository root in Release mode:

```sh
# Frequent small interactions, both formats
dotnet run --project benchmarks/PlistSerializer.Performance.csproj -c Release -- --filter '*small*roundtrip*'

# All model sizes, formats, individual operations, and edge cases
dotnet run --project benchmarks/PlistSerializer.Performance.csproj -c Release -- --filter '*'

# A shorter measurement of one feature family
dotnet run --project benchmarks/PlistSerializer.Performance.csproj -c Release -- --job Short --filter '*resolvers*'
```

The full timed suite has 115 cases, designed to finish within 30 minutes on the development machine. Every model and boundary workload keeps its complete round trip; the basic small and large models also keep individual operation timings. Imported conversions and expected rejections remain separately measured. This removes repeated stage measurements across similar workloads without combining unrelated cases into one average.

Verified on 2026-09-28: all 115 cases completed in **23m 49s**, including BenchmarkDotNet's generated build, on an Apple M1 Pro running macOS ARM64 and .NET 10.0.12. All 426 standalone validators and all 180 unit tests also passed. This is a measured runtime on that machine, not a timeout guarantee on slower or busier hosts.

The default `Suite` job uses one launch, six warmup iterations, and fifteen measured iterations targeting 500 ms each. BenchmarkDotNet still chooses the invocation count and measures allocations and GC. Fixed sampling bounds the time spent on noisy cases, at the cost of wider confidence intervals; inspect the reported error before interpreting small differences. Actual elapsed time depends on hardware and system load. Use a focused longer run when investigating a regression:

```sh
dotnet run --project benchmarks/PlistSerializer.Performance.csproj -c Release -- --filter '*basic/small*' --warmupCount 6 --iterationCount 30 --iterationTime 500
```

Filter by the slash-separated names to narrow a run. Results go to the ignored `BenchmarkDotNet.Artifacts/` directory. Use `--artifacts path` to choose another location. BenchmarkDotNet's [job documentation](https://benchmarkdotnet.org/articles/configs/jobs.html) explains sampling, and its [command-line documentation](https://benchmarkdotnet.org/articles/guides/console-args.html) describes overrides, filters, and exporters.

Completed reference runs are preserved in [benchmark history](results/README.md), outside the ignored artifacts directory. The [2026-09-28 baseline](results/2026-09-28-m1-pro/README.md) includes all 115 measurements, raw samples, source identity, and comparison guidance.

```sh
# List every case, including parameter values
dotnet run --project benchmarks/PlistSerializer.Performance.csproj -c Release -- --list-cases

# Check all 426 operation-level workloads and expected results without timing
dotnet run --project benchmarks/PlistSerializer.Performance.csproj -c Release -- --validate

# Check invariant conversions under another current culture
dotnet run --project benchmarks/PlistSerializer.Performance.csproj -c Release -- --validate --culture sv-SE

# Exercise BenchmarkDotNet's generated executable and setup for every case
dotnet run --project benchmarks/PlistSerializer.Performance.csproj -c Release -- --job Dry --filter '*'
```

`Dry` checks execution, not statistically useful performance. Normal measurements use warmed metadata caches and BenchmarkDotNet's normal runtime configuration. `--validate --culture` checks correctness under the chosen culture; the timed benchmarks set the culture to invariant.

## Coverage

The ten model families each use one model and 1,000 models. Both sizes measure complete XML and binary round trips. The `basic` family also measures serialization, deserialization, XML strings with and without metadata, and XML and binary save/load individually. All 426 original operation-level cases remain in `--validate`, including the stages no longer timed separately. Validation checks every model, not only the last entry. Edge cases use their own explicit sizes and formats.

| Family | Features and expected quirks |
| --- | --- |
| `basic` | Integer, string, boolean, array, and data payload |
| `scalars` | All ordinary signed/unsigned integer types, char, float, double, decimal, UTC/local dates, nonzero date offsets, TimeSpan, Guid, relative/absolute Uri, flags and unnamed enums; decimal conversion through double, XML fractional-second loss, UTC normalization |
| `nullable` | Present and missing nullable scalar values, nullable enums, null collection items and dictionary values, zero and false retained |
| `collections` | Arrays, List, HashSet, Dictionary, every supported collection interface, lazy IEnumerable, jagged arrays, nested dictionaries/lists, empty collections/data, dictionary input order |
| `members` | PlistName, DefaultValue, inherited attributes, overridden names, hidden properties/fields, public fields written but not read, indexers/get-only properties skipped, missing-member initializers, case-sensitive keys, sorted object keys |
| `resolvers` | Declared base types resolving to two derived types or falling back to the base, inside properties, arrays, lists, and dictionaries |
| `untyped` | Object members becoming dictionaries, lists, byte arrays, and scalar values; nested heterogeneous values and omitted nulls |
| `nodes` | PNode and every XML-compatible concrete node type; serializer/deserializer node identity, UID members, nested node collections |
| `nested-shared` | Recursive object properties/collections and shared source objects expanding into independent results |
| `payloads` | Empty, ASCII, non-ASCII, supplementary Unicode, XML escaping, repeated/unique strings, lengths around extended-count and stack-buffer boundaries, byte payloads |
| `legacy-dictionary`, `pair-enumerable`, `expando`, `legacy-list` | Non-generic IDictionary/IEnumerable, non-string keys converted to strings, lazy key/value pairs, ExpandoObject, omitted nulls |
| `imported-conversions` | Numeric strings, case-insensitive and numeric enums, nullable enums, UID-to-integer/object conversion, custom TypeConverter, mismatched node types, invalid nullable Guid/TimeSpan, unknown keys and fields ignored |
| `unsupported-collections` | Queue from an array becomes null; SortedDictionary from a dictionary stays empty |
| `integer-widths`, `real-special-values`, `uid-widths` | Signed/unsigned binary width boundaries, 64-bit extrema, zero, subnormal reals, NaN/infinities, UID widths |
| `binary-only-nodes`, `untyped-null` | NullNode, FillNode, ulong.MaxValue UID, and null untyped results, measured only in binary |
| `reference-widths` | Dictionaries of 14, 15, 127, 128, 255, 256, and 32,768 entries, crossing extended lengths and reference/offset widths |
| `nesting`, `large-data`, `root-string`, `root-integer` | Depths 16/128/512, 1 MiB data, and scalar roots |
| `rejections` | Conflicting plist names, non-string dictionary target keys, mismatched value types, invalid enums, integer overflow, null roots, XML null/fill, object/collection cycles, and excessive XML/binary nesting |

The expected lossy or unsupported behavior is explicit in the validators. A changed exception type or an unexpected default/null fails validation instead of becoming a successful timing result. These cases cover the documented model features and selected boundary/error paths; they are not an exhaustive malformed-file or arbitrary custom-type test suite.

MemoryDiagnoser measures allocation, not object retention. `PlistMemoryTests` separately checks collectible model types, source/result objects, node identity lifetimes, stream buffers, and large payloads. `PlistConcurrencyTests` checks concurrent first use of metadata caches and repeated XML/binary round trips. Run `dotnet test` for the full compatibility gate, and repeat the timing-sensitive cache checks:

```sh
for run in 1 2 3; do
  dotnet test --filter 'FullyQualifiedName~PlistMemoryTests|FullyQualifiedName~PlistConcurrencyTests' || exit
done
```

## Historical comparison before BenchmarkDotNet

The table below records the original simple-model stopwatch comparison on macOS ARM64 with .NET 10.0.12 against commit `8b9112f`. It used five batches, 10,000 small operations or 30 large operations per batch, and disabled tiered compilation. These are historical results for the original workload, not measurements of the expanded BenchmarkDotNet suite. One large operation processed 1,000 models. Use new BenchmarkDotNet reports to compare revisions with the expanded coverage.

| Operation | Before, µs | After, µs | Before, bytes | After, bytes |
| --- | ---: | ---: | ---: | ---: |
| small/serialize | 1.34 | 0.86 | 1,616 | 1,120 |
| small/deserialize | 8.38 | 1.67 | 4,072 | 544 |
| small/to-string | 1.77 | 1.22 | 13,336 | 2,904 |
| small/Xml/save | 1.66 | 1.38 | 11,760 | 4,624 |
| small/Xml/load | 6.42 | 5.79 | 6,833 | 6,728 |
| small/Xml/roundtrip | 19.74 | 10.74 | 24,223 | 12,952 |
| small/Binary/save | 2.73 | 2.19 | 4,608 | 2,992 |
| small/Binary/load | 2.46 | 1.25 | 3,784 | 2,360 |
| small/Binary/roundtrip | 15.98 | 6.47 | 14,044 | 6,976 |
| large/serialize | 1,092.39 | 758.82 | 1,272,873 | 984,665 |
| large/deserialize | 7,944.50 | 1,420.26 | 3,785,182 | 376,712 |
| large/to-string | 1,347.17 | 1,195.99 | 2,711,930 | 1,494,913 |
| large/Xml/save | 1,174.42 | 1,024.12 | 1,727,786 | 1,225,887 |
| large/Xml/load | 4,100.97 | 3,711.47 | 1,389,090 | 1,356,705 |
| large/Xml/roundtrip | 15,552.05 | 7,135.41 | 8,201,361 | 3,967,720 |
| large/Binary/save | 2,113.18 | 1,324.74 | 2,063,809 | 1,135,353 |
| large/Binary/load | 2,537.29 | 1,201.37 | 2,787,271 | 1,478,649 |
| large/Binary/roundtrip | 15,181.66 | 4,976.52 | 9,958,849 | 4,023,415 |

The changes cache deserialization property maps and resolver lookups with weak type keys, remove temporary collection-check arrays, and avoid repeated member-name reflection. XML strings are written directly to a text writer. XML streams use a smaller buffer and reuse fixed indentation text. Binary I/O avoids per-reference, per-marker, and small string/integer byte arrays, and merges repeated booleans. No operation buffers or caller payloads are added to static pools.
