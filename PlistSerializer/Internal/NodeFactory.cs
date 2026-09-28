using System.Xml;
using PlistSerializer.Nodes;

namespace PlistSerializer.Internal;

// creates concrete nodes from an XML tag or a binary type code
internal static class NodeFactory
{
    // reads the node at the reader's position, turning a dict whose only entry is a non-negative
    // CF$UID integer into a UID; Apple also turns other numbers into UIDs, lossily, which this skips
    public static PNode ReadXml(XmlReader reader)
    {
        // the plist element is at depth 0, so the root node is at 1
        if (reader.Depth > Plist.MaxDepth)
            throw new PlistFormatException($"Invalid plist file: nodes nest deeper than {Plist.MaxDepth} levels.");

        var node = Create(reader.LocalName);
        node.ReadXml(reader);

        return node is DictionaryNode { Count: 1 } dictionary
            && dictionary.TryGetValue(UidNode.CfUidKey, out var value)
            && value is IntegerNode { Value: >= 0 } integer
                ? new UidNode((ulong)integer.Value)
                : node;
    }

    // the length tells apart the nodes that share binary tag 0: BooleanNode, NullNode and FillNode
    public static PNode Create(byte binaryTag, int length)
        => binaryTag switch
        {
            0 when length == 0x00 => new NullNode(),
            0 when length == 0x0F => new FillNode(),
            0 => new BooleanNode(),
            1 => new IntegerNode(),
            2 => new RealNode(),
            3 => new DateNode(),
            4 => new DataNode(),
            5 => new StringNode(),
            6 => new StringNode { IsUtf16 = true },
            8 => new UidNode(),
            0xA => new ArrayNode(),
            0xD => new DictionaryNode(),
            _ => throw new PlistFormatException($"Unknown node - binary tag {binaryTag}")
        };

    public static PNode Create(string tag)
        => tag switch
        {
            "dict" => new DictionaryNode(),
            "integer" => new IntegerNode(),
            "real" => new RealNode(),
            "string" or "ustring" => new StringNode(),
            "array" => new ArrayNode(),
            "data" => new DataNode(),
            "date" => new DateNode(),
            "true" or "false" or "boolean" => new BooleanNode(),
            _ => throw new PlistFormatException($"Unknown node - XML tag \"{tag}\"")
        };

    // holds the extended length of a binary node
    public static PNode CreateLengthElement(int length)
        => new IntegerNode(length);

    public static PNode CreateKeyElement(string key)
        => new StringNode(key);
}
