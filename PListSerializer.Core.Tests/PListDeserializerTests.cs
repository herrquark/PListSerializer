using System.Text;
using PListNet;
using PListNet.Nodes;
using PListSerializer.Core.Attributes;
using PListSerializer.Core.Tests.TestModels;

namespace PListSerializer.Core.Tests;

public class PListDeserializerTests
{
    [Fact]
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
            Assert.NotNull(res.Array0);
            Assert.NotNull(res.Array1);
            Assert.NotNull(res.Array2);

            Assert.NotNull(res.Dictionary0);
            Assert.NotNull(res.Dictionary1);
            Assert.NotNull(res.Dictionary2);
            Assert.NotNull(res.Dictionary3);

            Assert.NotNull(res.Ints1);
            Assert.NotNull(res.List0);
            Assert.NotNull(res.List1);
            Assert.NotNull(res.List2);
        });

    }

    [Fact]
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
            Assert.NotNull(res.ArraySameType);
            Assert.NotNull(res.ArraySameType2);

            Assert.NotNull(res.ClassSameType);
            Assert.NotNull(res.ClassSameType2);
            Assert.NotNull(res.ClassSameType3);

            Assert.NotNull(res.DictionarySameType);
            Assert.NotNull(res.DictionarySameType2);

            Assert.NotNull(res.List1);
            Assert.NotNull(res.List2);
            Assert.NotNull(res.List3);
        });
    }

    [Fact]
    public void Deserialize_Effect_Test()
    {
        var byteArray = File.ReadAllBytes(Path.Combine("Resources", "PList2.plist"));
        var stream = new MemoryStream(byteArray);
        var node = PList.Load(stream);
        var r = Deserializer.Deserialize<RootPList>(node);

        Assert.NotNull(r);

        Assert.Multiple(() =>
        {
            Assert.Equal("Custom", r.GroupIdentifier);
            Assert.Equal("Clarity Booster - 2018.lmp", r.PresetIdentifierKey);
            Assert.True(r.Hidden);
            Assert.Equal("259F230F-A18A-489C-87FE-024B503E1F5C", r.Id);
            Assert.NotNull(r.AdjustmentLayers);

        });

        Assert.NotNull(r.AdjustmentLayers[0]);
    }

    [Fact]
    public void Serialize_EffectsInfo_Test()
    {
        var byteArray = File.ReadAllBytes(Path.Combine("Resources", "PList3.plist"));
        var stream = new MemoryStream(byteArray);
        var node = PList.Load(stream);
        var r = Deserializer.Deserialize<EffectsPlist>(node);

        Assert.NotNull(r.AdjustmentLayers);
        var adjustmentLayer = r.AdjustmentLayers["DevelopAdjustmentLayer"];
        Assert.NotNull(adjustmentLayer);

        Assert.Multiple(() =>
        {
            Assert.Equal("raw_dev2", adjustmentLayer.InfoImageName);
            Assert.Equal("1", adjustmentLayer.Identifier);
            Assert.NotNull(adjustmentLayer.Sublayers);

        });

        Assert.Equal(4, adjustmentLayer.Sublayers.Length);
        Assert.NotNull(adjustmentLayer.Sublayers[0].EffectsIMG);
    }

    [Fact]
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

        Assert.NotNull(root);

        Assert.Multiple(() =>
        {
            Assert.Equal(4, root.Arr.Length);
            Assert.IsType<ResolverTestOne>(root.Arr[0]);
            Assert.IsType<ResolverTestTwo>(root.Arr[1]);
            Assert.IsType<ResolverTestThree>(root.Arr[2]);
            Assert.IsType<ResolverTestBaseClass>(root.Arr[3]);
        });


        Assert.Multiple(() =>
        {
            Assert.Equal(4, root.Lst.Count);
            Assert.IsType<ResolverTestOne>(root.Lst[0]);
            Assert.IsType<ResolverTestTwo>(root.Lst[1]);
            Assert.IsType<ResolverTestThree>(root.Lst[2]);
            Assert.IsType<ResolverTestBaseClass>(root.Lst[3]);
        });

        Assert.IsType<ResolverTestThree>(root.Cls);
    }
}

