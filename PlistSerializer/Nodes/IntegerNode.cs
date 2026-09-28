using System.Globalization;
using PlistSerializer.Extensions;

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
        var buf = new byte[1 << nodeLength];

        if (stream.Read(buf, 0, buf.Length) != buf.Length)
            throw new PlistFormatException();

        Value = nodeLength switch
        {
            0 => buf[0],
            1 => buf.ToUInt16(),
            2 => buf.ToUInt32(),
            3 => buf.ToInt64(),
            _ => throw new PlistFormatException("Int > 64Bit"),
        };
    }

    internal override void WriteBinary(Stream stream)
    {
        byte[] buf = BinaryLength switch
        {
            0 => [(byte)Value],
            1 => ((ushort)Value).GetBytes(),
            2 => ((uint)Value).GetBytes(),
            3 => Value.GetBytes(),
            _ => throw new Exception($"Unexpected length: {BinaryLength}."),
        };

        stream.Write(buf, 0, buf.Length);
    }
}
