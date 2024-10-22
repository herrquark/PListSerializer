using PListSerializer.Core.Attributes;

namespace PListSerializer.Core.Extensions;

internal static class TypeExtensions
{
    public static bool IsList(this Type type)
    {
        return type != null &&
               type.IsGenericType &&
               type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
    }

    public static bool IsDictionary(this Type type)
    {
        return type != null &&
               type.IsGenericType &&
               type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
    }


    private static Dictionary<Type, IPlistTypeResolver> ResolverCache { get; set; } = [];

    public static IPlistTypeResolver GetResolver(this Type type)
    {
        var attr = type
            .GetCustomAttributes(typeof(PlistTypeResolverAttribute), false)
            .Cast<PlistTypeResolverAttribute>()
            .FirstOrDefault();

        return attr is not null
            ? ResolverCache.GetOrAdd(type, () => (IPlistTypeResolver)Activator.CreateInstance(attr.Resolver))
            : null;
    }
}
