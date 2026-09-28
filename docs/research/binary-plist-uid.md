# UID in Apple property lists (`CF$UID`, NSKeyedArchiver object references)

Researched 2026-09-28. Primary sources only, pinned to these revisions:

| Tag | Source | Revision |
|---|---|---|
| SCF | [swiftlang/swift-corelibs-foundation](https://github.com/swiftlang/swift-corelibs-foundation) (`apple/swift-corelibs-foundation` redirects here), `Sources/CoreFoundation` | `main` @ [`6f21ccf`](https://github.com/swiftlang/swift-corelibs-foundation/tree/6f21ccf1461d160ffb27eb946e515d71269d8c1e) (2026-09-24) |
| CF | [apple-oss-distributions/CF](https://github.com/apple-oss-distributions/CF), the newest tag published | `CF-1153.18` @ [`dc54c6b`](https://github.com/apple-oss-distributions/CF/tree/dc54c6bb1c1e5e0b9486c1d26dd5bef110b20bf3) |
| PY | [python/cpython](https://github.com/python/cpython) `Lib/plistlib.py`, `Doc/library/plistlib.rst` | `main` @ [`7f7ff4c`](https://github.com/python/cpython/tree/7f7ff4c99d1f53424be8c323c50f3623c3488ba1) (2026-09-18); UID code identical to the local Python 3.13.5 used below |
| EXP | Experiments run for this note: Apple `plutil` on macOS 27.0 (build 26A428), Python 3.13.5 `plistlib`, and this repo at `d8cf93f` built outside the repo | hand-built bplist bytes, see each section |

Apple's shipping Foundation (including `NSKeyedArchiver`) is closed source. Everything said about current Apple behaviour beyond the CF source comes from the `plutil` experiments and is marked **(EXP)**.

## Summary

- Binary marker is `1000 nnnn` (`0x80`). The low nibble is **byte count minus one**, not a power of two as for integers. Payload is an unsigned big-endian integer. Legal on read: 1 to 16 bytes (`0x80`–`0x8F`, where `0x8F` means 16 bytes, not an extended length).
- Writers emit only the power-of-two widths: `0x80` (1 byte), `0x81` (2), `0x83` (4), and Python also `0x87` (8). `0x82` (3 bytes) is readable but never written.
- Maximum value: CF stores UIDs as `uint32_t`, rejects the whole binary plist if a UID exceeds `UINT32_MAX`, and never writes more than 4 bytes. Python accepts and writes `0 <= v < 2**64`. **The sources disagree here.**
- XML has no UID element (the DTD has none, and CF rejects `<uid>`). A UID is written as `<dict><key>CF$UID</key><integer>N</integer></dict>`. CF converts such a dict back to a UID on load when it has exactly one entry, the key `CF$UID` and a number value. Python writes no UID to XML (raises `TypeError`) and never converts the dict back.
- Neither CF nor Python uniques UIDs by value in the binary object table. CF uniques only strings, numbers, dates and data. Python uniques UIDs by object identity only.

## 1. Binary format

**Marker and nibble.** The format comment gives `nnnn+1` for UIDs where it gives `2^nnn` for integers:

> ```
> int	0001 0nnn	...		// # of bytes is 2^nnn, big-endian bytes
> uid	1000 nnnn	...		// nnnn+1 is # of bytes
> ```
> SCF [`CFBinaryPList.c#L229-L236`](https://github.com/swiftlang/swift-corelibs-foundation/blob/6f21ccf1461d160ffb27eb946e515d71269d8c1e/Sources/CoreFoundation/CFBinaryPList.c#L229-L236), same text in CF-1153.18 [`CFBinaryPList.c#L255-L262`](https://github.com/apple-oss-distributions/CF/blob/dc54c6bb1c1e5e0b9486c1d26dd5bef110b20bf3/CFBinaryPList.c#L255-L262)

Unlike `null`, `url`, `uuid`, `ordset`, `set` and the UTF-8 string, the UID line carries no `[v"1?"+ only]` tag, so it is valid in `bplist00`. The marker constant is `kCFBinaryPlistMarkerUID = 0x80` (SCF [`include/ForFoundationOnly.h#L443`](https://github.com/swiftlang/swift-corelibs-foundation/blob/6f21ccf1461d160ffb27eb946e515d71269d8c1e/Sources/CoreFoundation/include/ForFoundationOnly.h#L443)).

**CF writer** picks the smallest of 1, 2, 4 or 8 bytes and encodes `nbytes - 1`:

> ```c
> uint64_t bigint = _CFKeyedArchiverUIDGetValue(uid);
> if (bigint <= (uint64_t)0xff) { nbytes = 1; }
> else if (bigint <= (uint64_t)0xffff) { nbytes = 2; }
> else if (bigint <= (uint64_t)0xffffffff) { nbytes = 4; }
> else { nbytes = 8; }
> marker = kCFBinaryPlistMarkerUID | (uint8_t)(nbytes - 1);
> bigint = CFSwapInt64HostToBig(bigint);
> ```
> SCF [`CFBinaryPList.c#L283-L301`](https://github.com/swiftlang/swift-corelibs-foundation/blob/6f21ccf1461d160ffb27eb946e515d71269d8c1e/Sources/CoreFoundation/CFBinaryPList.c#L283-L301) (condensed); identical logic in CF-1153.18 [`#L352-L371`](https://github.com/apple-oss-distributions/CF/blob/dc54c6bb1c1e5e0b9486c1d26dd5bef110b20bf3/CFBinaryPList.c#L352-L371)

So CF emits `0x80`, `0x81` and `0x83`. The 8-byte branch (`0x87`) cannot be reached, because `_CFKeyedArchiverUIDGetValue` returns `uint32_t` (section 4). **(EXP)** `plutil -convert binary1` wrote `0x80 05`, `0x81 0100`, `0x83 00010000` and `0x83 ffffffff`, and re-compacted a 16-byte `0x8F` UID of value 9 to `0x80 09`.

**CF reader** accepts any nibble, reads `nibble + 1` bytes, and rejects values above 32 bits:

> ```c
> CFIndex cnt = (marker & 0x0f) + 1;
> ...
> // uids are not required to be in the most compact possible representation, but only the last 64 bits are significant currently
> uint64_t bigint = _getSizedInt(ptr, cnt);
> if (UINT32_MAX < bigint) FAIL_FALSE;
> ```
> SCF [`CFBinaryPList.c#L1453-L1478`](https://github.com/swiftlang/swift-corelibs-foundation/blob/6f21ccf1461d160ffb27eb946e515d71269d8c1e/Sources/CoreFoundation/CFBinaryPList.c#L1453-L1478); CF-1153.18 [`#L1280-L1297`](https://github.com/apple-oss-distributions/CF/blob/dc54c6bb1c1e5e0b9486c1d26dd5bef110b20bf3/CFBinaryPList.c#L1280-L1297)

`_getSizedInt` has fast paths for 1, 2, 4 and 8 bytes, and otherwise folds bytes big-endian into a `uint64_t` ("Compatibility with existing archives, including anything with a non-power-of-2 size and 16-byte values"). SCF [`#L707-L725`](https://github.com/swiftlang/swift-corelibs-foundation/blob/6f21ccf1461d160ffb27eb946e515d71269d8c1e/Sources/CoreFoundation/CFBinaryPList.c#L707-L725). The fold (`res = (res << 8) + data[idx]`) overflows silently, so a 9 to 16 byte payload keeps only its low 64 bits. **(EXP)** `plutil` read `0x82 010000` as 65536 and `0x87 …07` as 7, accepted `0x8F` with 16 bytes, read a 16-byte `2^64+5` as **5** (truncation), and refused the whole file for `0x87` carrying `2^32` and for `0x84` carrying `0x0102030405`.

**CPython** reads `1 + tokenL` bytes and writes the power-of-two widths, including 8 bytes:

> ```python
> elif tokenH == 0x80:  # UID
>     # used by Key-Archiver plist files
>     result = UID(int.from_bytes(self._fp.read(1 + tokenL), 'big'))
> ```
> PY [`Lib/plistlib.py#L611-L613`](https://github.com/python/cpython/blob/7f7ff4c99d1f53424be8c323c50f3623c3488ba1/Lib/plistlib.py#L611-L613)

> ```python
> elif value.data < 1 << 8:  self._fp.write(struct.pack('>BB', 0x80, value))
> elif value.data < 1 << 16: self._fp.write(struct.pack('>BH', 0x81, value))
> elif value.data < 1 << 32: self._fp.write(struct.pack('>BL', 0x83, value))
> elif value.data < 1 << 64: self._fp.write(struct.pack('>BQ', 0x87, value))
> else: raise OverflowError(value)
> ```
> PY [`Lib/plistlib.py#L831-L843`](https://github.com/python/cpython/blob/7f7ff4c99d1f53424be8c323c50f3623c3488ba1/Lib/plistlib.py#L831-L843) (condensed)

The `UID` constructor raises `ValueError` for values `>= 1 << 64` or below 0 (PY [`#L80-L105`](https://github.com/python/cpython/blob/7f7ff4c99d1f53424be8c323c50f3623c3488ba1/Lib/plistlib.py#L80-L105)). As a result a 16-byte payload `>= 2**64` fails to load. **(EXP)** Python read `0x82` (3 bytes), `0x84` (5 bytes, value `4328719365`, which CF rejects) and `0x8F`, and rejected the 16-byte `2^64` with `InvalidFileException`. UID support arrived in Python 3.8 with commit [`c981ad1`](https://github.com/python/cpython/commit/c981ad16b0f9740bd3381c96b4227a1faa1a88d9) (bpo-26707, the issue `github-7-binary-2.plist` comes from).

## 2. XML format

**No XML element.** The DTD's `plistObject` is `(array | data | date | dict | real | integer | string | true | false )` ([PropertyList-1.0.dtd](https://www.apple.com/DTDs/PropertyList-1.0.dtd), same as `/System/Library/DTDs/PropertyList.dtd`). **(EXP)** `plutil` refuses `<uid>7</uid>` with "Encountered unknown tag uid".

**CF writer (SCF)** has an explicit branch that writes the dictionary form on separate, indented lines:

> ```c
> } else if (typeID == _CFKeyedArchiverUIDGetTypeID()) {
>     // This is only used for the keyed archiver
>     ... "<dict>\n" ... _appendIndents(indentation+1) "<key>" "CF$UID" "</key>\n"
>     ... _appendIndents(indentation + 1) "<integer>"
>     uint64_t v = _CFKeyedArchiverUIDGetValue((CFKeyedArchiverUIDRef)object);
>     CFNumberRef num = CFNumberCreate(kCFAllocatorSystemDefault, kCFNumberSInt64Type, &v);
>     ... "</integer>\n" ... _appendIndents(indentation) "</dict>\n"
> ```
> SCF [`CFPropertyList.c#L571-L600`](https://github.com/swiftlang/swift-corelibs-foundation/blob/6f21ccf1461d160ffb27eb946e515d71269d8c1e/Sources/CoreFoundation/CFPropertyList.c#L571-L600) (condensed)

CF-1153.18's `_CFAppendXML0` has **no** UID branch ([`CFPropertyList.c#L525-L625`](https://github.com/apple-oss-distributions/CF/blob/dc54c6bb1c1e5e0b9486c1d26dd5bef110b20bf3/CFPropertyList.c#L525-L625)). Reading the code, a UID there would emit only its indentation. This is derived from the code and was not tested. UIDs pass XML validation in both versions (`if (_CFKeyedArchiverUIDGetTypeID() == type) return true;` for any format but OpenStep, SCF [`#L192`](https://github.com/swiftlang/swift-corelibs-foundation/blob/6f21ccf1461d160ffb27eb946e515d71269d8c1e/Sources/CoreFoundation/CFPropertyList.c#L192)). **(EXP)** Current `plutil -convert xml1` writes exactly the SCF layout, with tabs:

```
<dict>
	<key>a</key>
	<dict>
		<key>CF$UID</key>
		<integer>256</integer>
	</dict>
</dict>
```

**CF reader** turns a single-entry dict back into a UID when the value is any `CFNumber`, and converts it to `SInt32`:

> ```c
> CFIndex cnt = CFDictionaryGetCount(dict);
> if (1 == cnt) {
>     CFTypeRef val = CFDictionaryGetValue(dict, CFSTR("CF$UID"));
>     if (val && CFGetTypeID(val) == _kCFRuntimeIDCFNumber) {
>         ...
>         uint32_t v;
>         CFNumberGetValue((CFNumberRef)val, kCFNumberSInt32Type, &v);
>         uid = (CFTypeRef)_CFKeyedArchiverUIDCreate(pInfo->allocator, v);
> ```
> SCF [`CFPropertyList.c#L1558-L1569`](https://github.com/swiftlang/swift-corelibs-foundation/blob/6f21ccf1461d160ffb27eb946e515d71269d8c1e/Sources/CoreFoundation/CFPropertyList.c#L1558-L1569); same in CF-1153.18 [`#L1536-L1547`](https://github.com/apple-oss-distributions/CF/blob/dc54c6bb1c1e5e0b9486c1d26dd5bef110b20bf3/CFPropertyList.c#L1536-L1547)

The conditions are exactly one entry, the key `CF$UID`, and a number value, which includes `<real>`. The value range is not checked. **(EXP)** `plutil` XML to binary gave these results:

| `CF$UID` value | Result |
|---|---|
| `<integer>7</integer>` | UID 7 |
| `<integer>4294967295</integer>` | UID 4294967295 |
| `<integer>4294967296</integer>` | UID **0** |
| `<integer>4294967301</integer>` | UID **5** |
| `<integer>-1</integer>` | UID **4294967295** |
| `<real>1.9</real>` | UID **1** |
| `<string>7</string>`, `<true/>`, a second key, or key `CF$UIDx` | stays a dict |

The truncation to the low 32 bits was observed, not read from `CFNumberGetValue` documentation.

**CPython** writes no UID to XML. `write_value` has no UID arm and ends in `raise TypeError("unsupported type: %s" % type(value))` (PY [`Lib/plistlib.py#L360-L393`](https://github.com/python/cpython/blob/7f7ff4c99d1f53424be8c323c50f3623c3488ba1/Lib/plistlib.py#L360-L393)). The docs limit UID to binary: "Support added for reading and writing UID tokens in binary plists as used by NSKeyedArchiver and NSKeyedUnarchiver" (PY [`plistlib.rst#L36-L38`](https://github.com/python/cpython/blob/7f7ff4c99d1f53424be8c323c50f3623c3488ba1/Doc/library/plistlib.rst#L36-L38)). `plistlib.py` never mentions `CF$UID`, so the XML reader returns a plain dict. **(EXP)** `dumps({'a': UID(1)}, fmt=FMT_XML)` raises `TypeError`, and `loads` of a `CF$UID` dict returns `{'CF$UID': 7}`.

## 3. Uniquing in the binary object table

**CF uniques only four types by value.** UIDs fall through to the append path on every visit:

> ```c
> // Do not unique dictionaries or arrays, because: they
> // are slow to compare, and have poor hash codes.
> // Uniquing bools is unnecessary.
> if (_kCFRuntimeIDCFString == type || _kCFRuntimeIDCFNumber == type || _kCFRuntimeIDCFDate == type || _kCFRuntimeIDCFData == type) {
>     ... CFSetAddValue(uniquingset, plist); ... return;
> }
> refnum = CFArrayGetCount(objlist);
> CFArrayAppendValue(objlist, plist);
> CFDictionaryAddValue(objtable, plist, (const void *)(uintptr_t)refnum);
> ```
> SCF [`CFBinaryPList.c#L472-L494`](https://github.com/swiftlang/swift-corelibs-foundation/blob/6f21ccf1461d160ffb27eb946e515d71269d8c1e/Sources/CoreFoundation/CFBinaryPList.c#L472-L494); CF-1153.18 [`#L543-L565`](https://github.com/apple-oss-distributions/CF/blob/dc54c6bb1c1e5e0b9486c1d26dd5bef110b20bf3/CFBinaryPList.c#L543-L565)

The UID class also compares by pointer: `NULL, // equal -- pointer equality only` and `NULL, // hash -- pointer hashing only` (SCF [`#L105-L106`](https://github.com/swiftlang/swift-corelibs-foundation/blob/6f21ccf1461d160ffb27eb946e515d71269d8c1e/Sources/CoreFoundation/CFBinaryPList.c#L105-L106)). `objtable` is a hash with NULL callbacks, so it is keyed by pointer, and `CFDictionaryAddValue` does not replace an existing key. When the same UID instance is reached twice, it is therefore appended twice, every reference points at the first copy, and the second copy is written but never referenced. **(EXP)** `plutil` rewrote a file whose one UID object was referenced twice into two UID objects plus one orphan. XML with the same `CF$UID` three times became three UID objects, while three equal `<integer>`s became one object. Both UID fixtures in this repo show this orphan pattern (section 4), which suggests they were written by CF with an archiver that reuses one UID instance per value. That origin is inferred, not verified.

**CPython uniques UIDs by identity only.** `_scalars = (str, int, float, datetime.datetime, bytes)` does not include `UID`, so a UID goes through `id(value) in self._objidtable` (PY [`#L659`](https://github.com/python/cpython/blob/7f7ff4c99d1f53424be8c323c50f3623c3488ba1/Lib/plistlib.py#L659), [`#L713-L730`](https://github.com/python/cpython/blob/7f7ff4c99d1f53424be8c323c50f3623c3488ba1/Lib/plistlib.py#L713-L730)). **(EXP)** `[u, u, u]` produced one UID object, and `[UID(7), UID(7), UID(7)]` produced three.

Every reader resolves object references independently, so shared UID objects and duplicated ones are both valid input. CF caches decoded UIDs by offset ("these are always immutable", SCF [`#L1466-L1470`](https://github.com/swiftlang/swift-corelibs-foundation/blob/6f21ccf1461d160ffb27eb946e515d71269d8c1e/Sources/CoreFoundation/CFBinaryPList.c#L1466-L1470)).

## 4. Reference implementations: value width, Apple docs, fixtures

**Value width is 32 bits in CF and Foundation.** `struct __CFKeyedArchiverUID { CFRuntimeBase _base; uint32_t _value; };`, `_CFKeyedArchiverUIDCreate(CFAllocatorRef allocator, uint32_t value)` and `uint32_t _CFKeyedArchiverUIDGetValue(...)` (SCF [`CFBinaryPList.c#L84-L129`](https://github.com/swiftlang/swift-corelibs-foundation/blob/6f21ccf1461d160ffb27eb946e515d71269d8c1e/Sources/CoreFoundation/CFBinaryPList.c#L84-L129), exported with the same signatures in [`include/ForFoundationOnly.h#L426-L429`](https://github.com/swiftlang/swift-corelibs-foundation/blob/6f21ccf1461d160ffb27eb946e515d71269d8c1e/Sources/CoreFoundation/include/ForFoundationOnly.h#L426-L429); CF-1153.18 [`#L103-L152`](https://github.com/apple-oss-distributions/CF/blob/dc54c6bb1c1e5e0b9486c1d26dd5bef110b20bf3/CFBinaryPList.c#L103-L152)). Swift's `_NSKeyedArchiverUID` holds `internal let value : UInt32`, and its `isEqual` returns `false` ("no need to compare these?") ([`Foundation/NSKeyedArchiverHelpers.swift#L18-L45`](https://github.com/swiftlang/swift-corelibs-foundation/blob/6f21ccf1461d160ffb27eb946e515d71269d8c1e/Sources/Foundation/NSKeyedArchiverHelpers.swift#L18-L45)). The swift-corelibs `NSKeyedArchiver` reuses one UID instance per value (`_createObjectRefCached`, [`NSKeyedArchiver.swift#L357-L367`](https://github.com/swiftlang/swift-corelibs-foundation/blob/6f21ccf1461d160ffb27eb946e515d71269d8c1e/Sources/Foundation/NSKeyedArchiver.swift#L357-L367)). Python documents `0 <= data < 2**64` ([`plistlib.rst#L141-L150`](https://github.com/python/cpython/blob/7f7ff4c99d1f53424be8c323c50f3623c3488ba1/Doc/library/plistlib.rst#L141-L150), rendered at [docs.python.org](https://docs.python.org/3/library/plistlib.html#plistlib.UID)).

**Apple documentation says nothing concrete.** The DocC data behind [NSKeyedArchiver](https://developer.apple.com/documentation/foundation/nskeyedarchiver), [NSKeyedArchiver.outputFormat](https://developer.apple.com/documentation/foundation/nskeyedarchiver/outputformat), [PropertyListSerialization.PropertyListFormat](https://developer.apple.com/documentation/foundation/propertylistserialization/propertylistformat) and [CFPropertyListFormat](https://developer.apple.com/documentation/corefoundation/cfpropertylistformat) never mentions UID, `CF$UID` or the binary layout. The CF source comment is the only Apple specification.

**What the fixtures contain** (walked through their offset tables):

| Fixture | UID objects | Markers | Values | Notes |
|---|---|---|---|---|
| `uid-test.plist` | 15 of 68 | all `0x80` | 0–13 | UID 0 appears twice: one copy referenced twice, one orphan |
| `github-7-binary.plist` | 3 of 15 | all `0x80` | 2, 3, 35 | made from `github-7-xml.plist` with `plutil` (PList-Net commit `e05b030`) |
| `github-7-binary-2.plist` | 26 of 70 | all `0x80` | 1–22 | UID 14 is one object referenced 4 times plus 3 orphans; UID 19 has 1 orphan |
| `github-7-xml.plist` | — | — | 35, 2, 3 | three `CF$UID` dicts in the CF multi-line layout, tab-indented |
| `uid-test.xml.plist` | — | — | none | no `CF$UID` or `<uid>` at all. `$top` is `<dict/>` where the binary has `{root: UID 1}`, and every UID-valued entry is missing. Origin unknown |

No fixture contains a 2-, 3-, 4- or 8-byte UID.

## 5. This repo compared with the sources

All behaviour below was observed with **(EXP)** runs of the repo's public API.

1. **Writer marker nibble.** `UidNode.BinaryLength` returns `0/1/2/3` (log2, copied from `IntegerNode`). The spec needs `nbytes - 1`, that is `0/1/3/7`. A 4-byte UID gets `0x82` (3 bytes to CF and Python), and an 8-byte one gets `0x83`.
2. **`GetBytes` widening.** `GetBytes(ushort)` returns 4 bytes and `GetBytes(uint)` returns 8 (`EndianConverterExtensions.cs` L44-L48). `UidNode` is their only caller. Together with defect 1, every UID above 255 is written corrupt:

   | Value | Repo writes | Repo reads back | CF (`plutil`) reads | Python reads |
   |---|---|---|---|---|
   | 300 | `81 0000012c` | 0 | 0 | 0 |
   | 65536 | `82 0000000000010000` | 0 | 0 | 0 |
   | 4294967295 | `82 00000000ffffffff` | 0 | 0 | 0 |
   | 4294967296 | `83 0000000100000000` | 4294967296 | 1 | 1 |

   A keyed archive written by this repo therefore breaks as soon as it references object 256 or later.
3. **Reader width.** `ReadBinary` reads `1 << nodeLength` bytes where the spec says `nodeLength + 1`. It is correct only for `0x80` and `0x81`. For `0x82` it reads 4 bytes instead of 3, so `0x82 010000` gives 16777224. For `0x83`, **the form Apple writes for 65536 through 2^32-1**, it reads 8 bytes, so 65536 reads as 281475111652608 and 4294967295 as 18446744069549526272, with no error. `0x84`–`0x87` throw `PlistFormatException`. For `0x8F`, `BinaryFormatReader.GetObjectLengthAndTag` treats nibble `0xF` as an extended length for every tag, so a 16-byte UID throws "Invalid node length byte header", or is misparsed if the first payload byte happens to be `0x1_`.
4. **Maximum value.** Nothing is range-checked. The node is `ulong` and the writer accepts the full range, which is Python's range, while CF rejects any file with a UID above `uint.MaxValue`.
5. **XML write.** `PNode<T>.WriteXml` wraps `ToXmlString()` in `<uid>` and escapes it through `LightXmlWriter.WriteValue`, so the output is `<uid>&lt;dict&gt;&lt;key&gt;CF$UID&lt;/key&gt;&lt;integer&gt;3&lt;/integer&gt;&lt;/dict&gt;</uid>`. `plutil` rejects it ("unknown tag uid"). Python fails with `ValueError: missing value for key 'a'`, and this repo's own reader throws `NotImplementedException`.
6. **XML read.** `NodeFactory` registers the non-standard tag `uid`, and `UidNode.Parse` throws `NotImplementedException`. A `CF$UID` dict stays a `DictionaryNode`, which matches Python but not CF. So XML-to-binary conversion writes a dict plus an integer where `plutil` writes a UID.
7. **Uniquing.** `UidNode` inherits `IsBinaryUnique => true`, and `BinaryFormatWriter` dedupes it by `BinaryTag` 8 and `Value`: `[UID7, UID7, int7, UID8]` gives 4 objects. The output is valid but differs from both CF and Python, neither of which unique by value. Not a defect on its own.
8. **Object mapping.** `Serializer.Serialize` has no UID or `PNode` arm, so `Serialize(new UidNode(5))` falls through to reflection and gives `<dict><key>Value</key><string>5</string></dict>`, and no .NET type produces a UID. `Deserializer` has no `UidNode` arm, so it falls to `DeserializeObject`, which returns `null`. As a result a `ulong` member gets 0, `ulong?` and `object` members get `null`, and a top-level `Deserialize<ulong>(uidNode)` throws `NullReferenceException`.
9. **Test coverage.** Every binary fixture holds only 1-byte UIDs, so defects 1–3 go undetected. `ToString_SourceXml_Test("uid-test.xml.plist")` involves no UID at all. `github-7-xml.plist` round-trips only because `CF$UID` stays a plain dict. Two of the three binary UID tests only assert `NotNull`.

## Comparison

| | Apple CF | CPython `plistlib` | This repo (`d8cf93f`) |
|---|---|---|---|
| Read widths | nibble+1 = 1–16 bytes, any width | nibble+1 = 1–16 bytes, any width | `1 << nibble` bytes: correct for 1 and 2 only; `0x83` misread silently; `0x84`–`0x8F` throw |
| Write widths / markers | 1, 2, 4 → `0x80`, `0x81`, `0x83` | 1, 2, 4, 8 → `0x80`, `0x81`, `0x83`, `0x87` | `0x80` + 1, `0x81` + **4**, `0x82` + **8**, `0x83` + 8 |
| Maximum value | `UINT32_MAX`; larger fails the whole file (9–16 byte payloads are truncated to 64 bits first) | `2**64 - 1`; larger raises | `ulong.MaxValue`, unchecked |
| XML write | `CF$UID` dict, multi-line (SCF; no branch in CF-1153.18) | refuses (`TypeError`) | `<uid>` with escaped markup, unreadable everywhere |
| XML read | 1-entry `CF$UID` dict with a number → UID, low 32 bits | stays a dict | stays a dict; `<uid>` throws `NotImplementedException` |
| Uniquing | none for UIDs; a repeated instance adds an orphan copy | by identity only | by value |

## Recommended implementation

Every item below is testable through the public API. Items marked **Decision** are points where the sources disagree. Each lists the options and leaves the choice open.

- **`BinaryLength`** returns `nbytes - 1`: `<= byte.MaxValue → 0`, `<= ushort.MaxValue → 1`, `<= uint.MaxValue → 3`, otherwise `7`, the last only under max-value option B.
- **`GetBytes(ushort)`** returns exactly 2 big-endian bytes and **`GetBytes(uint)`** exactly 4, for example `[(byte)(v >> 8), (byte)v]` or `((short)v).GetBytes()`, which keeps the bit pattern in an unchecked context. There is no `InternalsVisibleTo`, so test them through `UidNode` output.
- **`WriteBinary`** writes 1, 2, 4 (or 8) bytes to match `BinaryLength`.
- **Reader.** Decode UID length nibbles without the extended-length rule: in `GetObjectLengthAndTag`, tag 8 with nibble `0xF` means 16 bytes. `UidNode.ReadBinary` reads `nodeLength + 1` bytes and folds them big-endian. For payloads over 8 bytes, throw `PlistFormatException` if any leading byte is non-zero, as Python does. Do not copy CF's silent 64-bit truncation. Then apply the max-value limit.
- **Decision: maximum value.**
  - (A) CF parity: allow up to `uint.MaxValue`. Reading a larger value throws `PlistFormatException`, and writing one throws instead of emitting `0x87`.
  - (B) Python parity: allow up to `ulong.MaxValue` and write `0x87` plus 8 bytes. Apple's reader then rejects the entire file.
  - Either way, keep `Value` as `ulong`. Changing it to `uint` is a third option but breaks the public API.
- **XML output.** Override `WriteXml` in `UidNode` to write the CF layout: `<dict>` at `indent`, `<key>CF$UID</key>` and `<integer>N</integer>` at `indent + 1`, `</dict>` at `indent`, tab indents, `\n` line ends, N in invariant decimal. Never write `<uid>`.
- **XML input.** After reading each child node, in `DictionaryNode.ReadXml`, `ArrayNode.ReadXml` and the root in `Plist.LoadAsXml`, replace a `DictionaryNode` that has exactly one entry, the key `CF$UID` and an `IntegerNode` value with a `UidNode`.
  - **Decision: which dicts convert.**
    - (A) Strict: convert only `0 <= v <=` the chosen maximum, and otherwise keep the dict unchanged.
    - (B) CF-exact: also convert `RealNode` values and truncate to the low 32 bits.
    - (C) Python: never convert, which is the current behaviour.
  - A or B changes what consumers get for these dicts, a `UidNode` where they used to get a `DictionaryNode`. Weigh the downstream projects before choosing.
  - Also decide whether to drop the XML registration of `uid` in `NodeFactory`, which needs a binary-only `Register` overload, or to keep it as a lenient reader.
- **Decision: uniquing.**
  - (A) Keep the current value-based dedupe. It is valid and gives smaller files.
  - (B) Set `IsBinaryUnique => false` so each occurrence is its own object, which matches Python with distinct instances and CF.
  - Nothing requires copying CF's orphan copies.
- **`Deserializer` / `Serializer`.**
  - Add a `Deserializer` arm `_ when node is UidNode uid => ConvertToType(uid.Value, type)` next to the scalar arms, and optionally return the node itself for target type `UidNode` or `PNode`.
  - Add a first `Serializer` arm `UidNode uid => uid`, or more generally `PNode n => n`, placed before the `IEnumerable` arms because `ArrayNode` and `DictionaryNode` are enumerable.
  - Update the asymmetry table in `AGENTS.md` to match.

**Round-trip tests that would prove it.**

1. **Binary write widths** (Theory). Save `new UidNode(v)` as the root. The UID object starts at offset 8, and bytes `[8..]` should equal: 0 → `80 00`, 255 → `80 ff`, 256 → `81 0100`, 65535 → `81 ffff`, 65536 → `83 00010000`, 4294967295 → `83 ffffffff`. For 4294967296, expect `87 0000000100000000` under option B or an exception under option A.
2. **Binary read widths** (Theory, inline bytes). Build a one-entry dict with a hand-written UID object: `80 05` → 5, `81 0100` → 256, `82 010000` → 65536, `83 00010000` → 65536, `87 0000000000000007` → 7, `8F` + 16 bytes of 9 → 9. Out-of-range payloads, such as `83 ffffffff` + 1 under option A or 16 bytes with a non-zero high half, should throw `PlistFormatException`.
3. **Binary round trip** of each boundary value from test 1 through `Save` then `Load`.
4. **Apple golden file.** Add a `plutil`-made fixture holding UIDs 1, 256, 65536 and 4294967295, which gives markers `0x80`, `0x81` and `0x83`. Assert the loaded values, and assert that re-saving gives the same UID object bytes.
5. **XML write.** `Plist.ToString(new DictionaryNode { ["a"] = new UidNode(256) })` should equal the `plutil` text in section 2.
6. **XML read.** A one-key `CF$UID` integer dict becomes a `UidNode`. A second key, a `<string>` value, or the key `CF$UIDx` stays a `DictionaryNode`. Add cases for `<real>`, `-1` and `4294967296` according to the chosen option.
7. **XML fixture regression.** `ToString_SourceXml_Test("github-7-xml.plist")` stays byte-identical, now through `UidNode`, since the fixture already uses the CF layout.
8. **Cross-format.** Load `github-7-xml.plist`, save as binary and reload: UIDs 35, 2 and 3 should sit where `github-7-binary.plist` has them. Load `github-7-binary-2.plist`, then `ToString`, then `Load`: `$top/data` should still be UID 1.
9. **Uniquing** pinned to the chosen option: count the objects written for `[UID7, UID7, int7]`.
10. **Mapping.** `Deserialize<Model>` puts a `UidNode` into `ulong`, `uint` and `int` members. `Serialize(new UidNode(5))` returns the same node.
11. **Fixture cleanup.** Replace or supplement `uid-test.xml.plist` with a `plutil` conversion of `uid-test.plist` so that XML UIDs are actually covered. Byte identity with the repo's writer is unverified for its other values, such as reals and data wrapping.
