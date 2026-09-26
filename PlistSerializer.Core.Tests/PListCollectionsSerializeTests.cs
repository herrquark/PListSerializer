using System.Diagnostics;
using PListNet;
using PListNet.Nodes;
using PListSerializer.Core.Tests.TestModels;

namespace PListSerializer.Core.Tests;

public class PListCollectionsSerializeTests
{
    [Fact]
    public void Recursion_Deep_SubclassArray_Test()
    {
        var arr = new ClassWithSameTypes[]
        {
            new() {Id = "0"},
            new() {Id = "1"},
            new()
            {
                Id = "2",
                ArraySameType = [ new() {Id = "20"}, new() {Id = "21"} ],
                NullableInt = 42
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
            new() {Id = "5", HashSetOfStrings = ["a", "b", "c"] },
            new() {Id = "6", HashSetOfSelf = [ new() {Id = "hsc1"}, new() {Id = "hsc2"} ] },
            new() {Id = "7", ByteArray = [1, 2, 3, 4, 5] }
        };

        var res = Serializer.Serialize(arr) as ArrayNode;
        Assert.NotNull(res);
        Assert.Equal(8, res.Count);

        Assert.Multiple(() =>
        {
            Assert.IsType<DictionaryNode>(res[0]);
            Assert.IsType<DictionaryNode>(res[1]);
            Assert.IsType<DictionaryNode>(res[2]);
            Assert.IsType<DictionaryNode>(res[3]);
            Assert.IsType<DictionaryNode>(res[4]);
            Assert.IsType<DictionaryNode>(res[5]);
        });

        Assert.Multiple(() =>
        {
            Assert.IsType<StringNode>((res[0] as DictionaryNode)[nameof(ClassWithSameTypes.Id)]);
            Assert.IsType<StringNode>((res[1] as DictionaryNode)[nameof(ClassWithSameTypes.Id)]);
            Assert.IsType<StringNode>((res[2] as DictionaryNode)[nameof(ClassWithSameTypes.Id)]);
            Assert.IsType<StringNode>((res[3] as DictionaryNode)[nameof(ClassWithSameTypes.Id)]);
            Assert.IsType<StringNode>((res[4] as DictionaryNode)[nameof(ClassWithSameTypes.Id)]);
            Assert.IsType<StringNode>((res[5] as DictionaryNode)[nameof(ClassWithSameTypes.Id)]);
        });

        Assert.Multiple(() =>
        {
            Assert.Equal("0", ((res[0] as DictionaryNode)[nameof(ClassWithSameTypes.Id)] as StringNode).Value);
            Assert.Equal("1", ((res[1] as DictionaryNode)[nameof(ClassWithSameTypes.Id)] as StringNode).Value);
            Assert.Equal("2", ((res[2] as DictionaryNode)[nameof(ClassWithSameTypes.Id)] as StringNode).Value);
            Assert.Equal("3", ((res[3] as DictionaryNode)[nameof(ClassWithSameTypes.Id)] as StringNode).Value);
            Assert.Equal("4", ((res[4] as DictionaryNode)[nameof(ClassWithSameTypes.Id)] as StringNode).Value);
            Assert.Equal("5", ((res[5] as DictionaryNode)[nameof(ClassWithSameTypes.Id)] as StringNode).Value);
        });

        // check if nulls are not serialized
        Assert.Multiple(() =>
        {
            Assert.DoesNotContain(nameof(ClassWithSameTypes.ArraySameType), res[1] as DictionaryNode);
            Assert.DoesNotContain(nameof(ClassWithSameTypes.NullableInt), res[1] as DictionaryNode);
            Assert.Contains(nameof(ClassWithSameTypes.ArraySameType), res[2] as DictionaryNode);
        });


        Assert.Multiple(() =>
        {
            Assert.IsType<ArrayNode>((res[2] as DictionaryNode)[nameof(ClassWithSameTypes.ArraySameType)]);
            Assert.IsType<ArrayNode>((res[3] as DictionaryNode)[nameof(ClassWithSameTypes.ArraySameType)]);
        });

        var array2 = (res[2] as DictionaryNode)[nameof(ClassWithSameTypes.ArraySameType)] as ArrayNode;
        Assert.NotNull(array2);
        Assert.Multiple(() =>
        {
            Assert.IsType<DictionaryNode>(array2[0]);
            Assert.IsType<DictionaryNode>(array2[1]);
        });

        Assert.Multiple(() =>
        {
            Assert.IsType<StringNode>((array2[0] as DictionaryNode)[nameof(ClassWithSameTypes.Id)]);
            Assert.IsType<StringNode>((array2[1] as DictionaryNode)[nameof(ClassWithSameTypes.Id)]);
        });

        Assert.Multiple(() =>
        {
            Assert.Equal("20", ((array2[0] as DictionaryNode)[nameof(ClassWithSameTypes.Id)] as StringNode).Value);
            Assert.Equal("21", ((array2[1] as DictionaryNode)[nameof(ClassWithSameTypes.Id)] as StringNode).Value);
        });

        Assert.Multiple(() =>
        {
            Assert.IsType<ArrayNode>((res[5] as DictionaryNode)[nameof(ClassWithSameTypes.HashSetOfStrings)]);
            Assert.IsType<ArrayNode>((res[6] as DictionaryNode)[nameof(ClassWithSameTypes.HashSetOfSelf)]);
        });

        var hss = Deserializer.Deserialize<ClassWithSameTypes>(res[5]);
        Assert.Equal(arr[5].HashSetOfStrings, hss.HashSetOfStrings);

        var ba = Deserializer.Deserialize<ClassWithSameTypes>(res[7]);
        Assert.Equal(arr[7].ByteArray, ba.ByteArray);

        // var hsc = Deserializer.Deserialize<ClassWithSameTypes>(res[7]);
        // Assert.That(hsc.HashSetOfSelf.Select(x => x.Id), Is.EquivalentTo(arr[7].HashSetOfSelf.Select(x => x.Id)));
    }

    [Fact]
    public void Array_of_Arrays_of_Objects_Serialize_Test()
    {
        var arr = new SimpleClass[][]
        {
            [
                new SimpleClass() {Id = 0, Name = "Zero"},
                new SimpleClass() {Id = 1, Name = "One"},
                new SimpleClass() {Id = 2, Name = "Two"}
            ],
            [
                new SimpleClass() {Id = 10, Name = "Ten"},
                new SimpleClass() {Id = 11, Name = "Eleven"},
                new SimpleClass() {Id = 12, Name = "Twelve"}
            ],
            [
                new SimpleClass() {Id = 20, Name = "Twenty"},
                new SimpleClass() {Id = 21, Name = "Twenty-One"},
                new SimpleClass() {Id = 22, Name = "Twenty-Two"}
            ]
        };

        var res = Serializer.Serialize(arr) as ArrayNode;
        Assert.NotNull(res);
        Assert.Equal(3, res.Count);

        Console.WriteLine(PList.ToString(res, writePlistMeta: false));
    }
}
