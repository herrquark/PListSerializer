using System.Xml;
using PlistSerializer.Core.Extensions;
using XmlTools;

namespace PlistSerializer.Core.Nodes;

/// <summary>
/// A plist null, which only occurs in the binary format.
/// </summary>
public class NullNode : PNode
{
    internal override string XmlTag => "null";

    internal override byte BinaryTag => 0;

    internal override int BinaryLength => 0;

    internal override bool IsBinaryUnique => false;

    internal override void ReadBinary(Stream stream, int nodeLength)
    {
        if (nodeLength != 0x00)
            throw new PlistFormatException();
    }

    internal override void WriteBinary(Stream stream)
    {
    }

    internal override void ReadXml(XmlReader reader)
        => reader.ReadStartElement(XmlTag);

    internal override void WriteXml(LightXmlWriter writer, int indent = 0)
        => writer.WriteSelfClosingLineWithIndent(XmlTag, indent);
}
