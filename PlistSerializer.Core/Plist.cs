using System.Text;
using System.Xml;
using PlistSerializer.Core.Extensions;
using PlistSerializer.Core.Internal;
using XmlTools;

namespace PlistSerializer.Core;

/// <summary>
/// Parses, saves, and creates a Plist File
/// </summary>
public static class Plist
{
    /// <summary>
    /// Loads the Plist from specified stream.
    /// </summary>
    /// <param name="stream">The stream containing the Plist.</param>
    /// <returns>A <see cref="PNode"/> object loaded from the stream</returns>
    public static PNode Load(Stream stream)
        => IsFormatBinary(stream) // Detect binary format, and read using the appropriate method
            ? LoadAsBinary(stream)
            : LoadAsXml(stream);

    private static bool IsFormatBinary(Stream stream)
    {
        var buf = new byte[8];

        // read in first 8 bytes
        stream.Read(buf, 0, buf.Length);

        // rewind
        stream.Seek(0, SeekOrigin.Begin);

        // compare to known indicator (TODO: validate version as well)
        return Encoding.UTF8.GetString(buf, 0, 6) == "bplist";
    }

    private static PNode LoadAsBinary(Stream stream)
    {
        var reader = new BinaryFormatReader();
        return reader.Read(stream);
    }

    private static PNode LoadAsXml(Stream stream)
    {
        // set resolver to null in order to avoid calls to apple.com to resolve DTD
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Ignore,
        };

        using var reader = XmlReader.Create(stream, settings);

        reader.MoveToContent();
        reader.ReadStartElement("plist");

        reader.MoveToContent();
        var node = NodeFactory.Create(reader.LocalName);
        node.ReadXml(reader);

        reader.ReadEndElement();

        return node;
    }

    /// <summary>
    /// Saves the Plist to the specified stream.
    /// </summary>
    /// <param name="rootNode">Root node of the Plist structure.</param>
    /// <param name="stream">The stream in which the Plist is saves.</param>
    /// <param name="format">The format of the Plist (Binary/Xml).</param>
    public static void Save(PNode rootNode, Stream stream, PlistFormat format)
    {
        if (format == PlistFormat.Xml)
            WriteXmlToStream(rootNode, stream);
        else
            WriteBinaryToStream(rootNode, stream);
    }

    /// <summary>
    /// Saves the Plist to the specified stream.
    /// </summary>
    /// <param name="rootNode">Root node of the Plist structure.</param>
    public static string ToString(PNode rootNode, bool writePlistMeta = true)
    {
        using var xmlStream = new MemoryStream();
        WriteXmlToStream(rootNode, xmlStream, writePlistMeta: writePlistMeta);

        return Encoding.UTF8.GetString(xmlStream.ToArray(), 0, (int)xmlStream.Length);
    }


    private static readonly Encoding UTF8NoByteOrderMark = new UTF8Encoding(false);
    private static void WriteXmlToStream(PNode rootNode, Stream stream, string newLine = "\n", bool writePlistMeta = true)
    {
        using var streamWriter = new StreamWriter(stream, UTF8NoByteOrderMark, 2048, true);
        using var xmlWriter = new LightXmlWriter(streamWriter);

        if (writePlistMeta)
            xmlWriter.WritePlistHeader();

        rootNode.WriteXml(xmlWriter);

        if (writePlistMeta)
            xmlWriter.WritePlistFooter();

        xmlWriter.Flush();
    }

    private static void WriteBinaryToStream(PNode rootNode, Stream stream)
        => new BinaryFormatWriter().Write(stream, rootNode);
}
