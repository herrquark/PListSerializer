using PlistSerializer.Extensions;

namespace PlistSerializer.Nodes;

/// <summary>
/// A plist UID, which keyed archives use to reference objects.
/// </summary>
public class UidNode : PNode<ulong>
{
    internal override string XmlTag => "uid";

    internal override byte BinaryTag => 8;

    internal override int BinaryLength
        => Value switch
        {
            <= byte.MaxValue => 0,
            <= ushort.MaxValue => 1,
            <= uint.MaxValue => 2,
            _ => 3
        };

    /// <inheritdoc/>
    public sealed override ulong Value { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UidNode"/> class.
    /// </summary>
    public UidNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UidNode"/> class with a value.
    /// </summary>
    /// <param name="value">The value of the node.</param>
    public UidNode(ulong value)
        => Value = value;

    internal override void Parse(string data)
        => throw new NotImplementedException();

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
            3 => buf.ToUInt64(),
            _ => throw new PlistFormatException("Int > 64Bit"),
        };
    }

    internal override string ToXmlString()
        => $"<dict><key>CF$UID</key><integer>{Value}</integer></dict>";

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
