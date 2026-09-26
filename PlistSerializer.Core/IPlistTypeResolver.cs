namespace PlistSerializer.Core;

public interface IPlistTypeResolver
{
    Type ResolveType(PNode node);
}
