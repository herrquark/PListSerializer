using System.Buffers.Binary;
using System.Globalization;

namespace PlistSerializer.Nodes;

/// <summary>
/// A plist integer.
/// </summary>
public class IntegerNode : PNode<long>
{
    internal override string XmlTag => "integer";

    internal override byte BinaryTag => 1;

    // 1, 2 and 4-byte integers are unsigned and only the 8-byte form is signed, so negatives take 8 bytes
    internal override int BinaryLength
        => Value switch
        {
            >= 0 and <= byte.MaxValue => 0,
            >= 0 and <= ushort.MaxValue => 1,
            >= 0 and <= uint.MaxValue => 2,
            _ => 3,
        };

    /// <inheritdoc/>
    public override long Value { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerNode"/> class.
    /// </summary>
    public IntegerNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerNode"/> class with a value.
    /// </summary>
    /// <param name="value">The value of the node.</param>
    public IntegerNode(long value)
        => Value = value;

    internal override void Parse(string data)
        => Value = long.Parse(data, CultureInfo.InvariantCulture);

    internal override string ToXmlString()
        => Value.ToString(CultureInfo.InvariantCulture);

    internal override void ReadBinary(Stream stream, int nodeLength)
    {
        if (nodeLength is < 0 or > 3)
            throw new PlistFormatException("Int > 64Bit");
        Span<byte> buf = stackalloc byte[1 << nodeLength];

        if (stream.Read(buf) != buf.Length)
            throw new PlistFormatException();

        Value = nodeLength switch
        {
            0 => buf[0],
            1 => BinaryPrimitives.ReadUInt16BigEndian(buf),
            2 => BinaryPrimitives.ReadUInt32BigEndian(buf),
            3 => BinaryPrimitives.ReadInt64BigEndian(buf),
            _ => throw new PlistFormatException("Int > 64Bit"),
        };
    }

    internal override void WriteBinary(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(buffer, Value);
        stream.Write(buffer.Slice(8 - (1 << BinaryLength)));
    }
}
