using System.ComponentModel;

namespace PlistSerializer.Core.Attributes;

public class PlistNameAttribute(string name) : DescriptionAttribute(name)
{
}
