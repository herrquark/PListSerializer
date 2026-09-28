using System.Reflection;
using PlistSerializer.Attributes;

namespace PlistSerializer.Extensions;

internal static class PropertyInfoExtensions
{
    // inherit, so that an override keeps the name its base declaration gives
    public static string GetName(this MemberInfo memberInfo)
        => memberInfo?.GetCustomAttribute<PlistNameAttribute>(true)?.Description ?? memberInfo?.Name;

    // get-only properties and indexers have no plist key to read or write
    public static bool IsPlistMember(this PropertyInfo propertyInfo)
        => propertyInfo.CanWrite && propertyInfo.GetIndexParameters().Length == 0;

    // reflection also returns a member that a derived type hides with `new` and a different type,
    // so keep the most derived of each name, as C# member lookup does, and reject a key two members claim
    public static T[] ResolvePlistKeys<T>(this IEnumerable<T> members) where T : MemberInfo
    {
        var visible = members
            .GroupBy(m => m.Name)
            .Select(g => g.Aggregate((a, b) => a.DeclaringType.IsSubclassOf(b.DeclaringType) ? a : b))
            .ToArray();

        var clash = visible.GroupBy(m => m.GetName()).FirstOrDefault(g => g.Count() > 1);

        return clash is null
            ? visible
            : throw new PlistFormatException($"The members {string.Join(" and ", clash.Select(m => $"{m.DeclaringType.Name}.{m.Name}"))} share the plist key \"{clash.Key}\".");
    }
}
