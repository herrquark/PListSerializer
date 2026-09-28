using System.Xml;
using XmlTools;

namespace PlistSerializer.Nodes;

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

    // Apple and Python reject a <null/> element, and so does this library's reader
    internal override void WriteXml(LightXmlWriter writer, int indent = 0)
        => throw new PlistFormatException("XML plists have no null, so a NullNode can only be written in binary.");
}
