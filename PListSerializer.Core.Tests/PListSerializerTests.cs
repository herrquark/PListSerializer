using PListNet.Nodes;
using PListSerializer.Core.Tests.TestModels;

namespace PListSerializer.Core.Tests;

public class PListSerializerTests
{
    [Fact]
    public void Serialize_PlistName_Test()
    {
        var source = new RootPList
        {
            GroupIdentifier = "Custom",
            Priority = 3,
            Hidden = true,
            Id = "259F230F-A18A-489C-87FE-024B503E1F5C"
        };

        var node = Serializer.Serialize(source) as DictionaryNode;
        Assert.NotNull(node);
        Assert.Equal(["group_identifier", "Hidden", "priority", "uuid"], node.Keys);

        var res = Deserializer.Deserialize<RootPList>(node);
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
}
