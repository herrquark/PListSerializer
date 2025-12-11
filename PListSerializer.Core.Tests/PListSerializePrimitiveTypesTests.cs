using PListNet;
using PListNet.Nodes;

namespace PListSerializer.Core.Tests;

public class PListSerializePrimitiveTypesTests
{
    [Theory]
    [InlineData(42)]
    [InlineData(-13423)]
    [InlineData(0)]
    public void Serialize_Int_Test(int source)
    {
        var node = Serializer.Serialize(source);
        Assert.NotNull(node);

        var intNode = node as IntegerNode;
        Assert.NotNull(intNode);

        Assert.Equal(source, intNode.Value);
    }

    [Theory]
    [InlineData(42)]
    [InlineData(-13423)]
    [InlineData(0)]
    public void Serialize_Long_Test(long source)
    {
        var node = Serializer.Serialize(source);
        Assert.NotNull(node);

        var intNode = node as IntegerNode;
        Assert.NotNull(intNode);

        Assert.Equal(source, intNode.Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Serialize_Bool_Test(bool source)
    {
        var node = Serializer.Serialize(source);
        Assert.NotNull(node);

        var boolNode = node as BooleanNode;
        Assert.NotNull(boolNode);

        Assert.Equal(source, boolNode.Value);
    }

    [Theory]
    [InlineData("String_42")]
    [InlineData("String_42grtryrthytrytryrt")]
    public void Serialize_String_Test(string source)
    {
        var node = Serializer.Serialize(source);
        Assert.NotNull(node);

        var stringNode = node as StringNode;
        Assert.NotNull(stringNode);

        Assert.Equal(source, stringNode.Value);
    }

    [Fact]
    public void Serialize_Date_Test()
    {
        DateTime source = DateTime.MaxValue;

        var node = Serializer.Serialize(source);
        Assert.NotNull(node);

        var dateNode = node as DateNode;
        Assert.NotNull(dateNode);

        Assert.Equal(source, dateNode.Value);
    }

    [Fact]
    public void Serialize_Enum_Test()
    {
        var source = TestEnum.Value2;

        var node = Serializer.Serialize(source);
        Assert.NotNull(node);

        var enumNode = node as StringNode;
        Assert.NotNull(enumNode);

        Assert.Multiple(() =>
        {
            Assert.Equal(source.ToString(), enumNode.Value);
            Assert.Equal("<string>Value2</string>\n", PList.ToString(enumNode, writePlistMeta: false));
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
