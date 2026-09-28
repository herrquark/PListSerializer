using PlistSerializer.Attributes;

namespace PlistSerializer.Tests.TestModels;

public class ClassWithHiddenMembersBase
{
    public int Value { get; set; }

    public int Field;
}

// reflection returns both a hiding member and the one it hides, since their types differ
public class ClassWithHiddenMembers : ClassWithHiddenMembersBase
{
    public new string Value { get; set; }

    public new string Field;
}

public class ClassWithSharedKey
{
    [PlistName("Key")]
    public int First { get; set; }

    [PlistName("Key")]
    public int Second { get; set; }
}
