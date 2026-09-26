# AGENTS.md

PlistSerializer reads and writes Apple property lists and maps them to .NET objects, and ships as the NuGet package `PlistSerializer.Quark`. Everything lives in the one project `PlistSerializer.Core`, whose folders match its namespaces:

- `Plist` (`Load`, `Save`, `ToString`) reads and writes plist bytes in XML and binary format as a tree of `PNode`s, whose concrete types live in `Nodes/`. `Internal/` holds the binary reader and writer and the node factory. This layer was merged in from the maintainer's PList-Net fork.
- `Serializer.Serialize(object) → PNode` and `Deserializer.Deserialize<T>(PNode)` map objects to and from that tree by reflection, steered by the `[PlistName]` and `[PlistTypeResolver]` attributes in `Attributes/` and by `IPlistTypeResolver`.
- `Extensions/` holds the internal helpers of both layers.

## Build and test

- `dotnet test` from the repo root builds both projects and runs the suite. It is the only gate, because the GitHub workflow is a placeholder that builds nothing.
- To narrow a run, pass Microsoft Testing Platform flags after `--`: `dotnet test -- --filter-class "*PlistConcurrencyTests"` or `-- --filter-method "*Nullable_Test"`. The VSTest-style `dotnet test --filter` is silently ignored here and runs every test.
- Every build packs `PlistSerializer.Core/bin/<Configuration>/PlistSerializer.Quark.<version>.nupkg` (`GeneratePackageOnBuild`). That file is expected output.

## Conventions

- "Plist" is one word in every name (`Plist`, `PlistFormat`, `IsPlistMember`), because Apple never abbreviates "property list" as "PList". `PNode` is the one established exception.
- Public API members carry XML docs; internal members and private helpers do not.
- Private `static readonly` fields are PascalCase, private instance fields are `_camelCase`.

## Serializer and Deserializer are asymmetric

The two classes were written separately and share only the reflection helpers in `Extensions/` (`TypeExtensions`, `PropertyInfoExtensions`). A change to one side leaves the other untouched, and a model that deserializes correctly may still fail to round-trip.

| | `Serializer` | `Deserializer` |
|---|---|---|
| Members | public properties with a setter, and public fields | public properties with a setter; fields are never set |
| Omitted | nulls (members, dictionary values, collection items) and values equal to `[DefaultValue]`; `0` and `false` are still written | a missing key leaves the member at its initial value |
| Collections | any `IDictionary`, `IEnumerable<KeyValuePair<string, object>>` or `IEnumerable` | `T[]`, `List<T>`, `HashSet<T>`, `Dictionary<string, T>` and the interfaces listed in `TypeExtensions`, which get the concrete type; any other collection type comes back `null` from an array node and empty from a dict node, and a non-string dictionary key throws |
| Wrong input | — | inconsistent by type: a mismatched node or unparseable value yields `null`, a default or an exception, and a top-level value type throws `NullReferenceException`; test the exact case you depend on |

Both sides key a member by its `[PlistName]`, else its C# name, through `GetName`, and skip indexers and get-only properties through `IsPlistMember` (`Extensions/PropertyInfoExtensions.cs`). The deserializer skips unknown keys. Object members are written sorted case-insensitively by key, while dictionaries keep their input order.

### Adding type support

Each side dispatches in one `switch` expression: `Serializer.Serialize` and `Deserializer.Deserialize(Type, PNode)`. Add an arm there, and place it carefully because the first match wins. `string` and `byte[]` are both `IEnumerable`, so their serializer arms come before the enumerable arm. In the deserializer, `IsArray` comes before the scalar arms so that `byte[]` takes `<data>`. Scalars without their own serializer arm (`short`, `uint`, `char`, and so on) are written as `<string>` and parsed back by `ConvertToType`, which unwraps `Nullable<T>`, handles `TimeSpan`/`Uri`/`Guid` specially, then tries `TypeConverter` and finally `Convert.ChangeType`.

### Type resolvers

`[PlistTypeResolver(typeof(R))]` on a class makes the deserializer call `R.ResolveType(node)` whenever that class is the declared type, and `null` falls back to the declared type. The serializer has no counterpart; it writes the runtime type's members. Only one `R` instance is created per decorated class and it is shared across threads, so resolvers stay stateless.

## Thread safety

Both entry points are static and cache reflection results in static `ConcurrentDictionary` instances (`Serializer.MembersCache`, `TypeExtensions.ResolverCache`). Populate a cache through `GetOrAdd` or the indexer, which tolerate two threads adding the same key. A check-then-`Add` throws on that collision and has already caused one race. Every new cache gets a case in `PlistConcurrencyTests`, which closes a generic holder over every exported BCL class so that each call meets an uncached type, with a `Barrier` releasing all threads into each round together. That test is timing-based, so a single green run is weak evidence. Repeat it a few times.

## Language and targets

- `PlistSerializer.Core` targets `netstandard2.1` with `LangVersion latest`. Current C# syntax compiles, but BCL APIs newer than netstandard2.1 are unavailable, as are features that need runtime polyfills (`init`, `required`). The test project targets `net10.0`.
- Nullable reference types are disabled in both projects, so reference types are declared without `?`.
- The package version is `<Version>` in `PlistSerializer.Core/PlistSerializer.Core.csproj`, which also sets the assembly version. Version bumps go in their own `bump version` commit after the change. `CHANGELOG.md` is packed as the package's release notes, so each new version gets an entry at the top of its `PlistSerializer.Quark` section.

## Tests

- The suite uses xUnit v3 on Microsoft Testing Platform, with `Xunit` as a global using. Tests are named `<Operation>_<Subject>_Test` and group related asserts in `Assert.Multiple`.
- Models live in `TestModels/` under the namespace `PlistSerializer.Core.Tests.TestModels`.
- Small plists go inline as a raw string literal loaded with `Plist.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)))`, as `Deserialize_WithResolver_Test` does. Larger fixtures go in `Resources/`, which the test csproj copies to the output by glob, and a test opens one with `File.OpenRead(Path.Combine("Resources", "Name.plist"))`, spelling the file name with its exact casing, because Linux file systems are case-sensitive.
- `ToString_SourceXml_Test` compares written XML with fixtures byte for byte, so fixtures keep their committed whitespace and line endings, which `.gitattributes` shields from git's conversion.
