using System.Reflection;
using PlistSerializer.Core.Attributes;

namespace PlistSerializer.Core.Extensions;

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
}
