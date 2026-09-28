using PlistSerializer.Nodes;

namespace PlistSerializer.Tests.TestModels;

public class ClassWithUid
{
    public ulong Unsigned { get; set; }
    public int Signed { get; set; }
    public ulong? Nullable { get; set; }
    public object Boxed { get; set; }
    public UidNode Node { get; set; }
    public PNode AnyNode { get; set; }
}
