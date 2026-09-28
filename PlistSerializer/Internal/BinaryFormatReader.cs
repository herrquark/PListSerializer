using System.Text;
using PlistSerializer.Extensions;
using PlistSerializer.Nodes;

namespace PlistSerializer.Internal;

// reads a binary plist, as described in
// https://medium.com/@karaiskc/understanding-apples-binary-property-list-format-281e6da00dbd
internal class BinaryFormatReader
{
    public PNode Read(Stream stream)
    {
        ValidatePlistFileHeader(stream);

        var trailer = ReadTrailer(stream);
        var nodeOffsets = ReadNodeOffsets(stream, trailer);
        var readerState = new ReaderState(stream, nodeOffsets, trailer.OffsetIntSize, trailer.ObjectRefSize);

        return ReadInternal(readerState, trailer.TopObject);
    }

    private static void ValidatePlistFileHeader(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);

        var buffer = new byte[8];
        if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
            throw new PlistFormatException("Invalid plist file: must start with 8-byte header.");

        // the first 6 bytes must read "bplist"
        var text = Encoding.UTF8.GetString(buffer, 0, 6);
        if (text != "bplist")
            throw new PlistFormatException("Invalid plist file: must start with string \"bplist\".");

        // TODO: get version (ASCII numbers in bytes 7 and 8) and pass back to the parser
    }

    private static PlistTrailer ReadTrailer(Stream stream)
    {
        // the trailer is the last 32 bytes of the file
        var buffer = new byte[32];
        stream.Seek(-32, SeekOrigin.End);

        if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
            throw new PlistFormatException("Invalid plist file: unable to read trailer.");

        // all data in a binary plist file is big-endian
        return new PlistTrailer
        {
            Unused = new byte[5],
            SortVersion = buffer[5],
            OffsetIntSize = buffer[6],
            ObjectRefSize = buffer[7],
            NumObjects = buffer.ToUInt64(8),
            TopObject = buffer.ToUInt64(16),
            OffsetTableOffset = buffer.ToUInt64(24)
        };
    }

    // offsets are read as Int32 because Stream.Read takes Int32 positions
    private static int[] ReadNodeOffsets(Stream stream, PlistTrailer trailer)
    {
        if (trailer.NumObjects > int.MaxValue)
            throw new PlistFormatException($"Offset table contains too many entries: {trailer.NumObjects}.");

        // position the stream at the start of the offset table
        if (stream.Seek((long)trailer.OffsetTableOffset, SeekOrigin.Begin) != (long)trailer.OffsetTableOffset)
            throw new PlistFormatException("Invalid plist file: unable to seek to start of the offset table.");

        var buffer = new byte[trailer.OffsetIntSize];
        var nodeOffsets = new int[trailer.NumObjects];

        for (ulong i = 0; i < trailer.NumObjects; i++)
        {
            if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
                throw new PlistFormatException($"Invalid plist file: unable to read value {i} in the offset table.");

            nodeOffsets[i] = ReadNumber(buffer);
        }

        return nodeOffsets;
    }

    private static int ReadNumber(byte[] buffer)
        => buffer.Length switch
        {
            1 => buffer[0],
            2 => buffer.ToUInt16(),
            4 => (int)buffer.ToUInt32(),
            8 => (int)buffer.ToUInt64(),
            _ => throw new PlistFormatException($"Unexpected offset int size: {buffer.Length}."),
        };

    private PNode ReadInternal(ReaderState readerState, ulong nodeIndex)
    {
        readerState.Stream.Seek(readerState.NodeOffsets[nodeIndex], SeekOrigin.Begin);
        return ReadInternal(readerState);
    }

    private PNode ReadInternal(ReaderState readerState)
    {
        var tagAndLength = GetObjectLengthAndTag(readerState.Stream);
        var objectLength = tagAndLength.Length;

        var node = NodeFactory.Create(tagAndLength.Tag, objectLength);

        // arrays and dictionaries are read here, while the other nodes read themselves
        if (node is ArrayNode arrayNode)
        {
            ReadInArray(arrayNode, objectLength, readerState);
            return node;
        }

        if (node is DictionaryNode dictionaryNode)
        {
            ReadInDictionary(dictionaryNode, objectLength, readerState);
            return node;
        }

        node.ReadBinary(readerState.Stream, objectLength);

        return node;
    }

    private static NodeTagAndLength GetObjectLengthAndTag(Stream stream)
    {
        // read the marker byte
        // left 4 bits represent the tag, which indicates the node type
        // right 4 bits indicate the length
        //  - if size fits in 4 bits, the number is the length
        //  - if the bit value is 1111 and the node type carries a count, the following byte will contain information needed to decode length as follows:
        //      - 4 left bits are 0001
        //      - 4 right bits is the power of 2 required to represent the length
        //      - the following pow(2, x) bytes give us the length (big-endian)
        var buf = new byte[1];

        if (stream.Read(buf, 0, buf.Length) != buf.Length)
            throw new PlistFormatException("Couldn't read node tag byte.");

        var tag = (byte)((buf[0] >> 4) & 0x0F);
        var length = buf[0] & 0x0F;

        // length fits in 4 bits, or the node type has no count that could spill over
        if (length != 0xF || !HasCount(tag))
            return new NodeTagAndLength(tag, length);

        // read next byte to determine the length (in bytes) of actual length value
        if (stream.Read(buf, 0, buf.Length) != buf.Length)
            throw new PlistFormatException("Couldn't read node length byte.");

        // verify that leftmost bits are 0001
        if (((buf[0] >> 4) & 0x0F) != 0x1)
            throw new PlistFormatException("Invalid node length byte header.");

        // get the rightmost bits, giving us the number of bytes (power of 2) that we need
        var byteCount = (int)Math.Pow(2, buf[0] & 0x0F);

        // now get the length
        var lengthBuffer = new byte[byteCount];
        if (stream.Read(lengthBuffer, 0, lengthBuffer.Length) != lengthBuffer.Length)
            throw new PlistFormatException("Couldn't read node length byte(s).");

        length = ReadNumber(lengthBuffer);

        return new NodeTagAndLength(tag, length);
    }

    // data, strings, arrays, sets and dictionaries; for the other types the nibble is not a count,
    // so 1111 is a value of its own, such as a fill byte or a 16-byte UID
    private static bool HasCount(byte tag)
        => tag is 0x4 or 0x5 or 0x6 or 0x7 or 0xA or 0xB or 0xC or 0xD;

    private void ReadInArray(ICollection<PNode> node, int nodeLength, ReaderState readerState)
    {
        var buf = new byte[nodeLength * readerState.ObjectRefSize];

        if (readerState.Stream.Read(buf, 0, buf.Length) != buf.Length)
            throw new PlistFormatException();

        for (var i = 0; i < nodeLength; i++)
        {
            var nodeIndex = GetNodeIndex(readerState, buf, i);
            node.Add(ReadInternal(readerState, nodeIndex));
        }
    }

    private void ReadInDictionary(IDictionary<string, PNode> node, int nodeLength, ReaderState readerState)
    {
        var keyBuffer = new byte[nodeLength * readerState.ObjectRefSize];
        var valueBuffer = new byte[nodeLength * readerState.ObjectRefSize];

        if (readerState.Stream.Read(keyBuffer, 0, keyBuffer.Length) != keyBuffer.Length)
            throw new PlistFormatException();

        if (readerState.Stream.Read(valueBuffer, 0, valueBuffer.Length) != valueBuffer.Length)
            throw new PlistFormatException();

        for (var i = 0; i < nodeLength; i++)
        {
            var keyNode = ReadInternal(readerState, GetNodeIndex(readerState, keyBuffer, i));

            if (keyNode is not StringNode stringKey)
                throw new PlistFormatException("Key is not a string");

            var valueNode = ReadInternal(readerState, GetNodeIndex(readerState, valueBuffer, i));

            node.Add(stringKey.Value, valueNode);
        }
    }

    private static ulong GetNodeIndex(ReaderState readerState, byte[] buffer, int index)
        => readerState.ObjectRefSize switch
        {
            1 => buffer[index],
            2 => buffer.ToUInt16(readerState.ObjectRefSize * index),
            4 => buffer.ToUInt32(readerState.ObjectRefSize * index),
            8 => buffer.ToUInt64(readerState.ObjectRefSize * index),
            _ => throw new PlistFormatException($"Unexpected object reference size: {readerState.ObjectRefSize}."),
        };

    private sealed class ReaderState(Stream stream, int[] nodeOffsets, int offsetIntSize, int objectRefSize)
    {
        public Stream Stream { get; } = stream;
        public int[] NodeOffsets { get; } = nodeOffsets;
        public int OffsetIntSize { get; } = offsetIntSize;
        public int ObjectRefSize { get; } = objectRefSize;
    }
}
