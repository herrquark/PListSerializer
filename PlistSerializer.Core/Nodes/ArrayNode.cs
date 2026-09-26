using System.Collections;
using System.Xml;
using PlistSerializer.Core.Extensions;
using PlistSerializer.Core.Internal;
using XmlTools;

namespace PlistSerializer.Core.Nodes;

/// <summary>
/// A plist array of nodes.
/// </summary>
public class ArrayNode : PNode, IList<PNode>
{
    private readonly List<PNode> _list = [];

    internal override string XmlTag => "array";

    internal override byte BinaryTag => 0x0A;

    internal override int BinaryLength => _list.Count;

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
            var node = NodeFactory.Create(reader.LocalName);
            node.ReadXml(reader);

            Add(node);
            reader.MoveToContent();
        }

        reader.ReadEndElement();
    }

    internal override void WriteXml(LightXmlWriter writer, int indent = 0)
    {
        writer.WriteStartElementLineWithIndent(XmlTag, indent);

        for (var i = 0; i < Count; i++)
            this[i].WriteXml(writer, indent + 1);

        writer.WriteEndElementLineWithIndent(XmlTag, indent);
    }

    /// <inheritdoc/>
    public int IndexOf(PNode item)
        => _list.IndexOf(item);

    /// <inheritdoc/>
    public void Insert(int index, PNode item)
        => _list.Insert(index, item);

    /// <inheritdoc/>
    public void RemoveAt(int index)
        => _list.RemoveAt(index);

    /// <inheritdoc/>
    public PNode this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }

    /// <inheritdoc/>
    public void Add(PNode item)
        => _list.Add(item);

    /// <summary>
    /// Adds the nodes to the end of the array.
    /// </summary>
    /// <param name="items">The nodes to add.</param>
    public void AddRange(IEnumerable<PNode> items)
        => _list.AddRange(items);

    /// <inheritdoc/>
    public void Clear()
        => _list.Clear();

    /// <inheritdoc/>
    public bool Contains(PNode item)
        => _list.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(PNode[] array, int arrayIndex)
        => _list.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public bool Remove(PNode item)
        => _list.Remove(item);

    /// <inheritdoc/>
    public int Count => _list.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public IEnumerator<PNode> GetEnumerator()
        => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _list.GetEnumerator();
}
