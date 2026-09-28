using System.Reflection;
using System.Runtime.CompilerServices;
using PlistSerializer.Attributes;

namespace PlistSerializer.Extensions;

internal static class TypeExtensions
{
    private static readonly ConditionalWeakTable<Type, Lazy<IPlistTypeResolver>> ResolverCache = new();

    // the interfaces deserialize into the concrete collection
    public static bool IsList(this Type type)
    {
        var definition = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
        return definition == typeof(List<>) || definition == typeof(IList<>) || definition == typeof(ICollection<>)
            || definition == typeof(IEnumerable<>) || definition == typeof(IReadOnlyList<>) || definition == typeof(IReadOnlyCollection<>);
    }

    public static bool IsHashSet(this Type type)
    {
        var definition = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
        return definition == typeof(HashSet<>) || definition == typeof(ISet<>);
    }

    public static bool IsDictionary(this Type type)
    {
        var definition = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
        return definition == typeof(Dictionary<,>) || definition == typeof(IDictionary<,>) || definition == typeof(IReadOnlyDictionary<,>);
    }

    public static IPlistTypeResolver GetResolver(this Type type)
        => ResolverCache.GetValue(type, CreateResolver).Value;

    // Cache missing attributes too. Only the winning Lazy constructs a shared resolver.
    private static Lazy<IPlistTypeResolver> CreateResolver(Type type)
        => new(() => BuildResolver(type));

    private static IPlistTypeResolver BuildResolver(Type type)
    {
        var attribute = type.GetCustomAttribute<PlistTypeResolverAttribute>(false);

        return attribute is not null
            ? (IPlistTypeResolver)Activator.CreateInstance(attribute.Resolver)
            : null;
    }
}
