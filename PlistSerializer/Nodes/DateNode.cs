using System.Globalization;
using PlistSerializer.Core.Extensions;

namespace PlistSerializer.Core.Nodes;

/// <summary>
/// A plist date.
/// </summary>
public sealed class DateNode : PNode<DateTime>
{
    // binary dates count seconds from this moment
    private static readonly DateTime ReferenceDate = new(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    internal override string XmlTag => "date";

    internal override byte BinaryTag => 3;

    internal override int BinaryLength => 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateNode"/> class.
    /// </summary>
    public DateNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DateNode"/> class with a value.
    /// </summary>
    /// <param name="value">The value of the node.</param>
    public DateNode(DateTime value)
        => Value = value;

    internal override void Parse(string data)
        => Value = DateTime.Parse(data, CultureInfo.InvariantCulture);

    internal override string ToXmlString()
        => Value.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffffZ");

    internal override void ReadBinary(Stream stream, int nodeLength)
    {
        var buf = new byte[1 << nodeLength];
        if (stream.Read(buf, 0, buf.Length) != buf.Length)
            throw new PlistFormatException();

        var seconds = nodeLength switch
        {
            < 2 => throw new PlistFormatException("Date < 32Bit"),
            2 => (double)buf.ToSingle(),
            3 => buf.ToDouble(),
            _ => throw new PlistFormatException("Date > 64Bit"),
        };

        Value = ReferenceDate.AddSeconds(seconds);
    }

    internal override void WriteBinary(Stream stream)
    {
        var buf = (Value - ReferenceDate).TotalSeconds.GetBytes();
        stream.Write(buf, 0, buf.Length);
    }
}
