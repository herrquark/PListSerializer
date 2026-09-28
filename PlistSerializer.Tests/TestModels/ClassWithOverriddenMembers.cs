using System.ComponentModel;
using PlistSerializer.Attributes;

namespace PlistSerializer.Tests.TestModels;

public class ClassWithVirtualMembers
{
    [PlistName("inherited_key")]
    [DefaultValue(7)]
    public virtual int Inherited { get; set; }

    [PlistName("base_key")]
    public virtual int Renamed { get; set; }
}

public class ClassWithOverriddenMembers : ClassWithVirtualMembers
{
    public override int Inherited { get; set; }

    [PlistName("derived_key")]
    public override int Renamed { get; set; }
}
