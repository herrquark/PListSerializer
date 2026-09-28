using System.Collections;
using System.Xml;
using PlistSerializer.Extensions;
using PlistSerializer.Internal;
using XmlTools;

namespace PlistSerializer.Nodes;

/// <summary>
/// A plist dictionary of nodes keyed by strings.
/// </summary>
public class DictionaryNode : PNode, IDictionary<string, PNode>
{
    private readonly IDictionary<string, PNode> _dictionary = new Dictionary<string, PNode>();

    internal override string XmlTag => "dict";

    internal override byte BinaryTag => 0x0D;

    internal override int BinaryLength => Count;

    internal override bool IsBinaryUnique => false;

    internal override void ReadBinary(Stream stream, int nodeLength)
        => throw new NotImplementedException("This type of node does not do its own reading, refer to the binary reader.");

    internal override void WriteBinary(Stream stream)
        => throw new NotImplementedException("This type of node does not do its own writing, refer to the binary writer.");

    internal override void ReadXml(XmlReader reader)
    {
        var wasEmpty = reader.IsEmptyElement;
        reader.Read();

        if (wasEmpty)
            return;

        // skip white space and such to get to the first element
        reader.MoveToContent();

        while (reader.NodeType != XmlNodeType.EndElement)
        {
            // <key/> is the empty key
            var key = reader.ReadElementContentAsString("key", string.Empty);

            reader.MoveToContent();
            Add(key, NodeFactory.ReadXml(reader));

            reader.MoveToContent();
        }

        reader.ReadEndElement();
    }

    internal override void WriteXml(LightXmlWriter writer, int indent = 0)
    {
        if (Keys.Count == 0)
        {
            writer.WriteSelfClosingLineWithIndent(XmlTag, indent);
            return;
        }

        writer.WriteStartElementLineWithIndent(XmlTag, indent);

        foreach (var key in Keys)
        {
            writer.WriteElementLineWithValue("key", key, indent + 1);
            this[key].WriteXml(writer, indent + 1);
        }

        writer.WriteEndElementLineWithIndent(XmlTag, indent);
    }

    /// <inheritdoc/>
    public bool ContainsKey(string key)
        => _dictionary.ContainsKey(key);

    /// <inheritdoc/>
    public void Add(string key, PNode value)
        => _dictionary.Add(key, value);

    /// <inheritdoc/>
    public bool Remove(string key)
        => _dictionary.Remove(key);

    /// <inheritdoc/>
    public bool TryGetValue(string key, out PNode value)
        => _dictionary.TryGetValue(key, out value);

    /// <inheritdoc/>
    public PNode this[string key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }

    /// <inheritdoc/>
    public ICollection<string> Keys => _dictionary.Keys;

    /// <inheritdoc/>
    public ICollection<PNode> Values => _dictionary.Values;

    /// <inheritdoc/>
    public void Add(KeyValuePair<string, PNode> item)
        => _dictionary.Add(item);

    /// <inheritdoc/>
    public void Clear()
        => _dictionary.Clear();

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<string, PNode> item)
        => _dictionary.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<string, PNode>[] array, int arrayIndex)
        => _dictionary.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<string, PNode> item)
        => _dictionary.Remove(item);

    /// <inheritdoc/>
    public int Count => _dictionary.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, PNode>> GetEnumerator()
        => _dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _dictionary.GetEnumerator();
}
