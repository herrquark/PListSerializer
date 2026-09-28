using System.Text;
using PlistSerializer.Nodes;

namespace PlistSerializer.Tests;

public class PlistXmlReaderTests
{
    [Fact]
    public void Load_XmlDictionary_Test()
    {
        using var stream = File.OpenRead(Path.Combine("Resources", "asdf-Info.plist"));
        var node = Plist.Load(stream);

        Assert.NotNull(node);

        var dictionary = node as DictionaryNode;
        Assert.NotNull(dictionary);

        Assert.Equal(14, dictionary.Count);
    }

    [Fact]
    public void Load_XmlNestedCollections_Test()
    {
        using var stream = File.OpenRead(Path.Combine("Resources", "dict-inside-array.plist"));
        var node = Plist.Load(stream);

        Assert.NotNull(node);
        Assert.IsType<DictionaryNode>(node);

        var array = ((DictionaryNode)node).Values.First() as ArrayNode;
        Assert.NotNull(array);
        Assert.Single(array);

        var dictionary = array[0] as DictionaryNode;
        Assert.NotNull(dictionary);

        Assert.Equal(4, dictionary.Count);
    }

    [Fact]
    public void Load_XmlNestedCollectionsWithComplexText_Test()
    {
        using var stream = File.OpenRead(Path.Combine("Resources", "Pods-acknowledgements.plist"));
        var root = Plist.Load(stream) as DictionaryNode;

        Assert.NotNull(root);
        Assert.Equal(3, root.Count);

        Assert.Multiple(() =>
        {
            Assert.IsType<StringNode>(root["StringsTable"]);
            Assert.IsType<StringNode>(root["Title"]);
        });

        var array = root["PreferenceSpecifiers"] as ArrayNode;
        Assert.NotNull(array);
        Assert.Equal(15, array.Count);

        foreach (var node in array)
        {
            Assert.IsType<DictionaryNode>(node);

            var dictionary = (DictionaryNode)node;
            Assert.Equal(3, dictionary.Count);
        }
    }

    [Fact]
    public void Load_XmlEmptyArray_Test()
    {
        using var stream = File.OpenRead(Path.Combine("Resources", "empty-array.plist"));
        var root = Plist.Load(stream) as DictionaryNode;

        Assert.NotNull(root);
        Assert.Single(root);

        Assert.IsType<DictionaryNode>(root["Entitlements"]);
        var dict = root["Entitlements"] as DictionaryNode;

        var array = dict["com.apple.developer.icloud-container-identifiers"] as ArrayNode;
        Assert.NotNull(array);
        Assert.Empty(array);
    }

    [Fact]
    public void Load_XmlUid_Test()
    {
        using var stream = File.OpenRead(Path.Combine("Resources", "github-7-xml.plist"));
        Assert.NotNull(Plist.Load(stream));
    }

    [Fact]
    public void Load_XmlCfUid_Test()
    {
        var root = Assert.IsType<DictionaryNode>(LoadXml("""
            <dict>
                <key>a</key>
                <dict><key>CF$UID</key><integer>7</integer></dict>
                <key>b</key>
                <array><dict><key>CF$UID</key><integer>8</integer></dict></array>
            </dict>
            """));
        var top = Assert.IsType<UidNode>(LoadXml("<dict><key>CF$UID</key><integer>9</integer></dict>"));

        Assert.Multiple(() =>
        {
            Assert.Equal(7UL, Assert.IsType<UidNode>(root["a"]).Value);
            Assert.Equal(8UL, Assert.IsType<UidNode>(Assert.IsType<ArrayNode>(root["b"])[0]).Value);
            Assert.Equal(9UL, top.Value);
        });
    }

    [Theory]
    [InlineData("<dict><key>CF$UID</key><integer>7</integer><key>b</key><integer>1</integer></dict>")]
    [InlineData("<dict><key>CF$UID</key><string>7</string></dict>")]
    [InlineData("<dict><key>CF$UID</key><real>7</real></dict>")]
    [InlineData("<dict><key>CF$UID</key><integer>-1</integer></dict>")]
    [InlineData("<dict><key>CF$UIDx</key><integer>7</integer></dict>")]
    [InlineData("<dict><key>cf$uid</key><integer>7</integer></dict>")]
    public void Load_XmlCfUidLookalike_Test(string dict)
        => Assert.IsType<DictionaryNode>(LoadXml(dict));

    [Fact]
    public void Load_XmlUidElement_Test()
        => Assert.Throws<PlistFormatException>(() => LoadXml("<uid>7</uid>"));

    [Fact]
    public void Load_XmlUidsMatchBinary_Test()
    {
        // both files were written by plutil from the same source
        using var xml = File.OpenRead(Path.Combine("Resources", "uid-widths.xml.plist"));
        using var binary = File.OpenRead(Path.Combine("Resources", "uid-widths.plist"));
        var fromXml = Assert.IsType<DictionaryNode>(Plist.Load(xml));
        var fromBinary = Assert.IsType<DictionaryNode>(Plist.Load(binary));

        // plutil sorts the keys of XML dicts only, so compare by key
        Assert.Equal(fromBinary.Keys.Order(), fromXml.Keys.Order());
        foreach (var key in fromXml.Keys)
            Assert.Equal(fromBinary[key], fromXml[key]);
    }

    private static PNode LoadXml(string node)
        => Plist.Load(new MemoryStream(Encoding.UTF8.GetBytes($"<plist version=\"1.0\">{node}</plist>")));
}
