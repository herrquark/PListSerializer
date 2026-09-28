using System.Xml;
using XmlTools;

namespace PlistSerializer.Nodes;

/// <summary>
/// A plist fill byte, which only occurs in the binary format.
/// </summary>
public class FillNode : PNode
{
    internal override string XmlTag => "fill";

    internal override byte BinaryTag => 0;

    internal override int BinaryLength => 0x0F;

    internal override bool IsBinaryUnique => false;

    internal override void ReadBinary(Stream stream, int nodeLength)
    {
        if (nodeLength != 0x0F)
            throw new PlistFormatException();
    }

    internal override void WriteBinary(Stream stream)
    {
    }

    internal override void ReadXml(XmlReader reader)
        => reader.ReadStartElement(XmlTag);

    // Apple and Python reject a <fill/> element, and so does this library's reader
    internal override void WriteXml(LightXmlWriter writer, int indent = 0)
        => throw new PlistFormatException("XML plists have no fill byte, so a FillNode can only be written in binary.");
}
