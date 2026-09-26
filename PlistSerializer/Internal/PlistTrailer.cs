namespace PlistSerializer.Core.Internal;

// the 32-byte trailer at the end of a binary plist, which Apple defines as
//  uint8_t _unused[5];
//  uint8_t _sortVersion;
//  uint8_t _offsetIntSize;
//  uint8_t _objectRefSize;
//  uint64_t _numObjects;
//  uint64_t _topObject;
//  uint64_t _offsetTableOffset;
internal struct PlistTrailer
{
    public byte[] Unused;
    public byte SortVersion;
    public byte OffsetIntSize;
    public byte ObjectRefSize;
    public ulong NumObjects;
    public ulong TopObject;
    public ulong OffsetTableOffset;
}
