using PListNet.Nodes;

namespace PListSerializer.Core.Tests;

public class PListDeserializePrimitiveTypesTests
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
}
