using PListNet;

namespace PListSerializer.Core.Converters;

public interface IPlistConverter
{
}

internal interface IPlistConverter<T> : IPlistConverter
{
    T Deserialize(PNode rootNode);
    PNode Serialize(T obj);
}
