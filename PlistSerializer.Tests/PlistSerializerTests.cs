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

    [Fact]
    public void Serialize_HiddenMembers_Test()
    {
        var source = new ClassWithHiddenMembers { Value = "a", Field = "b" };
        ((ClassWithHiddenMembersBase)source).Value = 1;
        ((ClassWithHiddenMembersBase)source).Field = 2;

        var node = Assert.IsType<DictionaryNode>(Serializer.Serialize(source));
        var res = Deserializer.Deserialize<ClassWithHiddenMembers>(node);

        // the members of the derived class hide those of the base, as in C#
        Assert.Multiple(() =>
        {
            Assert.Equal(["Field", "Value"], node.Keys);
            Assert.Equal("a", Assert.IsType<StringNode>(node["Value"]).Value);
            Assert.Equal("b", Assert.IsType<StringNode>(node["Field"]).Value);
            Assert.Equal("a", res.Value);
        });
    }

    [Fact]
    public void Serialize_SharedKey_Test()
    {
        var node = new DictionaryNode { ["Key"] = new IntegerNode(1) };

        Assert.Multiple(() =>
        {
            Assert.Throws<PlistFormatException>(() => Serializer.Serialize(new ClassWithSharedKey()));
            Assert.Throws<PlistFormatException>(() => Deserializer.Deserialize<ClassWithSharedKey>(node));
        });
    }

    [Fact]
    public void Serialize_NestingLimit_Test()
    {
        // a reference cycle would otherwise recurse until the stack overflows, which ends the process
        var cycle = new ClassWithClassSameType();
        cycle.SameClass = cycle;
        var listCycle = new List<object>();
        listCycle.Add(listCycle);

        var chain = new ClassWithClassSameType();
        for (var i = 1; i < 512; i++)
            chain = new ClassWithClassSameType { SameClass = chain };

        Assert.Multiple(() =>
        {
            Assert.Throws<PlistFormatException>(() => Serializer.Serialize(cycle));
            Assert.Throws<PlistFormatException>(() => Serializer.Serialize(listCycle));
            Assert.IsType<DictionaryNode>(Serializer.Serialize(chain));
            Assert.Throws<PlistFormatException>(() => Serializer.Serialize(new ClassWithClassSameType { SameClass = chain }));
        });
    }
}
