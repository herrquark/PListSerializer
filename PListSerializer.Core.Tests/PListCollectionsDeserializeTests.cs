using PListNet;
using PListNet.Nodes;
using PListSerializer.Core.Tests.TestModels;

namespace PListSerializer.Core.Tests;

[TestFixture]
public class PListCollectionsDeserializeTests
{
    [TestCase]
    public void Recursion_Deep_SubclassArray_Test()
    {
        var byteArray = File.ReadAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", "PList4.plist"));
        var stream = new MemoryStream(byteArray);
        var node = PList.Load(stream);
        var res = Deserializer.Deserialize<ClassWithSameTypes>(node);
        Assert.Multiple(() =>
        {
            Assert.That(res.ArraySameType, Is.Not.Null);
            Assert.That(res.Id, Is.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(res.ArraySameType[0].Id, Is.EqualTo("0"));
            Assert.That(res.ArraySameType[1].Id, Is.EqualTo("1"));
            Assert.That(res.ArraySameType[2].Id, Is.EqualTo("2"));
            Assert.That(res.ArraySameType[3].Id, Is.EqualTo("3"));
            Assert.That(res.ArraySameType[4].Id, Is.EqualTo("4"));
            Assert.That(res.ArraySameType[5].Id, Is.EqualTo("5"));

            Assert.That(res.ArraySameType[0].ArraySameType[0].Id, Is.EqualTo("00"));
            Assert.That(res.ArraySameType[0].ArraySameType[1].Id, Is.EqualTo("01"));
            Assert.That(res.ArraySameType[0].ArraySameType[0].ArraySameType[0].Id, Is.EqualTo("000"));
            Assert.That(res.ArraySameType[0].ArraySameType[0].ArraySameType[1].Id, Is.EqualTo("001"));
        });

    }

    [Test]
    public void Recursion_Deep_SubclassDictionaryAndArray_Test()
    {
        var byteArray = File.ReadAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", "PList5.plist"));
        var stream = new MemoryStream(byteArray);
        var node = PList.Load(stream);
        var root = Deserializer.Deserialize<ClassWithDictionaryAndArraySameType>(node);
        Assert.That(root.DictionaryArrays, Is.Not.Null);
        var array1 = root.DictionaryArrays["Arrays1"];

        Assert.That(array1, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(array1.Id, Is.Not.Null);
            Assert.That(array1.ArraySameType, Is.Not.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(array1.ArraySameType[0].Id, Is.EqualTo("0"));
            Assert.That(array1.ArraySameType[1].Id, Is.EqualTo("1"));
            Assert.That(array1.ArraySameType[2].Id, Is.EqualTo("2"));
            Assert.That(array1.ArraySameType[3].Id, Is.EqualTo("3"));
            Assert.That(array1.ArraySameType[4].Id, Is.EqualTo("4"));
            Assert.That(array1.ArraySameType[5].Id, Is.EqualTo("5"));

            Assert.That(array1.ArraySameType[0].ArraySameType[0].Id, Is.EqualTo("00"));
            Assert.That(array1.ArraySameType[0].ArraySameType[1].Id, Is.EqualTo("01"));
            Assert.That(array1.ArraySameType[0].ArraySameType[0].ArraySameType[0].Id, Is.EqualTo("000"));
            Assert.That(array1.ArraySameType[0].ArraySameType[0].ArraySameType[1].Id, Is.EqualTo("001"));
        });


        var array2 = root.DictionaryArrays["Arrays2"];

        Assert.That(array2, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(array2.Id, Is.Not.Null);
            Assert.That(array2.ArraySameType, Is.Not.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(array2.ArraySameType[0].Id, Is.EqualTo("0"));
            Assert.That(array2.ArraySameType[1].Id, Is.EqualTo("1"));

            Assert.That(array2.ArraySameType[1].ArraySameType[0].Id, Is.EqualTo("10"));
        });
    }

    [TestCase]
    public void Recursion_Deep_SubclassArray_WithEmpty_Test()
    {
        var byteArray = File.ReadAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", "PList6.plist"));
        var stream = new MemoryStream(byteArray);
        var node = PList.Load(stream);
        var res = Deserializer.Deserialize<ClassWithSameTypes>(node);
        Assert.That(res.ArraySameType, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.ArraySameType[0].Id, Is.EqualTo("0"));
            Assert.That(res.ArraySameType[1].Id, Is.EqualTo("1"));
            Assert.That(res.ArraySameType[1].ArraySameType[0].Id, Is.EqualTo("10"));
            Assert.That(res.ArraySameType[1].ArraySameType[1].Id, Is.EqualTo("11"));
        });
    }

    [TestCase]
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
        Console.WriteLine(PList.ToString(rootNode));

        var reObj = Deserializer.Deserialize<ClassWithSameTypes>(rootNode);

        Assert.Multiple(() =>
        {
            Assert.That(reObj.Id, Is.EqualTo("1"));
            Assert.That(reObj.NullableInt, Is.EqualTo(1));
            Assert.That(reObj.ArraySameType, Is.Not.Null);
            Assert.That(reObj.ArraySameType[0].Id, Is.EqualTo("10"));
            Assert.That(reObj.ArraySameType[0].NullableInt, Is.EqualTo(10));
            Assert.That(reObj.ArraySameType[1].Id, Is.EqualTo("11"));
            Assert.That(reObj.ArraySameType[1].NullableInt, Is.Null);

            Assert.That(reObj.NullableBool, Is.True);
            Assert.That(reObj.NullableBoolArray, Is.EqualTo(new bool?[] { true, false }));
            Assert.That(reObj.NullableBoolDictionary["true"], Is.True);
            Assert.That(reObj.NullableBoolDictionary["false"], Is.False);
            Assert.That(reObj.NullableBoolDictionary, Does.Not.ContainKey("null"));

            Assert.That(reObj.NullableDateTime, Is.Not.Null);
            Assert.That(reObj.NullableDateTimeArray.Length, Is.EqualTo(2));
            Assert.That(reObj.NullableDateTimeDictionary["now"], Is.Not.Null);
            Assert.That(reObj.NullableDateTimeDictionary["tomorrow"], Is.Not.Null);
            Assert.That(reObj.NullableDateTimeDictionary, Does.Not.ContainKey("null"));

            Assert.That(reObj.NullableDecimal, Is.EqualTo(1.1m));
            Assert.That(reObj.NullableDecimalArray, Is.EqualTo(new decimal?[] { 1.1m, 2.2m }));
            Assert.That(reObj.NullableDecimalDictionary["1.1"], Is.EqualTo(1.1m));
            Assert.That(reObj.NullableDecimalDictionary["2.2"], Is.EqualTo(2.2m));
            Assert.That(reObj.NullableDecimalDictionary, Does.Not.ContainKey("null"));

            Assert.That(reObj.NullableDouble, Is.EqualTo(1.1));
            Assert.That(reObj.NullableDoubleArray, Is.EqualTo(new double?[] { 1.1, 2.2 }));
            Assert.That(reObj.NullableDoubleDictionary["1.1"], Is.EqualTo(1.1));
            Assert.That(reObj.NullableDoubleDictionary["2.2"], Is.EqualTo(2.2));
            Assert.That(reObj.NullableDoubleDictionary, Does.Not.ContainKey("null"));

            Assert.That(reObj.NullableFloat, Is.EqualTo(1.1f));
            Assert.That(reObj.NullableFloatArray, Is.EqualTo(new float?[] { 1.1f, 2.2f }));
            Assert.That(reObj.NullableFloatDictionary["1.1"], Is.EqualTo(1.1f));
            Assert.That(reObj.NullableFloatDictionary["2.2"], Is.EqualTo(2.2f));
            Assert.That(reObj.NullableFloatDictionary, Does.Not.ContainKey("null"));

            Assert.That(reObj.NullableInt, Is.EqualTo(1));
            Assert.That(reObj.NullableIntArray, Is.EqualTo(new int?[] { 1, 2 }));
            Assert.That(reObj.NullableIntDictionary["1"], Is.EqualTo(1));
            Assert.That(reObj.NullableIntDictionary["2"], Is.EqualTo(2));
            Assert.That(reObj.NullableIntDictionary, Does.Not.ContainKey("null"));

            Assert.That(reObj.NullableLong, Is.EqualTo(1));
            Assert.That(reObj.NullableLongArray, Is.EqualTo(new long?[] { 1, 2 }));
            Assert.That(reObj.NullableLongDictionary["1"], Is.EqualTo(1));
            Assert.That(reObj.NullableLongDictionary["2"], Is.EqualTo(2));
            Assert.That(reObj.NullableLongDictionary, Does.Not.ContainKey("null"));

            Assert.That(reObj.NullableBoolList, Is.EqualTo(new List<bool?> { true, false }));
            Assert.That(reObj.NullableDateTimeList.Count, Is.EqualTo(2));
            Assert.That(reObj.NullableDecimalList, Is.EqualTo(new List<decimal?> { 1.1m, 2.2m }));
            Assert.That(reObj.NullableDoubleList, Is.EqualTo(new List<double?> { 1.1, 2.2 }));
            Assert.That(reObj.NullableFloatList, Is.EqualTo(new List<float?> { 1.1f, 2.2f }));
            Assert.That(reObj.NullableIntList, Is.EqualTo(new List<int?> { 1, 2 }));
            Assert.That(reObj.NullableLongList, Is.EqualTo(new List<long?> { 1, 2 }));
        });
    }
}
