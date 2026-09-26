using System.Reflection;
using PListSerializer.Core.Attributes;

namespace PListSerializer.Core.Extensions;

static class PropertyInfoExtensions
{
    public static string GetName(this MemberInfo memberInfo)
    {
        var result = memberInfo?
            .GetCustomAttributes(typeof(PlistNameAttribute), false)
            .Cast<PlistNameAttribute>()
            .FirstOrDefault();

        return result?.Description ?? memberInfo?.Name;
    }

    // get-only properties and indexers have no plist key to read or write
    public static bool IsPlistMember(this PropertyInfo propertyInfo)
        => propertyInfo.CanWrite && propertyInfo.GetIndexParameters().Length == 0;

    public static bool IsDictionary(this PropertyInfo property)
        => property.PropertyType.IsDictionary();

    public static HashSet<Type> GetGenericSubTypes(this PropertyInfo propertyInfo)
    {
        var result = new HashSet<Type>();
        var propertyType = propertyInfo.PropertyType;

        if (propertyType.IsArray)
            result.Add(propertyType.GetElementType());
        else if (propertyType.IsDictionary() || propertyType.IsList() || propertyType.IsHashSet())
            result = [.. propertyType.GenericTypeArguments];

        return result;
    }
}
