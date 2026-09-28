using System.Globalization;
using PlistSerializer.Nodes;

namespace PlistSerializer.Tests;

public class PlistDeserializePrimitiveTypesTests
{
    [Theory]
    [InlineData(42)]
    [InlineData(-13423)]
    [InlineData(0)]
    public void Deserialize_Int_Test(int source)
    {
        var node = new IntegerNode(source);
        var res = Deserializer.Deserialize<int>(node);
        Assert.IsType<int>(res);
        Assert.Equal(source, res);
    }

    [Theory]
    [InlineData(42)]
    [InlineData(-13423)]
    [InlineData(0)]
    public void Deserialize_Long_Test(long source)
    {
        var node = new IntegerNode(source);
        var res = Deserializer.Deserialize<long>(node);
        Assert.IsType<long>(res);
        Assert.Equal(source, res);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Deserialize_Bool_Test(bool source)
    {
        var node = new BooleanNode(source);
        var res = Deserializer.Deserialize<bool>(node);
        Assert.IsType<bool>(res);
        Assert.Equal(source, res);
    }

    [Theory]
    [InlineData("String_42")]
    [InlineData("String_42grtryrthytrytryrt")]
    public void Deserialize_String_Test(string source)
    {
        var node = new StringNode(source);
        var res = Deserializer.Deserialize<string>(node);
        Assert.IsType<string>(res);
        Assert.Equal(source, res);
    }

    [Fact]
    public void Deserialize_DateTime_Test()
    {
        DateTime source = DateTime.MaxValue;

        var node = new DateNode(source);
        var res = Deserializer.Deserialize<DateTime>(node);
        Assert.IsType<DateTime>(res);
        Assert.Equal(source, res);
    }

    [Fact]
    public void Deserialize_TimeSpan_Test()
    {
        TimeSpan source = TimeSpan.MaxValue;

        var node = new StringNode(source.ToString());
        var res = Deserializer.Deserialize<TimeSpan>(node);
        Assert.IsType<TimeSpan>(res);
        Assert.Equal(source, res);
    }

    [Fact]
    public void Deserialize_Uri_Test()
    {
        Uri source = new("https://example.com");

        var node = new StringNode(source.ToString());
        var res = Deserializer.Deserialize<Uri>(node);
        Assert.IsType<Uri>(res);
        Assert.Equal(source, res);
    }

    [Fact]
    public void Deserialize_DoubleFromString_Test()
    {
        // the German culture reads the point as a group separator
        var culture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            Assert.Equal(1.5, Deserializer.Deserialize<double>(new StringNode("1.5")));
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Theory]
    [InlineData("Value2")]
    [InlineData("value2")]
    [InlineData(1L)]
    public void Deserialize_Enum_Test(object source)
    {
        PNode node = source is string name ? new StringNode(name) : new IntegerNode((long)source);

        // nullable enums take the same path
        Assert.Multiple(() =>
        {
            Assert.Equal(PlistSerializePrimitiveTypesTests.TestEnum.Value2, Deserializer.Deserialize<PlistSerializePrimitiveTypesTests.TestEnum>(node));
            Assert.Equal(PlistSerializePrimitiveTypesTests.TestEnum.Value2, Deserializer.Deserialize<PlistSerializePrimitiveTypesTests.TestEnum?>(node));
        });
    }

    [Fact]
    public void Deserialize_Guid_Test()
    {
        Guid source = Guid.NewGuid();

        var node = new StringNode(source.ToString());
        var res = Deserializer.Deserialize<Guid>(node);
        Assert.IsType<Guid>(res);
        Assert.Equal(source, res);
    }
}
