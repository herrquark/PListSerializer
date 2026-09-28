using System.ComponentModel;
using PlistSerializer.Attributes;

namespace PlistSerializer.Performance.Models;

public class MemberBase
{
    public int Hidden { get; set; } = 13;

    public int HiddenField = 17;

    [PlistName("inherited_name")]
    public virtual string Inherited { get; set; }

    [PlistName("old_name")]
    public virtual int Renamed { get; set; }

    [DefaultValue(7)]
    public virtual int Default { get; set; } = 7;
}

public class MemberModel : MemberBase
{
    public new string Hidden { get; set; } = "derived";

    public new string HiddenField = "field initializer";

    public override string Inherited { get; set; } = "inherited attribute";

    [PlistName("new_name")]
    public override int Renamed { get; set; } = 42;

    public override int Default { get; set; } = 7;

    [DefaultValue(0)]
    public int OmittedZero { get; set; }

    [DefaultValue(7)]
    public int NonDefault { get; set; } = 8;

    public int WrittenZero { get; set; }

    public bool WrittenFalse { get; set; }

    public string Missing { get; set; } = "initializer";

    [PlistName("public_field")]
    public int Field = -7;

    [PlistName("Key")]
    public string UpperKey { get; set; } = "upper";

    [PlistName("key")]
    public string LowerKey { get; set; } = "lower";

    public string GetOnly => throw new InvalidOperationException("A get-only property must be skipped.");

    public string this[int index]
    {
        get => throw new InvalidOperationException("An indexer must be skipped.");
        set => throw new InvalidOperationException("An indexer must be skipped.");
    }
}

public class ConflictingModel
{
    [PlistName("shared")]
    public int First { get; set; }

    [PlistName("shared")]
    public int Second { get; set; }
}
