using PListNet;
using PListSerializer.Core.Attributes;

namespace PListSerializer.Core.Tests.TestModels;

public class Holder<T>
{
    public T Value { get; set; }
}

[PlistTypeResolver(typeof(NullResolver))]
public class ResolvedHolder<T>
{
    public T Value { get; set; }
}

public class NullResolver : IPlistTypeResolver
{
    public Type ResolveType(PNode node) => null;
}
