using System.Xml;
using XmlTools;

namespace PlistSerializer;

/// <summary>
/// A node of a plist tree.
/// </summary>
public abstract class PNode
{
    internal abstract string XmlTag { get; }

    internal abstract byte BinaryTag { get; }

    internal abstract int BinaryLength { get; }

    // whether equal nodes are written to the binary format once and referenced from every place they occur
    internal abstract bool IsBinaryUnique { get; }

    internal abstract void ReadXml(XmlReader reader);

    internal abstract void WriteXml(LightXmlWriter writer, int indent = 0);

    internal abstract void ReadBinary(Stream stream, int nodeLength);

    internal abstract void WriteBinary(Stream stream);
}

/// <summary>
/// A plist node holding a single value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public abstract class PNode<T> : PNode, IEquatable<PNode>
{
    /// <summary>
    /// Gets or sets the value of the node.
    /// </summary>
    public virtual T Value { get; set; }

    internal override bool IsBinaryUnique => true;

    // reads an empty element such as <string/> as an empty value
    internal override void ReadXml(XmlReader reader)
        => Parse(reader.ReadElementContentAsString());

    internal override void WriteXml(LightXmlWriter writer, int indent = 0)
    {
        if (indent > 0)
            writer.WriteRaw(new string('\t', indent));

        writer.WriteStartElement(XmlTag);
        writer.WriteValue(ToXmlString());
        writer.WriteEndElement(XmlTag);
        writer.WriteRaw("\n");
    }

    internal abstract void Parse(string data);

    internal abstract string ToXmlString();

    /// <inheritdoc/>
    public bool Equals(PNode other)
        => other is PNode<T> node && Value.Equals(node.Value);

    /// <inheritdoc/>
    public override bool Equals(object obj)
        => obj is PNode node && Equals(node);

    /// <inheritdoc/>
    public override int GetHashCode()
        => Value.GetHashCode();

    /// <inheritdoc/>
    public override string ToString()
        => $"{XmlTag}: {Value}";
}
