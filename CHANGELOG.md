## PlistSerializer.Quark

### 2.0.0
 - the package is renamed from `PListSerializer.Quark` to `PlistSerializer.Quark`, which NuGet treats as the same package id
 - PListNet is merged into the package, so remove any direct `PListNet.Quark` reference when upgrading, or its types are defined twice
 - all types live under `PlistSerializer`: `PListNet` and `PListSerializer.Core` become `PlistSerializer`, `PListNet.Nodes` becomes `PlistSerializer.Nodes`, and `PListSerializer.Core.Attributes` becomes `PlistSerializer.Attributes`
 - `PList`, `PListFormat` and `PListFormatException` are renamed to `Plist`, `PlistFormat` and `PlistFormatException`
 - `Serializer` and `Deserializer` are static classes
 - `LightXmlWriterExtensions`, `BinaryFormatWriter`, `XmlFormatReader` and `FillNode.GetSchema` are no longer public
 - binary UIDs above 255 are written and read correctly: they were corrupt on write, and Apple's 4-byte UIDs were misread
 - equal UIDs are written as separate binary objects, as Apple and Python write them
 - UIDs are written to XML as `CF$UID` dicts, which Apple reads, instead of an escaped `<uid>` element, and such dicts load as `UidNode`
 - the deserializer converts a `UidNode` to integer members, node-typed members receive the node itself, and the serializer writes node values as they are

## PListNet.Quark (merged into PlistSerializer.Quark 2.0.0)

### 3.4.4
 - use fast and simple [`LightXmlWriter`](https://github.com/lechu445/LightXmlWriter) for XML serialization
 - make sure that serialized XML is as close as possible to what Xcode generates

### 3.4.3
 - code cleanup and refactoring

### 3.4.2
 - added ability to serialize node to a string

### 3.4.1
 - added `AddRange` method to `ArrayNode`

### 3.4
 - removed EndianBitConverter dependency
 - code style change
 - netstandard2.1

### 3.2
 - fixed writing of boolean values (addressed issue #20)

### 3.1
 - fixed parsing of binary UID values

### 3.0
 - fixed a number of binary PList parsing bugs
 - fixed writing of unicode strings in XML format
 - converted to netstandard1.0

### 2.0.5
 - project cleanup (removed unused file)

### 2.0.4
 - fixed handling of empty arrays

### 2.0.3
 - fixed issue where invalid PList XML files were generated

### 2.0.2
 - switched NuGet package to Release bits

### 2.0.1
 - corrected bug in XML writer that resulted in written file missing the root "plist" element

### 1.x - 2.0
 - root namespace changed from `CE.iPhone.x` to `PListNet`
 - moved all nodes to the `PListNet.Nodes` namespace
 - renamed nodes from PListXXX to XXXNode (e.g. `PListArray` => `ArrayNode`)
 - dramatically reduced public API surface -- things meant to be internal now are
