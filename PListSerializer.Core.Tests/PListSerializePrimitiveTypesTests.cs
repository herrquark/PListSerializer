using PListNet;
using PListNet.Nodes;

namespace PListSerializer.Core.Tests;

[TestFixture]
public class PListSerializePrimitiveTypesTests
{
    [OneTimeSetUp]
    public void SetUp()
    {
    }

    [TestCase(42)]
    [TestCase(-13423)]
    [TestCase(0)]
    public void Serialize_Int_Test(int source)
    {
        var node = Serializer.Serialize(source);
        Assert.That(node, Is.Not.Null);

        var intNode = node as IntegerNode;
        Assert.That(intNode, Is.Not.Null);

        Assert.That(intNode.Value, Is.EqualTo(source));
    }

    [TestCase(42)]
    [TestCase(-13423)]
    [TestCase(0)]
    public void Serialize_Long_Test(long source)
    {
        var node = Serializer.Serialize(source);
        Assert.That(node, Is.Not.Null);

        var intNode = node as IntegerNode;
        Assert.That(intNode, Is.Not.Null);

        Assert.That(intNode.Value, Is.EqualTo(source));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Serialize_Bool_Test(bool source)
    {
        var node = Serializer.Serialize(source);
        Assert.That(node, Is.Not.Null);

        var boolNode = node as BooleanNode;
        Assert.That(boolNode, Is.Not.Null);

        Assert.That(boolNode.Value, Is.EqualTo(source));
    }

    [TestCase("String_42")]
    [TestCase("String_42grtryrthytrytryrt")]
    public void Serialize_String_Test(string source)
    {
        var node = Serializer.Serialize(source);
        Assert.That(node, Is.Not.Null);

        var stringNode = node as StringNode;
        Assert.That(stringNode, Is.Not.Null);

        Assert.That(stringNode.Value, Is.EqualTo(source));
    }

    [TestCase]
    public void Serialize_Date_Test()
    {
        DateTime source = DateTime.MaxValue;

        var node = Serializer.Serialize(source);
        Assert.That(node, Is.Not.Null);

        var dateNode = node as DateNode;
        Assert.That(dateNode, Is.Not.Null);

        Assert.That(dateNode.Value, Is.EqualTo(source));
    }

    [TestCase]
    public void Serialize_Enum_Test()
    {
        var source = TestEnum.Value2;

        var node = Serializer.Serialize(source);
        Assert.That(node, Is.Not.Null);

        var enumNode = node as StringNode;
        Assert.That(enumNode, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(enumNode.Value, Is.EqualTo(source.ToString()));
            Assert.That(PList.ToString(enumNode, writePlistMeta: false), Is.EqualTo("<string>Value2</string>\n"));
        });

    }

    // [TestCase]
    // public void Serialize_Enum_Test()
    // {
    //     var source = TestEnum.Value2;

    //     var node = Serializer.Serialize(source);
    //     Assert.That(node, Is.Not.Null);

    //     var enumNode = node as StringNode;
    //     Assert.That(enumNode, Is.Not.Null);

    //     Assert.That(enumNode.Value, Is.EqualTo(source.ToString()));

    //     Assert.That(PList.ToString(enumNode, writePlistMeta: false), Is.EqualTo("<string>Value2</string>\n"));

    //     var source2 = new TestClass
    //     {
    //         Enum = source,
    //         Enum2 = TestEnum.Value3,
    //         Dict = new Dictionary<string, object>
    //         {
    //             { "e", source },
    //             { "i", 123123 },
    //             { "d", 1.23232e+42 },
    //             { "o", new TestClass.SubClass()
    //             {
    //                 e = TestEnum.Value5,
    //                 i = 123123,
    //                 d = 1.23232e+42
    //             } }
    //         },
    //         Sub = new()
    //         {
    //             e = TestEnum.Value2,
    //             i = 123123,
    //             d = 1.23232e+42,
    //             Sub = new()
    //             {
    //                 e = TestEnum.Value4,
    //                 i = 123123,
    //                 d = 1.23232e+42
    //             }
    //         }
    //     };
    // }

    public enum TestEnum
    {
        Value1,
        Value2,
        Value3,
        Value4,
        Value5
    }

    // public class TestClass
    // {
    //     public TestEnum Enum { get; set; }
    //     public TestEnum Enum2 { get; set; }
    //     public decimal Dec { get; set; } = 3.234m;
    //     public Dictionary<string, object> Dict { get; set; }
    //     public SubClass Sub { get; set; } = default!;

    //     public class SubClass
    //     {
    //         public TestEnum e { get; set; }
    //         public int i { get; set; }
    //         public double d { get; set; }

    //         public SubClass Sub { get; set; } = default!;
    //     }
    // }
}
