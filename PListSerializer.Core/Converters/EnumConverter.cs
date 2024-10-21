using PListNet;

namespace PListSerializer.Core.Converters;

internal sealed class EnumConverter<TEnum> : IPlistConverter<TEnum> where TEnum : struct
{
    public TEnum Deserialize(PNode rootNode)
        => rootNode is PNode<string> genericNode
            ? Enum.Parse<TEnum>(genericNode.Value)
            : default;
}
