using System.Globalization;
using PlistSerializer.Nodes;

namespace PlistSerializer.Tests;

public class PlistSerializePrimitiveTypesTests
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
    public void Serialize_TimeSpan_Test()
    {
        var source = new TimeSpan(1, 2, 3, 4, 5);

        var node = Assert.IsType<StringNode>(Serializer.Serialize(source));

        Assert.Multiple(() =>
        {
            Assert.Equal("1.02:03:04.0050000", node.Value);
            Assert.Equal(source, Deserializer.Deserialize<TimeSpan>(node));
        });
    }

    [Theory]
    [InlineData("https://example.com/a%20b?c=d")]
    [InlineData("relative/path")]
    public void Serialize_Uri_Test(string source)
    {
        var uri = new Uri(source, UriKind.RelativeOrAbsolute);

        var node = Assert.IsType<StringNode>(Serializer.Serialize(uri));

        Assert.Multiple(() =>
        {
            Assert.Equal(source, node.Value);
            Assert.Equal(uri, Deserializer.Deserialize<Uri>(node));
        });
    }

    [Fact]
    public void Serialize_NegativeShort_Test()
    {
        // the Swedish culture writes a negative sign as U+2212, which other cultures do not read
        var culture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("sv-SE");
            Assert.Equal("-5", Assert.IsType<StringNode>(Serializer.Serialize((short)-5)).Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
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
            Assert.Equal("<string>Value2</string>\n", Plist.ToString(enumNode, writePlistMeta: false));
        });
    }

    public enum TestEnum
    {
        Value1,
        Value2,
        Value3,
        Value4,
        Value5
    }
}
