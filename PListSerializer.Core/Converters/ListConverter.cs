using PListNet;
using PListNet.Nodes;

namespace PListSerializer.Core.Converters
{
    class ListConverter<TVal>(IPlistConverter<TVal> elementConverter) : IPlistConverter<List<TVal>>
    {
        public List<TVal> Deserialize(PNode rootNode)
            => rootNode is ArrayNode arrayNode
                ? arrayNode.Select(elementConverter.Deserialize).ToList()
                : default;

        public PNode Serialize(List<TVal> obj)
        {
            var arrayNode = new ArrayNode();
            arrayNode.AddRange(obj.Select(elementConverter.Serialize));

            return arrayNode;
        }
    }
}
