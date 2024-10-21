using PListNet;
using PListNet.Nodes;

namespace PListSerializer.Core.Converters;

internal class DictionaryConverter<TVal>(IPlistConverter<TVal> elementConverter) : IPlistConverter<Dictionary<string, TVal>>
{
    public Dictionary<string, TVal> Deserialize(PNode rootNode)
        => rootNode is DictionaryNode dictionaryNode
            ? dictionaryNode.ToDictionary(k => k.Key, v => elementConverter.Deserialize(v.Value))
            : default;
}
