using PListNet;
using PListNet.Nodes;
using PListSerializer.Core.Tests.TestModels;

namespace PListSerializer.Core.Tests;

public class PListCollectionsDeserializeTests
{
    [Fact]
    public void Recursion_Deep_SubclassArray_Test()
    {
        var byteArray = File.ReadAllBytes(Path.Combine("Resources", "PList4.plist"));
        var stream = new MemoryStream(byteArray);
        var node = PList.Load(stream);
        var res = Deserializer.Deserialize<ClassWithSameTypes>(node);
        Assert.Multiple(() =>
        {
            Assert.NotNull(res.ArraySameType);
            Assert.Null(res.Id);
        });

        Assert.Multiple(() =>
        {
            Assert.Equal("0", res.ArraySameType[0].Id);
            Assert.Equal("1", res.ArraySameType[1].Id);
            Assert.Equal("2", res.ArraySameType[2].Id);
            Assert.Equal("3", res.ArraySameType[3].Id);
            Assert.Equal("4", res.ArraySameType[4].Id);
            Assert.Equal("5", res.ArraySameType[5].Id);

            Assert.Equal("00", res.ArraySameType[0].ArraySameType[0].Id);
            Assert.Equal("01", res.ArraySameType[0].ArraySameType[1].Id);
            Assert.Equal("000", res.ArraySameType[0].ArraySameType[0].ArraySameType[0].Id);
            Assert.Equal("001", res.ArraySameType[0].ArraySameType[0].ArraySameType[1].Id);
        });

    }

    [Fact]
    public void Recursion_Deep_SubclassDictionaryAndArray_Test()
    {
        var byteArray = File.ReadAllBytes(Path.Combine("Resources", "PList5.plist"));
        var stream = new MemoryStream(byteArray);
        var node = PList.Load(stream);
        var root = Deserializer.Deserialize<ClassWithDictionaryAndArraySameType>(node);
        Assert.NotNull(root.DictionaryArrays);
        var array1 = root.DictionaryArrays["Arrays1"];

        Assert.NotNull(array1);
        Assert.Multiple(() =>
        {
            Assert.NotNull(array1.Id);
            Assert.NotNull(array1.ArraySameType);
        });

        Assert.Multiple(() =>
        {
            Assert.Equal("0", array1.ArraySameType[0].Id);
            Assert.Equal("1", array1.ArraySameType[1].Id);
            Assert.Equal("2", array1.ArraySameType[2].Id);
            Assert.Equal("3", array1.ArraySameType[3].Id);
            Assert.Equal("4", array1.ArraySameType[4].Id);
            Assert.Equal("5", array1.ArraySameType[5].Id);

            Assert.Equal("00", array1.ArraySameType[0].ArraySameType[0].Id);
            Assert.Equal("01", array1.ArraySameType[0].ArraySameType[1].Id);
            Assert.Equal("000", array1.ArraySameType[0].ArraySameType[0].ArraySameType[0].Id);
            Assert.Equal("001", array1.ArraySameType[0].ArraySameType[0].ArraySameType[1].Id);
        });


        var array2 = root.DictionaryArrays["Arrays2"];

        Assert.NotNull(array2);
        Assert.Multiple(() =>
        {
            Assert.NotNull(array2.Id);
            Assert.NotNull(array2.ArraySameType);
        });

        Assert.Multiple(() =>
        {
            Assert.Equal("0", array2.ArraySameType[0].Id);
            Assert.Equal("1", array2.ArraySameType[1].Id);

            Assert.Equal("10", array2.ArraySameType[1].ArraySameType[0].Id);
        });
    }

    [Fact]
    public void Recursion_Deep_SubclassArray_WithEmpty_Test()
    {
        var byteArray = File.ReadAllBytes(Path.Combine("Resources", "PList6.plist"));
        var stream = new MemoryStream(byteArray);
        var node = PList.Load(stream);
        var res = Deserializer.Deserialize<ClassWithSameTypes>(node);
        Assert.NotNull(res.ArraySameType);
        Assert.Multiple(() =>
        {
            Assert.Equal("0", res.ArraySameType[0].Id);
            Assert.Equal("1", res.ArraySameType[1].Id);
            Assert.Equal("10", res.ArraySameType[1].ArraySameType[0].Id);
            Assert.Equal("11", res.ArraySameType[1].ArraySameType[1].Id);
        });
    }

    [Fact]
    public void Nullable_Test()
    {
        // test nullable properties
        var obj = new ClassWithSameTypes
        {
            Id = "1",
            ArraySameType =
            [
                new ClassWithSameTypes { Id = "10", NullableInt = 10 },
                new ClassWithSameTypes { Id = "11" }
            ],
            NullableBool = true,
            NullableBoolArray = [true, false, null],
            NullableBoolDictionary = new() { ["true"] = true, ["false"] = false, ["null"] = null },
            NullableDateTime = DateTime.Now,
            NullableDateTimeArray = [DateTime.Now, DateTime.Now.AddDays(1), null],
            NullableDateTimeDictionary = new() { ["now"] = DateTime.Now, ["tomorrow"] = DateTime.Now.AddDays(1), ["null"] = null },
            NullableDecimal = 1.1m,
            NullableDecimalArray = [1.1m, 2.2m, null],
            NullableDecimalDictionary = new() { ["1.1"] = 1.1m, ["2.2"] = 2.2m, ["null"] = null },
            NullableDouble = 1.1,
            NullableDoubleArray = [1.1, 2.2, null],
            NullableDoubleDictionary = new() { ["1.1"] = 1.1, ["2.2"] = 2.2, ["null"] = null },
            NullableFloat = 1.1f,
            NullableFloatArray = [1.1f, 2.2f, null],
            NullableFloatDictionary = new() { ["1.1"] = 1.1f, ["2.2"] = 2.2f, ["null"] = null },
            NullableInt = 1,
            NullableIntArray = [1, 2, null],
            NullableIntDictionary = new() { ["1"] = 1, ["2"] = 2, ["null"] = null },
            NullableLong = 1,
            NullableLongArray = [1, 2, null],
            NullableLongDictionary = new() { ["1"] = 1, ["2"] = 2, ["null"] = null },
            NullableBoolList = [true, false, null],
            NullableDateTimeList = [DateTime.Now, DateTime.Now.AddDays(1), null],
            NullableDecimalList = [1.1m, 2.2m, null],
            NullableDoubleList = [1.1, 2.2, null],
            NullableFloatList = [1.1f, 2.2f, null],
            NullableIntList = [1, 2, null],
            NullableLongList = [1, 2, null],

            ClassSameType = new ClassWithSameTypes
            {
                Id = "2",
            },
        };

        var rootNode = Serializer.Serialize(obj);

        var reObj = Deserializer.Deserialize<ClassWithSameTypes>(rootNode);

        Assert.Multiple(() =>
        {
            Assert.Equal("1", reObj.Id);
            Assert.Equal(1, reObj.NullableInt);
            Assert.NotNull(reObj.ArraySameType);
            Assert.Equal("10", reObj.ArraySameType[0].Id);
            Assert.Equal(10, reObj.ArraySameType[0].NullableInt);
            Assert.Equal("11", reObj.ArraySameType[1].Id);
            Assert.Null(reObj.ArraySameType[1].NullableInt);

            Assert.True(reObj.NullableBool);
            Assert.Equal([true, false], reObj.NullableBoolArray);
            Assert.True(reObj.NullableBoolDictionary["true"]);
            Assert.False(reObj.NullableBoolDictionary["false"]);
            Assert.DoesNotContain("null", reObj.NullableBoolDictionary.Keys);

            Assert.NotNull(reObj.NullableDateTime);
            Assert.Equal(2, reObj.NullableDateTimeArray.Length);
            Assert.NotNull(reObj.NullableDateTimeDictionary["now"]);
            Assert.NotNull(reObj.NullableDateTimeDictionary["tomorrow"]);
            Assert.DoesNotContain("null", reObj.NullableDateTimeDictionary.Keys);

            Assert.Equal(1.1m, reObj.NullableDecimal);
            Assert.Equal([1.1m, 2.2m], reObj.NullableDecimalArray);
            Assert.Equal(1.1m, reObj.NullableDecimalDictionary["1.1"]);
            Assert.Equal(2.2m, reObj.NullableDecimalDictionary["2.2"]);
            Assert.DoesNotContain("null", reObj.NullableDecimalDictionary.Keys);

            Assert.Equal(1.1, reObj.NullableDouble);
            Assert.Equal([1.1, 2.2], reObj.NullableDoubleArray);
            Assert.Equal(1.1, reObj.NullableDoubleDictionary["1.1"]);
            Assert.Equal(2.2, reObj.NullableDoubleDictionary["2.2"]);
            Assert.DoesNotContain("null", reObj.NullableDoubleDictionary.Keys);

            Assert.Equal(1.1f, reObj.NullableFloat);
            Assert.Equal([1.1f, 2.2f], reObj.NullableFloatArray);
            Assert.Equal(1.1f, reObj.NullableFloatDictionary["1.1"]);
            Assert.Equal(2.2f, reObj.NullableFloatDictionary["2.2"]);
            Assert.DoesNotContain("null", reObj.NullableFloatDictionary.Keys);

            Assert.Equal(1, reObj.NullableInt);
            Assert.Equal([1, 2], reObj.NullableIntArray);
            Assert.Equal(1, reObj.NullableIntDictionary["1"]);
            Assert.Equal(2, reObj.NullableIntDictionary["2"]);
            Assert.DoesNotContain("null", reObj.NullableIntDictionary.Keys);

            Assert.Equal(1, reObj.NullableLong);
            Assert.Equal([1, 2], reObj.NullableLongArray);
            Assert.Equal(1, reObj.NullableLongDictionary["1"]);
            Assert.Equal(2, reObj.NullableLongDictionary["2"]);
            Assert.DoesNotContain("null", reObj.NullableLongDictionary.Keys);

            Assert.Equal([true, false], reObj.NullableBoolList);
            Assert.Equal(2, reObj.NullableDateTimeList.Count);
            Assert.Equal([1.1m, 2.2m], reObj.NullableDecimalList);
            Assert.Equal([1.1, 2.2], reObj.NullableDoubleList);
            Assert.Equal([1.1f, 2.2f], reObj.NullableFloatList);
            Assert.Equal([1, 2], reObj.NullableIntList);
            Assert.Equal([1, 2], reObj.NullableLongList);
        });
    }
}
