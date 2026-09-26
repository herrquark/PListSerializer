namespace PlistSerializer.Core.Internal;

internal class NodeTagAndLength(byte tag, int length)
{
    public byte Tag { get; } = tag;
    public int Length { get; } = length;
}
