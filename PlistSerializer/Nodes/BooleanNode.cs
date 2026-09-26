using System.Xml;
using PlistSerializer.Extensions;
using XmlTools;

namespace PlistSerializer.Nodes;

/// <summary>
/// A plist boolean.
/// </summary>
public sealed class BooleanNode : PNode<bool>
{
    internal override string XmlTag => "boolean";

    internal override byte BinaryTag => 0;

    internal override int BinaryLength => Value ? 9 : 8;

    internal override bool IsBinaryUnique => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanNode"/> class.
    /// </summary>
    public BooleanNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanNode"/> class with a value.
    /// </summary>
    /// <param name="value">The value of the node.</param>
    public BooleanNode(bool value)
        => Value = value;

    // the element name is the value: <true/> or <false/>
    internal override void ReadXml(XmlReader reader)
    {
        Parse(reader.LocalName);
        reader.ReadStartElement();
    }

    internal override void WriteXml(LightXmlWriter writer, int indent = 0)
        => writer.WriteSelfClosingLineWithIndent(ToXmlString(), indent);

    internal override void Parse(string data)
        => Value = data == "true";

    internal override string ToXmlString()
        => Value ? "true" : "false";

    internal override void ReadBinary(Stream stream, int nodeLength)
    {
        if (nodeLength != 8 && nodeLength != 9)
            throw new PlistFormatException();

        Value = nodeLength == 9;
    }

    // the value is written as the length in the type byte
    internal override void WriteBinary(Stream stream)
    {
    }
}
