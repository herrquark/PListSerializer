using PListNet;
using PListNet.Nodes;

namespace PListSerializer.Core.Converters;

class HashSetConverter<TVal>(IPlistConverter<TVal> elementConverter) : IPlistConverter<HashSet<TVal>>
{
    public HashSet<TVal> Deserialize(PNode rootNode)
        => rootNode is ArrayNode arrayNode
            ? arrayNode.Select(elementConverter.Deserialize).ToHashSet()
            : default;
}
