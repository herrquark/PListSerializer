using PListNet;
using PListNet.Nodes;

namespace PListSerializer.Core.Converters
{
    internal class DictionaryConverter<TVal>(IPlistConverter<TVal> elementConverter) : IPlistConverter<Dictionary<string, TVal>>
    {
        public Dictionary<string, TVal> Deserialize(PNode rootNode)
            => rootNode is DictionaryNode dictionaryNode
                ? dictionaryNode.ToDictionary(k => k.Key, v => elementConverter.Deserialize(v.Value))
                : default;

        public PNode Serialize(Dictionary<string, TVal> obj)
        {
            var dictionaryNode = new DictionaryNode();

            foreach (var (key, value) in obj)
                dictionaryNode.Add(key, elementConverter.Serialize(value));

            return dictionaryNode;
        }
    }
}
