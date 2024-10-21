using PListNet;

namespace PListSerializer.Core.Converters;

internal class IntegerConverter : IPlistConverter<int>
{
    public int Deserialize(PNode rootNode)
        => rootNode is PNode<long> genericNode
            ? (int)genericNode.Value
            : default;
}
