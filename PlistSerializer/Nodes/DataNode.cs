namespace PlistSerializer.Core.Nodes;

/// <summary>
/// A plist data blob.
/// </summary>
public sealed class DataNode : PNode<byte[]>
{
    internal override string XmlTag => "data";

    internal override byte BinaryTag => 4;

    internal override int BinaryLength => Value.Length;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataNode"/> class.
    /// </summary>
    public DataNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataNode"/> class with a value.
    /// </summary>
    /// <param name="value">The value of the node.</param>
    public DataNode(byte[] value)
        => Value = value;

    // XML holds the bytes as Base64
    internal override void Parse(string data)
        => Value = Convert.FromBase64String(data);

    internal override string ToXmlString()
        => Convert.ToBase64String(Value);

    internal override void ReadBinary(Stream stream, int nodeLength)
    {
        Value = new byte[nodeLength];

        if (stream.Read(Value, 0, Value.Length) != Value.Length)
            throw new PlistFormatException();
    }

    internal override void WriteBinary(Stream stream)
        => stream.Write(Value, 0, Value.Length);
}
