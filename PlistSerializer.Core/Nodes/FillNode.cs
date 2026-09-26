using System.Xml;
using PlistSerializer.Core.Extensions;
using XmlTools;

namespace PlistSerializer.Core.Nodes;

/// <summary>
/// Represents a fill element in a Plist
/// </summary>
/// <remarks>Is skipped in Xml-Serialization</remarks>
public class FillNode : PNode
{
    /// <summary>
    /// Gets the Xml tag of this element.
    /// </summary>
    /// <value>The Xml tag of this element.</value>
    internal override string XmlTag => "fill";

    /// <summary>
    /// Gets the binary typecode of this element.
    /// </summary>
    /// <value>The binary typecode of this element.</value>
    internal override byte BinaryTag => 0;

    /// <summary>
    /// Gets the length of this Plist node.
    /// </summary>
    internal override int BinaryLength => 0x0F;

    /// <summary>
    /// Gets a value indicating whether this instance is written only once in binary mode.
    /// </summary>
    /// <value>
    /// 	<c>true</c> this instance is written only once in binary mode; otherwise, <c>false</c>.
    /// </value>
    internal override bool IsBinaryUnique => false;

    /// <summary>
    /// Reads this element binary from the reader.
    /// </summary>
    internal override void ReadBinary(Stream stream, int nodeLength)
    {
        if (nodeLength != 0x0F)
            throw new PlistFormatException();
    }

    /// <summary>
    /// Writes this node binary to the writer.
    /// </summary>
    internal override void WriteBinary(Stream stream)
    {
    }

    /// <summary>
    /// Generates an object from its XML representation.
    /// </summary>
    /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized.</param>
    internal override void ReadXml(XmlReader reader)
        => reader.ReadStartElement(XmlTag);

    internal override void WriteXml(LightXmlWriter writer, int indent = 0)
        => writer.WriteSelfClosingLineWithIndent(XmlTag, indent);
}
