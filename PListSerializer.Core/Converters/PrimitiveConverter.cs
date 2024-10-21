using PListNet;

namespace PListSerializer.Core.Converters;

internal class PrimitiveConverter<T> : IPlistConverter<T>
{
    public T Deserialize(PNode rootNode)
        => rootNode switch
        {
            PNode<T> genericNode => genericNode.Value,
            _ => default
        };
}
