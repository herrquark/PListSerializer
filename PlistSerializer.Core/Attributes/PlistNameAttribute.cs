using System.ComponentModel;

namespace PlistSerializer.Core.Attributes;

/// <summary>
/// Sets the plist key of a property or field, which is otherwise its C# name.
/// </summary>
/// <param name="name">The plist key.</param>
public class PlistNameAttribute(string name) : DescriptionAttribute(name)
{
}
