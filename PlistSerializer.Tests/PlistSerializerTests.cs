using PlistSerializer.Nodes;
using PlistSerializer.Tests.TestModels;

namespace PlistSerializer.Tests;

public class PlistSerializerTests
{
    [Fact]
    public void Serialize_PlistName_Test()
    {
        var source = new RootPlist
        {
            GroupIdentifier = "Custom",
            Priority = 3,
            Hidden = true,
            Id = "259F230F-A18A-489C-87FE-024B503E1F5C"
        };

        var node = Serializer.Serialize(source) as DictionaryNode;
        Assert.NotNull(node);
        Assert.Equal(["group_identifier", "Hidden", "priority", "uuid"], node.Keys);

        var res = Deserializer.Deserialize<RootPlist>(node);
        Assert.Multiple(() =>
        {
            Assert.Equal(source.GroupIdentifier, res.GroupIdentifier);
            Assert.Equal(source.Priority, res.Priority);
            Assert.Equal(source.Hidden, res.Hidden);
            Assert.Equal(source.Id, res.Id);
        });
    }

    [Fact]
    public void Serialize_ReadOnlyMembers_Test()
    {
        var node = Serializer.Serialize(new ClassWithReadOnlyMembers { Name = "Name" }) as DictionaryNode;

        Assert.NotNull(node);
        Assert.Equal([nameof(ClassWithReadOnlyMembers.Name)], node.Keys);
    }

    [Fact]
    public void Serialize_Node_Test()
    {
        var uid = new UidNode(5);
        var source = new ClassWithUid { Node = uid, AnyNode = new StringNode("a") };

        var node = Serializer.Serialize(source) as DictionaryNode;
        Assert.NotNull(node);

        var res = Deserializer.Deserialize<ClassWithUid>(node);

        Assert.Multiple(() =>
        {
            Assert.Same(uid, Serializer.Serialize(uid));
            Assert.Same(uid, node[nameof(ClassWithUid.Node)]);
            Assert.Same(source.AnyNode, node[nameof(ClassWithUid.AnyNode)]);
            Assert.Same(uid, res.Node);
        });
    }
}
