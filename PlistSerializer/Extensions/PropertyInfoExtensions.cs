using System.Reflection;
using PlistSerializer.Core.Attributes;

namespace PlistSerializer.Core.Extensions;

internal static class PropertyInfoExtensions
{
    public static string GetName(this MemberInfo memberInfo)
        => memberInfo?.GetCustomAttribute<PlistNameAttribute>(false)?.Description ?? memberInfo?.Name;

    // get-only properties and indexers have no plist key to read or write
    public static bool IsPlistMember(this PropertyInfo propertyInfo)
        => propertyInfo.CanWrite && propertyInfo.GetIndexParameters().Length == 0;
}
