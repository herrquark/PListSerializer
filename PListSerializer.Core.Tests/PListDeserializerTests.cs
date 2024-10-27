using System.Text;
using PListNet;
using PListNet.Nodes;
using PListSerializer.Core.Attributes;
using PListSerializer.Core.Tests.TestModels;

namespace PListSerializer.Core.Tests;

[TestFixture()]
public class PListDeserializerTests
{
    [TestCase]
    public void Deserialize_BigObject_Tests()
    {
        var node = new DictionaryNode();
        var arrayNode = new ArrayNode();
        var dictionaryNode = new DictionaryNode();
        node.Add("Array0", arrayNode);
        node.Add("Array1", arrayNode);
        node.Add("Array2", arrayNode);
        node.Add("List0", arrayNode);
        node.Add("List1", arrayNode);
        node.Add("List2", arrayNode);

        node.Add("Ints1", arrayNode);

        node.Add("Dictionary0", dictionaryNode);
        node.Add("Dictionary1", dictionaryNode);
        node.Add("Dictionary2", dictionaryNode);
        node.Add("Dictionary3", dictionaryNode);

        node.Add("DictionarySameType", dictionaryNode);
        node.Add("DictionarySameType2", dictionaryNode);
        node.Add("DictionarySameType3", dictionaryNode);

        var res = Deserializer.Deserialize<BigObject>(node);
        Assert.Multiple(() =>
        {
            Assert.That(res.Array0, Is.Not.Null);
            Assert.That(res.Array1, Is.Not.Null);
            Assert.That(res.Array2, Is.Not.Null);

            Assert.That(res.Dictionary0, Is.Not.Null);
            Assert.That(res.Dictionary1, Is.Not.Null);
            Assert.That(res.Dictionary2, Is.Not.Null);
            Assert.That(res.Dictionary3, Is.Not.Null);

            Assert.That(res.Ints1, Is.Not.Null);
            Assert.That(res.List0, Is.Not.Null);
            Assert.That(res.List1, Is.Not.Null);
            Assert.That(res.List2, Is.Not.Null);
        });

    }

    [TestCase]
    public void Deserialize_Class_With_Properties_Same_Test()
    {
        var node = new DictionaryNode();
        var arrayNode = new ArrayNode();
        var dictionaryNode = new DictionaryNode();
        node.Add("ArraySameType", arrayNode);
        node.Add("ArraySameType2", arrayNode);
        node.Add("ArraySameType3", arrayNode);

        node.Add("List1", arrayNode);
        node.Add("List2", arrayNode);
        node.Add("List3", arrayNode);

        node.Add("ClassSameType", dictionaryNode);
        node.Add("ClassSameType2", dictionaryNode);
        node.Add("ClassSameType3", dictionaryNode);

        node.Add("DictionarySameType", dictionaryNode);
        node.Add("DictionarySameType2", dictionaryNode);
        node.Add("DictionarySameType3", dictionaryNode);

        var res = Deserializer.Deserialize<ClassWithSameTypes>(node);
        Assert.Multiple(() =>
        {
            Assert.That(res.ArraySameType, Is.Not.Null);
            Assert.That(res.ArraySameType2, Is.Not.Null);

            Assert.That(res.ClassSameType, Is.Not.Null);
            Assert.That(res.ClassSameType2, Is.Not.Null);
            Assert.That(res.ClassSameType3, Is.Not.Null);

            Assert.That(res.DictionarySameType, Is.Not.Null);
            Assert.That(res.DictionarySameType2, Is.Not.Null);

            Assert.That(res.List1, Is.Not.Null);
            Assert.That(res.List2, Is.Not.Null);
            Assert.That(res.List3, Is.Not.Null);
        });
    }

    [TestCase]
    public void Deserialize_Effect_Test()
    {
        var byteArray = File.ReadAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", "PList2.plist"));
        var stream = new MemoryStream(byteArray);
        var node = PList.Load(stream);
        var r = Deserializer.Deserialize<RootPList>(node);

        Assert.That(r, Is.Not.Null);

        Console.WriteLine(PList.ToString(node));

        Assert.Multiple(() =>
        {
            Assert.That(r.GroupIdentifier, Is.EqualTo("Custom"));
            Assert.That(r.PresetIdentifierKey, Is.EqualTo("Clarity Booster - 2018.lmp"));
            Assert.That(r.Hidden, Is.EqualTo(true));
            Assert.That(r.Id, Is.EqualTo("259F230F-A18A-489C-87FE-024B503E1F5C"));
            Assert.That(r.AdjustmentLayers, Is.Not.Null);

        });

        Assert.That(r.AdjustmentLayers[0], Is.Not.Null);
    }

    [TestCase]
    public void Serialize_EffectsInfo_Test()
    {
        var byteArray = File.ReadAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", "PList3.plist"));
        var stream = new MemoryStream(byteArray);
        var node = PList.Load(stream);
        var r = Deserializer.Deserialize<EffectsPlist>(node);

        Assert.That(r.AdjustmentLayers, Is.Not.Null);
        var adjustmentLayer = r.AdjustmentLayers["DevelopAdjustmentLayer"];
        Assert.That(adjustmentLayer, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(adjustmentLayer.InfoImageName, Is.EqualTo("raw_dev2"));
            Assert.That(adjustmentLayer.Identifier, Is.EqualTo("1"));
            Assert.That(adjustmentLayer.Sublayers, Is.Not.Null);

        });

        Assert.That(adjustmentLayer.Sublayers, Has.Length.EqualTo(4));
        Assert.That(adjustmentLayer.Sublayers[0].EffectsIMG, Is.Not.Null);
    }

    [TestCase]
    public void Deserialize_WithResolver_Test()
    {
        var plist = """
                    <?xml version="1.0" encoding="UTF-8"?>
                    <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
                    <plist version="1.0">
                    <dict>
                        <key>Arr</key>
                        <array>
                            <dict>
                                <key>Type</key>
                                <string>One</string>
                                <key>Name_One</key>
                                <string>This is the one!</string>
                            </dict>
                            <dict>
                                <key>Type</key>
                                <string>Two</string>
                                <key>Name_Two</key>
                                <string>This is the two!</string>
                            </dict>
                            <dict>
                                <key>Type</key>
                                <string>Three</string>
                                <key>Name_Three</key>
                                <string>This is the three!</string>
                            </dict>
                            <dict>
                                <key>Type</key>
                                <string>OHNO!</string>
                                <key>Name_Three</key>
                                <string>This is the three!</string>
                            </dict>
                        </array>
                        <key>Lst</key>
                        <array>
                            <dict>
                                <key>Type</key>
                                <string>One</string>
                                <key>Name_One</key>
                                <string>This is the one!</string>
                            </dict>
                            <dict>
                                <key>Type</key>
                                <string>Two</string>
                                <key>Name_Two</key>
                                <string>This is the two!</string>
                            </dict>
                            <dict>
                                <key>Type</key>
                                <string>Three</string>
                                <key>Name_Three</key>
                                <string>This is the three!</string>
                            </dict>
                            <dict>
                                <key>Type</key>
                                <string>OHNO!</string>
                                <key>Name_Three</key>
                                <string>This is the three!</string>
                            </dict>
                        </array>
                        <key>Cls</key>
                        <dict>
                            <key>Type</key>
                            <string>Three</string>
                            <key>Name_Three</key>
                            <string>This is the three!</string>
                        </dict>
                    </dict>
                    </plist>
                    """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(plist));
        var rootNode = PList.Load(stream);

        var root = Deserializer.Deserialize<ResolverTestClass>(rootNode);

        Assert.That(root, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(root.Arr, Has.Length.EqualTo(4));
            Assert.That(root.Arr[0], Is.InstanceOf<ResolverTestOne>());
            Assert.That(root.Arr[1], Is.InstanceOf<ResolverTestTwo>());
            Assert.That(root.Arr[2], Is.InstanceOf<ResolverTestThree>());
            Assert.That(root.Arr[3], Is.InstanceOf<ResolverTestBaseClass>());
        });


        Assert.Multiple(() =>
        {
            Assert.That(root.Lst, Has.Count.EqualTo(4));
            Assert.That(root.Lst[0], Is.InstanceOf<ResolverTestOne>());
            Assert.That(root.Lst[1], Is.InstanceOf<ResolverTestTwo>());
            Assert.That(root.Lst[2], Is.InstanceOf<ResolverTestThree>());
            Assert.That(root.Lst[3], Is.InstanceOf<ResolverTestBaseClass>());
        });

        Assert.That(root.Cls, Is.InstanceOf<ResolverTestThree>());
    }
}

