using PListNet;
using PListNet.Nodes;

namespace PListSerializer.Core.Converters;

internal class PrimitiveConverter<T> : IPlistConverter<T>
{
    public T Deserialize(PNode rootNode)
        => rootNode switch
        {
            PNode<T> genericNode => genericNode.Value,
            _ => default
        };

    public PNode Serialize(T obj)
        => obj switch
        {
            bool b => new BooleanNode(b),
            int i => new IntegerNode(i),
            long l => new IntegerNode(l),
            string s => new StringNode(s),
            double d => new RealNode(d),
            byte[] bytes => new DataNode(bytes),
            DateTime dt => new DateNode(dt),
            _ => throw new NotSupportedException($"Type {typeof(T)} is not supported")
        };
}
