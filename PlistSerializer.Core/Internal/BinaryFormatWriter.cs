using System.Buffers.Binary;
using PlistSerializer.Core.Extensions;
using PlistSerializer.Core.Nodes;

namespace PlistSerializer.Core.Internal;

internal class BinaryFormatWriter
{
    // "bplist00"
    private static readonly byte[] Header = [0x62, 0x70, 0x6c, 0x69, 0x73, 0x74, 0x30, 0x30];

    private readonly Dictionary<byte, Dictionary<PNode, int>> _uniqueElements = [];

    public void Write(Stream stream, PNode node)
    {
        stream.Write(Header, 0, Header.Length);

        var offsets = new List<int>();
        var nodeCount = GetNodeCount(node);

        byte nodeIndexSize = nodeCount switch
        {
            <= byte.MaxValue => sizeof(byte),
            <= short.MaxValue => sizeof(short),
            _ => sizeof(int)
        };

        var topOffsetIndex = WriteInternal(stream, nodeIndexSize, offsets, node);
        nodeCount = offsets.Count;

        var offsetTableOffset = (int)stream.Position;

        byte offsetSize = offsetTableOffset switch
        {
            <= byte.MaxValue => sizeof(byte),
            <= short.MaxValue => sizeof(short),
            _ => sizeof(int)
        };

        for (var i = 0; i < offsets.Count; i++)
        {
            byte[] buf = offsetSize switch
            {
                1 => [(byte)offsets[i]],
                2 => ((short)offsets[i]).GetBytes(),
                4 => offsets[i].GetBytes(),
                _ => null
            };

            stream.Write(buf, 0, buf.Length);
        }

        var trailer = new byte[32];
        trailer[6] = offsetSize;
        trailer[7] = nodeIndexSize;

        nodeCount.GetBytes().CopyTo(trailer, 12);
        topOffsetIndex.GetBytes().CopyTo(trailer, 20);
        offsetTableOffset.GetBytes().CopyTo(trailer, 28);

        stream.Write(trailer, 0, trailer.Length);
    }

    // returns the index of the written node
    private int WriteInternal(Stream stream, byte nodeIndexSize, List<int> offsets, PNode node)
    {
        var nodeIndex = offsets.Count;
        if (node.IsBinaryUnique && node is IEquatable<PNode>)
        {
            if (!_uniqueElements.ContainsKey(node.BinaryTag))
                _uniqueElements.Add(node.BinaryTag, []);

            if (_uniqueElements[node.BinaryTag].ContainsKey(node))
            {
                if (node is BooleanNode)
                    nodeIndex = _uniqueElements[node.BinaryTag][node];
                else
                    return _uniqueElements[node.BinaryTag][node];
            }
            else
            {
                _uniqueElements[node.BinaryTag][node] = nodeIndex;
            }
        }

        var offset = (int)stream.Position;
        offsets.Add(offset);
        var length = node.BinaryLength;
        var typeCode = (byte)(node.BinaryTag << 4 | (length < 0x0F ? length : 0x0F));
        stream.WriteByte(typeCode);
        if (length >= 0x0F)
        {
            var lengthNode = NodeFactory.CreateLengthElement(length);
            var lengthTypeCode = (byte)(lengthNode.BinaryTag << 4 | lengthNode.BinaryLength);
            stream.WriteByte(lengthTypeCode);
            lengthNode.WriteBinary(stream);
        }

        if (node is ArrayNode arrayNode)
        {
            WriteInternal(stream, nodeIndexSize, offsets, arrayNode);
            return nodeIndex;
        }

        if (node is DictionaryNode dictionaryNode)
        {
            WriteInternal(stream, nodeIndexSize, offsets, dictionaryNode);
            return nodeIndex;
        }

        node.WriteBinary(stream);
        return nodeIndex;
    }

    private void WriteInternal(Stream stream, byte nodeIndexSize, List<int> offsets, ArrayNode array)
    {
        var nodes = new byte[nodeIndexSize * array.Count];
        var streamPos = stream.Position;

        stream.Write(nodes, 0, nodes.Length);
        for (var i = 0; i < array.Count; i++)
        {
            var nodeIndex = WriteInternal(stream, nodeIndexSize, offsets, array[i]);
            FormatIndex(nodeIndex, nodeIndexSize).CopyTo(nodes, nodeIndexSize * i);
        }

        stream.Seek(streamPos, SeekOrigin.Begin);
        stream.Write(nodes, 0, nodes.Length);
        stream.Seek(0, SeekOrigin.End);
    }

    private void WriteInternal(Stream stream, byte nodeIndexSize, List<int> offsets, DictionaryNode dictionary)
    {
        var keys = new byte[nodeIndexSize * dictionary.Count];
        var values = new byte[nodeIndexSize * dictionary.Count];
        var streamPos = stream.Position;
        stream.Write(keys, 0, keys.Length);
        stream.Write(values, 0, values.Length);

        var entries = dictionary.ToArray();

        for (var i = 0; i < dictionary.Count; i++)
        {
            var nodeIndex = WriteInternal(stream, nodeIndexSize, offsets, NodeFactory.CreateKeyElement(entries[i].Key));
            FormatIndex(nodeIndex, nodeIndexSize).CopyTo(keys, nodeIndexSize * i);
        }
        for (var i = 0; i < dictionary.Count; i++)
        {
            var nodeIndex = WriteInternal(stream, nodeIndexSize, offsets, entries[i].Value);
            FormatIndex(nodeIndex, nodeIndexSize).CopyTo(values, nodeIndexSize * i);
        }

        stream.Seek(streamPos, SeekOrigin.Begin);
        stream.Write(keys, 0, keys.Length);
        stream.Write(values, 0, values.Length);
        stream.Seek(0, SeekOrigin.End);
    }

    private static int GetNodeCount(PNode node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));

        if (node is ArrayNode array)
        {
            var count = 1;

            foreach (var subNode in array)
                count += GetNodeCount(subNode);

            return count;
        }

        if (node is DictionaryNode dictionary)
        {
            var count = 1;

            foreach (var subNode in dictionary.Values)
                count += GetNodeCount(subNode);

            count += dictionary.Keys.Count;
            return count;
        }

        return 1;
    }

    private static byte[] FormatIndex(int index, byte nodeIndexSize)
    {
        var buf = new byte[nodeIndexSize];

        switch (nodeIndexSize)
        {
            case 1:
                buf[0] = (byte)index;
                break;
            case 2:
                BinaryPrimitives.WriteInt16BigEndian(buf, (short)index);
                break;
            case 4:
                BinaryPrimitives.WriteInt32BigEndian(buf, index);
                break;
            default:
                throw new PlistFormatException("Invalid node index size");
        }

        return buf;
    }
}
