# PlistSerializer
.Net library for reading and writing Apple property lists in binary and XML format, and for mapping them to and from .Net objects.

```csharp
using PlistSerializer;

var node = Plist.Load(stream);
var settings = Deserializer.Deserialize<Settings>(node);
var xml = Plist.ToString(Serializer.Serialize(settings));
```

## Migrating to v2

Update your `PListSerializer.Quark` package reference to `PlistSerializer.Quark` version `2.0.0`. NuGet treats the old and new casing as the same package ID. Remove any direct `PListNet.Quark` reference, because its types are now included in `PlistSerializer.Quark`.

The resulting package reference is:

```xml
<PackageReference Include="PlistSerializer.Quark" Version="2.0.0" />
```

Update namespace imports and fully qualified names:

| Before v2 | v2 |
| --- | --- |
| `PListNet` | `PlistSerializer` |
| `PListSerializer.Core` | `PlistSerializer` |
| `PListNet.Nodes` | `PlistSerializer.Nodes` |
| `PListSerializer.Core.Attributes` | `PlistSerializer.Attributes` |
| `PList` | `Plist` |
| `PListFormat` | `PlistFormat` |
| `PListFormatException` | `PlistFormatException` |

`Serializer` and `Deserializer` are now static classes. Remove any instantiation or inheritance and call `Serializer.Serialize(value)` and `Deserializer.Deserialize<T>(node)` directly, as in the example above.

`LightXmlWriterExtensions`, `BinaryFormatWriter`, `XmlFormatReader` and `FillNode.GetSchema` are no longer public. Use `Plist.Load`, `Plist.Save` and `Plist.ToString` to read and write plists.

Check these behavior changes against your models and stored data:

- XML dates now deserialize as UTC, matching binary dates. XML output omits fractional seconds. `DateTimeOffset` serializes as a UTC date and deserializes with offset `+00:00`, so the original offset is not preserved.
- `TimeSpan` and `Uri` now serialize as strings instead of empty dictionaries. Scalar strings use the invariant culture. Check any existing values written with culture-specific formatting.
- XML UIDs use a `CF$UID` dictionary instead of the old escaped `<uid>` element. A dictionary with this shape loads as a `UidNode`.
- An `object` target now receives dictionaries as `Dictionary<string, object>`, arrays as `List<object>` and data as `byte[]`.
- Members that claim the same plist key now throw `PlistFormatException`. A member hidden with `new` gives way to the most derived member, and overrides inherit `[PlistName]` and `[DefaultValue]` unless they declare their own.
- Writing `NullNode` or `FillNode` to XML now throws `PlistFormatException`. Reading and serialization also reject nesting beyond 512 levels, and binary loading rejects excessive expansion of shared references.

See [CHANGELOG.md](CHANGELOG.md) for the complete v2 changes and format fixes.

## Acknowledgments
Based on the [PListSerializer](https://github.com/maksgithub/PListSerializer) project, with [PList-Net](https://github.com/PList-Net/PList-Net) merged in.

PList-Net was migrated from Google Code (https://code.google.com/p/iphone-plist-net/) and heavily refactored.

The original code was written by Christian Ecker, who relied on Pete Wilson's plutil.pl script for help with
understanding of the binary format of *.plist files.
