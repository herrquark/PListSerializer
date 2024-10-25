using PListNet.Nodes;
using PListSerializer.Core.Tests.TestModels;

namespace PListSerializer.Core.Tests;

[TestFixture]
public class PListCollectionsSerializeTests
{
    [TestCase]
    public void Recursion_Deep_SubclassArray_Test()
    {
        var arr = new ClassWithSameTypes[]
        {
            new() {Id = "0"},
            new() {Id = "1"},
            new()
            {
                Id = "2",
                ArraySameType = [ new() {Id = "20"}, new() {Id = "21"} ]
            },
            new()
            {
                Id = "3",
                ArraySameType =
                [
                    new()
                    {
                        Id = "30",
                        ArraySameType = [ new() {Id = "300"}, new() {Id = "301"} ]
                    },
                    new() {Id = "31"}
                ]
            },
            new() {Id = "4"},
            new() {Id = "5"},
            new() {Id = "hss", HashSetOfStrings = ["a", "b", "c"] },
            new() {Id = "hsc", HashSetOfSelf = [ new() {Id = "hsc1"}, new() {Id = "hsc2"} ] }
        };

        var res = Serializer.Serialize(arr) as ArrayNode;
        Assert.That(res, Is.Not.Null);
        Assert.That(res, Has.Count.EqualTo(8));

        Assert.Multiple(() => {
            Assert.That(res[0], Is.TypeOf<DictionaryNode>());
            Assert.That(res[1], Is.TypeOf<DictionaryNode>());
            Assert.That(res[2], Is.TypeOf<DictionaryNode>());
            Assert.That(res[3], Is.TypeOf<DictionaryNode>());
            Assert.That(res[4], Is.TypeOf<DictionaryNode>());
            Assert.That(res[5], Is.TypeOf<DictionaryNode>());
        });

        Assert.Multiple(() => {
            Assert.That((res[0] as DictionaryNode)["Id"], Is.TypeOf<StringNode>());
            Assert.That((res[1] as DictionaryNode)["Id"], Is.TypeOf<StringNode>());
            Assert.That((res[2] as DictionaryNode)["Id"], Is.TypeOf<StringNode>());
            Assert.That((res[3] as DictionaryNode)["Id"], Is.TypeOf<StringNode>());
            Assert.That((res[4] as DictionaryNode)["Id"], Is.TypeOf<StringNode>());
            Assert.That((res[5] as DictionaryNode)["Id"], Is.TypeOf<StringNode>());
        });

        Assert.Multiple(() => {
            Assert.That(((res[0] as DictionaryNode)["Id"] as StringNode).Value, Is.EqualTo("0"));
            Assert.That(((res[1] as DictionaryNode)["Id"] as StringNode).Value, Is.EqualTo("1"));
            Assert.That(((res[2] as DictionaryNode)["Id"] as StringNode).Value, Is.EqualTo("2"));
            Assert.That(((res[3] as DictionaryNode)["Id"] as StringNode).Value, Is.EqualTo("3"));
            Assert.That(((res[4] as DictionaryNode)["Id"] as StringNode).Value, Is.EqualTo("4"));
            Assert.That(((res[5] as DictionaryNode)["Id"] as StringNode).Value, Is.EqualTo("5"));
        });

        Assert.Multiple(() => {
            Assert.That((res[2] as DictionaryNode)["ArraySameType"], Is.TypeOf<ArrayNode>());
            Assert.That((res[3] as DictionaryNode)["ArraySameType"], Is.TypeOf<ArrayNode>());
        });

        var array2 = (res[2] as DictionaryNode)["ArraySameType"] as ArrayNode;
        Assert.That(array2, Is.Not.Null);
        Assert.Multiple(() => {
            Assert.That(array2[0], Is.TypeOf<DictionaryNode>());
            Assert.That(array2[1], Is.TypeOf<DictionaryNode>());
        });

        Assert.Multiple(() => {
            Assert.That((array2[0] as DictionaryNode)["Id"], Is.TypeOf<StringNode>());
            Assert.That((array2[1] as DictionaryNode)["Id"], Is.TypeOf<StringNode>());
        });

        Assert.Multiple(() => {
            Assert.That(((array2[0] as DictionaryNode)["Id"] as StringNode).Value, Is.EqualTo("20"));
            Assert.That(((array2[1] as DictionaryNode)["Id"] as StringNode).Value, Is.EqualTo("21"));
        });

        Assert.Multiple(() => {
            Assert.That((res[6] as DictionaryNode)["HashSetOfStrings"], Is.TypeOf<ArrayNode>());
            Assert.That((res[7] as DictionaryNode)["HashSetOfSelf"], Is.TypeOf<ArrayNode>());

        });

        var hss = new Deserializer().Deserialize<ClassWithSameTypes>(res[6]);
        Assert.That(hss.HashSetOfStrings, Is.EquivalentTo(arr[6].HashSetOfStrings));

        var hsc = new Deserializer().Deserialize<ClassWithSameTypes>(res[7]);
        Assert.That(hsc.HashSetOfSelf.Select(x => x.Id), Is.EquivalentTo(arr[7].HashSetOfSelf.Select(x => x.Id)));
    }
}
