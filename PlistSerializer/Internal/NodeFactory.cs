using System.Xml;
using PlistSerializer.Nodes;

namespace PlistSerializer.Internal;

// creates concrete nodes from an XML tag or a binary type code
internal static class NodeFactory
{
    private static readonly Dictionary<string, Type> XmlTags = [];
    private static readonly Dictionary<byte, Type> BinaryTags = [];

    static NodeFactory()
    {
        Register(new DictionaryNode());
        Register(new IntegerNode());
        Register(new RealNode());
        Register(new StringNode());
        Register(new ArrayNode());
        Register(new DataNode());
        Register(new DateNode());

        // UIDs have no XML element, ReadXml finds them by their dict
        RegisterBinary(new UidNode());

        Register("string", 5, new StringNode());
        Register("ustring", 6, new StringNode());

        Register("true", 0, new BooleanNode());
        Register("false", 0, new BooleanNode());
    }

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
            6 => new StringNode { IsUtf16 = true },
            _ when BinaryTags.ContainsKey(binaryTag) => (PNode)Activator.CreateInstance(BinaryTags[binaryTag]),
            _ => throw new PlistFormatException($"Unknown node - binary tag {binaryTag}")
        };

    public static PNode Create(string tag)
        => XmlTags.ContainsKey(tag)
            ? (PNode)Activator.CreateInstance(XmlTags[tag])
            : throw new PlistFormatException($"Unknown node - XML tag \"{tag}\"");

    // holds the extended length of a binary node
    public static PNode CreateLengthElement(int length)
        => new IntegerNode(length);

    public static PNode CreateKeyElement(string key)
        => new StringNode(key);

    private static void Register<T>(T node) where T : PNode, new()
    {
        if (!XmlTags.ContainsKey(node.XmlTag))
            XmlTags.Add(node.XmlTag, node.GetType());

        RegisterBinary(node);
    }

    private static void RegisterBinary<T>(T node) where T : PNode, new()
    {
        if (!BinaryTags.ContainsKey(node.BinaryTag))
            BinaryTags.Add(node.BinaryTag, node.GetType());
    }

    private static void Register<T>(string xmlTag, byte binaryTag, T node) where T : PNode, new()
    {
        if (!XmlTags.ContainsKey(xmlTag))
            XmlTags.Add(xmlTag, node.GetType());

        if (!BinaryTags.ContainsKey(binaryTag))
            BinaryTags.Add(binaryTag, node.GetType());
    }
}
