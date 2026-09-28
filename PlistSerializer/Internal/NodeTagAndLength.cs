namespace PlistSerializer.Internal;

internal readonly struct NodeTagAndLength(byte tag, int length)
{
    public byte Tag { get; } = tag;
    public int Length { get; } = length;
}
