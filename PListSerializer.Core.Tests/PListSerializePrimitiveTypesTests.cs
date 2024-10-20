using PListNet.Nodes;

namespace PListSerializer.Core.Tests;

[TestFixture]
public class PListSerializePrimitiveTypesTests
{
    private Deserializer _deserializer;

    [OneTimeSetUp]
    public void SetUp()
    {
        _deserializer = new Deserializer();
    }

    [TestCase(42)]
    [TestCase(-13423)]
    [TestCase(0)]
    public void Serialize_Int_Test(int source)
    {
        var node = _deserializer.Serialize(source);
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
        var node = _deserializer.Serialize(source);
        Assert.That(node, Is.Not.Null);

        var intNode = node as IntegerNode;
        Assert.That(intNode, Is.Not.Null);

        Assert.That(intNode.Value, Is.EqualTo(source));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Serialize_Bool_Test(bool source)
    {
        var node = _deserializer.Serialize(source);
        Assert.That(node, Is.Not.Null);

        var boolNode = node as BooleanNode;
        Assert.That(boolNode, Is.Not.Null);

        Assert.That(boolNode.Value, Is.EqualTo(source));
    }

    [TestCase("String_42")]
    [TestCase("String_42grtryrthytrytryrt")]
    public void Serialize_String_Test(string source)
    {
        var node = _deserializer.Serialize(source);
        Assert.That(node, Is.Not.Null);

        var stringNode = node as StringNode;
        Assert.That(stringNode, Is.Not.Null);

        Assert.That(stringNode.Value, Is.EqualTo(source));
    }

    [TestCase]
    public void Serialize_Date_Test()
    {
        DateTime source = DateTime.MaxValue;

        var node = _deserializer.Serialize(source);
        Assert.That(node, Is.Not.Null);

        var dateNode = node as DateNode;
        Assert.That(dateNode, Is.Not.Null);

        Assert.That(dateNode.Value, Is.EqualTo(source));
    }
}
