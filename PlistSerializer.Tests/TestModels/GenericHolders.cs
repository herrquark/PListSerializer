using PlistSerializer.Attributes;

namespace PlistSerializer.Tests.TestModels;

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

[PlistTypeResolver(typeof(CountingResolver))]
public class CountedResolvedHolder
{
    public string Value { get; set; }
}

public class CountingResolver : IPlistTypeResolver
{
    public static int Instances;

    public CountingResolver()
    {
        Interlocked.Increment(ref Instances);
        Thread.SpinWait(100000);
    }

    public Type ResolveType(PNode node) => null;
}
