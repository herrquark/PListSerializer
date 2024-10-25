using System.Reflection;
using PListSerializer.Core.Attributes;

namespace PListSerializer.Core.Extensions;

static class PropertyInfoExtensions
{
    public static string GetName(this PropertyInfo propertyInfo)
    {
        var result = propertyInfo?
            .GetCustomAttributes(typeof(PlistNameAttribute), false)
            .Cast<PlistNameAttribute>()
            .FirstOrDefault();

        return result?.Description ?? propertyInfo?.Name;
    }

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
