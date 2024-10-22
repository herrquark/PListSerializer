using PListNet;

namespace PListSerializer.Core;

public interface IPlistTypeResolver
{
    Type ResolveType(PNode node);
}

public interface IPlistTypeResolver<T> : IPlistTypeResolver where T : class
{
}
