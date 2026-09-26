using System.ComponentModel;

namespace PListSerializer.Core.Attributes;

public class PlistNameAttribute(string name) : DescriptionAttribute(name)
{
}
