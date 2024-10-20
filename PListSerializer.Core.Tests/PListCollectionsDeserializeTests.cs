using PListNet;
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
        var d = new Deserializer();
        var res = d.Deserialize<ClassWithSameTypes>(node);
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
        var d = new Deserializer();
        var root = d.Deserialize<ClassWithDictionaryAndArraySameType>(node);
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
        var d = new Deserializer();
        var res = d.Deserialize<ClassWithSameTypes>(node);
        Assert.That(res.ArraySameType, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.ArraySameType[0].Id, Is.EqualTo("0"));
            Assert.That(res.ArraySameType[1].Id, Is.EqualTo("1"));
            Assert.That(res.ArraySameType[1].ArraySameType[0].Id, Is.EqualTo("10"));
            Assert.That(res.ArraySameType[1].ArraySameType[1].Id, Is.EqualTo("11"));
        });

    }
}
