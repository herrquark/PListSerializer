using System.Collections.Concurrent;
using PlistSerializer.Core.Attributes;

namespace PlistSerializer.Core.Extensions;

internal static class TypeExtensions
{
    // the interfaces deserialize into the concrete collection
    public static bool IsList(this Type type)
        => type.IsGenericTypeOf(typeof(List<>), typeof(IList<>), typeof(ICollection<>), typeof(IEnumerable<>), typeof(IReadOnlyList<>), typeof(IReadOnlyCollection<>));

    public static bool IsHashSet(this Type type)
        => type.IsGenericTypeOf(typeof(HashSet<>), typeof(ISet<>));

    public static bool IsDictionary(this Type type)
        => type.IsGenericTypeOf(typeof(Dictionary<,>), typeof(IDictionary<,>), typeof(IReadOnlyDictionary<,>));

    private static bool IsGenericTypeOf(this Type type, params Type[] definitions)
        => type != null &&
           type.IsGenericType &&
           definitions.Contains(type.GetGenericTypeDefinition());


    private static ConcurrentDictionary<Type, IPlistTypeResolver> ResolverCache { get; set; } = [];

    public static IPlistTypeResolver GetResolver(this Type type)
    {
        var attr = type
            .GetCustomAttributes(typeof(PlistTypeResolverAttribute), false)
            .Cast<PlistTypeResolverAttribute>()
            .FirstOrDefault();

        return attr is not null
            ? ResolverCache.GetOrAdd(type, _ => (IPlistTypeResolver)Activator.CreateInstance(attr.Resolver))
            : null;
    }
}
