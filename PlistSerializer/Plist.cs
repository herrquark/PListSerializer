using System.Text;
using System.Xml;
using PlistSerializer.Extensions;
using PlistSerializer.Internal;
using XmlTools;

namespace PlistSerializer;

/// <summary>
/// Reads and writes property lists in XML and binary format.
/// </summary>
public static class Plist
{
    // the deepest nesting that reading and serializing accept, counting the root as 1, since deeper
    // recursion can overflow a 1 MB stack, which ends the process instead of throwing
    internal const int MaxDepth = 512;

    private static readonly Encoding Utf8NoByteOrderMark = new UTF8Encoding(false);

    /// <summary>
    /// Loads a plist from the stream, detecting whether it is in XML or binary format.
    /// </summary>
    /// <param name="stream">A seekable stream containing the plist.</param>
    /// <returns>The root node of the plist.</returns>
    public static PNode Load(Stream stream)
        => IsFormatBinary(stream)
            ? LoadAsBinary(stream)
            : LoadAsXml(stream);

    /// <summary>
    /// Saves a plist to the stream.
    /// </summary>
    /// <param name="rootNode">The root node of the plist.</param>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="format">The format to write.</param>
    public static void Save(PNode rootNode, Stream stream, PlistFormat format)
    {
        if (format == PlistFormat.Xml)
            WriteXmlToStream(rootNode, stream);
        else
            WriteBinaryToStream(rootNode, stream);
    }

    /// <summary>
    /// Writes a plist as an XML string.
    /// </summary>
    /// <param name="rootNode">The root node of the plist.</param>
    /// <param name="writePlistMeta">Whether to wrap the node in the XML declaration, the doctype and the <c>plist</c> element.</param>
    /// <returns>The plist as XML.</returns>
    public static string ToString(PNode rootNode, bool writePlistMeta = true)
    {
        using var writer = new StringWriter();
        WriteXml(rootNode, writer, writePlistMeta);
        return writer.ToString();
    }

    private static bool IsFormatBinary(Stream stream)
    {
        Span<byte> buf = stackalloc byte[8];

        // read in the first 8 bytes and rewind
        var length = 0;
        while (length < buf.Length)
        {
            var read = stream.Read(buf.Slice(length));
            if (read == 0)
                break;
            length += read;
        }
        stream.Seek(0, SeekOrigin.Begin);

        // compare to the known indicator (TODO: validate the version as well)
        return length >= 6 && buf.Slice(0, 6).SequenceEqual("bplist"u8);
    }

    private static PNode LoadAsBinary(Stream stream)
        => new BinaryFormatReader().Read(stream);

    private static PNode LoadAsXml(Stream stream)
    {
        // ignore the DTD so that reading never calls apple.com to resolve it
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Ignore,
        };

        using var reader = XmlReader.Create(stream, settings);

        reader.MoveToContent();
        reader.ReadStartElement("plist");

        reader.MoveToContent();
        var node = NodeFactory.ReadXml(reader);

        reader.ReadEndElement();

        return node;
    }

    private static void WriteXmlToStream(PNode rootNode, Stream stream, bool writePlistMeta = true)
    {
        using var streamWriter = new StreamWriter(stream, Utf8NoByteOrderMark, 512, true);
        WriteXml(rootNode, streamWriter, writePlistMeta);
    }

    private static void WriteXml(PNode rootNode, TextWriter writer, bool writePlistMeta)
    {
        using var xmlWriter = new LightXmlWriter(writer);

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
