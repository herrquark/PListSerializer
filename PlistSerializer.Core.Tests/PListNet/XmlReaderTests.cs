using PlistSerializer.Core.Nodes;

namespace PlistSerializer.Core.Tests;

public class XmlReaderTests
{
    [Fact]
    public void Load_XmlDictionary_Test()
    {
        using var stream = TestFileHelper.GetTestFileStream("TestFiles/asdf-Info.plist");
        var node = Plist.Load(stream);

        Assert.NotNull(node);

        var dictionary = node as DictionaryNode;
        Assert.NotNull(dictionary);

        Assert.Equal(14, dictionary.Count);
    }

    [Fact]
    public void Load_XmlNestedCollections_Test()
    {
        using var stream = TestFileHelper.GetTestFileStream("TestFiles/dict-inside-array.plist");
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
        using var stream = TestFileHelper.GetTestFileStream("TestFiles/Pods-acknowledgements.plist");
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
        using var stream = TestFileHelper.GetTestFileStream("TestFiles/empty-array.plist");
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
        using var stream = TestFileHelper.GetTestFileStream("TestFiles/github-7-xml.plist");
        Assert.NotNull(Plist.Load(stream));
    }
}
