# PlistSerializer
.Net library for reading and writing Apple property lists in binary and XML format, and for mapping them to and from .Net objects.

```csharp
using PlistSerializer;

var node = Plist.Load(stream);
var settings = Deserializer.Deserialize<Settings>(node);
var xml = Plist.ToString(Serializer.Serialize(settings));
```

## Acknowledgments
Based on the [PListSerializer](https://github.com/maksgithub/PListSerializer) project, with [PList-Net](https://github.com/PList-Net/PList-Net) merged in.

PList-Net was migrated from Google Code (https://code.google.com/p/iphone-plist-net/) and heavily refactored.

The original code was written by Christian Ecker, who relied on Pete Wilson's plutil.pl script for help with
understanding of the binary format of *.plist files.
