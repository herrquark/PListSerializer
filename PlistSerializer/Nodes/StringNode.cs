using System.Text;
using PlistSerializer.Extensions;
using XmlTools;

namespace PlistSerializer.Nodes;

/// <summary>
/// A plist string.
/// </summary>
public class StringNode : PNode<string>
{
    private string _value;

    internal override string XmlTag => "string";

    internal override byte BinaryTag => (byte)(IsUtf16 ? 6 : 5);

    internal override int BinaryLength => Value.Length;

    internal bool IsUtf16 { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringNode"/> class.
    /// </summary>
    public StringNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringNode"/> class with a value.
    /// </summary>
    /// <param name="value">The value of the node.</param>
    public StringNode(string value)
        => Value = value;

    /// <inheritdoc/>
    public sealed override string Value
    {
        get => _value;
        set
        {
            _value = value;

            // the binary format stores ASCII strings as one byte per character and all others as UTF-16
            IsUtf16 = false;
            foreach (var character in value)
            {
                if (character <= 0x7F)
                    continue;
                IsUtf16 = true;
                break;
            }
        }
    }

    internal override void Parse(string data)
        => Value = data;

    internal override void WriteXml(LightXmlWriter writer, int indent = 0)
        => writer.WriteElementLineWithValue(XmlTag, ToXmlString(), indent);

    internal override string ToXmlString()
        => Value;

    internal override void ReadBinary(Stream stream, int nodeLength)
    {
        if (nodeLength < 0)
            throw new PlistFormatException("Invalid string length.");
        var byteCount = checked(nodeLength * (IsUtf16 ? 2 : 1));
        Span<byte> buf = byteCount <= 256 ? stackalloc byte[byteCount] : new byte[byteCount];

        if (stream.Read(buf) != buf.Length)
            throw new PlistFormatException();

        var encoding = BinaryTag == 5 ? Encoding.UTF8 : Encoding.BigEndianUnicode;

        Value = encoding.GetString(buf);
    }

    internal override void WriteBinary(Stream stream)
    {
        var encoding = IsUtf16 ? Encoding.BigEndianUnicode : Encoding.UTF8;
        var byteCount = encoding.GetByteCount(Value);
        Span<byte> buf = byteCount <= 256 ? stackalloc byte[byteCount] : new byte[byteCount];
        encoding.GetBytes(Value.AsSpan(), buf);
        stream.Write(buf);
    }
}
