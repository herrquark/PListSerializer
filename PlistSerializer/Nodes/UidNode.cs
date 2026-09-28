using PlistSerializer.Extensions;

namespace PlistSerializer.Nodes;

/// <summary>
/// A plist UID, which keyed archives use to reference objects.
/// </summary>
public class UidNode : PNode<ulong>
{
    internal override string XmlTag => "uid";

    internal override byte BinaryTag => 8;

    // the marker's low nibble is the byte count minus one, unlike an integer's power of two
    internal override int BinaryLength => ByteCount - 1;

    // Apple and Python write equal UIDs as separate objects
    internal override bool IsBinaryUnique => false;

    private int ByteCount
        => Value switch
        {
            <= byte.MaxValue => 1,
            <= ushort.MaxValue => 2,
            <= uint.MaxValue => 4,
            _ => 8
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
        var buf = new byte[nodeLength + 1];

        if (stream.Read(buf, 0, buf.Length) != buf.Length)
            throw new PlistFormatException();

        // payloads may be up to 16 bytes wide, but only the last 8 may be non-zero
        var start = Math.Max(0, buf.Length - sizeof(ulong));
        if (buf.Take(start).Any(b => b != 0))
            throw new PlistFormatException("UID > 64Bit");

        ulong value = 0;
        for (var i = start; i < buf.Length; i++)
            value = value << 8 | buf[i];

        Value = value;
    }

    internal override string ToXmlString()
        => $"<dict><key>CF$UID</key><integer>{Value}</integer></dict>";

    internal override void WriteBinary(Stream stream)
    {
        byte[] buf = ByteCount switch
        {
            1 => [(byte)Value],
            2 => ((ushort)Value).GetBytes(),
            4 => ((uint)Value).GetBytes(),
            _ => Value.GetBytes(),
        };

        stream.Write(buf, 0, buf.Length);
    }
}
