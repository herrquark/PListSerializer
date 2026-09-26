using System.Collections.Concurrent;
using System.Reflection;
using PlistSerializer.Attributes;

namespace PlistSerializer.Extensions;

internal static class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, IPlistTypeResolver> ResolverCache = [];

    // the interfaces deserialize into the concrete collection
    public static bool IsList(this Type type)
        => type.IsGenericTypeOf(typeof(List<>), typeof(IList<>), typeof(ICollection<>), typeof(IEnumerable<>), typeof(IReadOnlyList<>), typeof(IReadOnlyCollection<>));

    public static bool IsHashSet(this Type type)
        => type.IsGenericTypeOf(typeof(HashSet<>), typeof(ISet<>));

    public static bool IsDictionary(this Type type)
        => type.IsGenericTypeOf(typeof(Dictionary<,>), typeof(IDictionary<,>), typeof(IReadOnlyDictionary<,>));

    public static IPlistTypeResolver GetResolver(this Type type)
    {
        var attribute = type.GetCustomAttribute<PlistTypeResolverAttribute>(false);

        return attribute is not null
            ? ResolverCache.GetOrAdd(type, _ => (IPlistTypeResolver)Activator.CreateInstance(attribute.Resolver))
            : null;
    }

    private static bool IsGenericTypeOf(this Type type, params Type[] definitions)
        => type != null &&
           type.IsGenericType &&
           definitions.Contains(type.GetGenericTypeDefinition());
}
