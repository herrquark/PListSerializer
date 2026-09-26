using PlistSerializer.Nodes;

namespace PlistSerializer.Tests;

public class PlistBinaryReaderTests
{
    [Fact]
    public void Load_BinaryDictionary_Test()
    {
        using var stream = File.OpenRead(Path.Combine("Resources", "asdf-Info-bin.plist"));
        var node = Plist.Load(stream);

        Assert.NotNull(node);

        var dictionary = node as DictionaryNode;
        Assert.NotNull(dictionary);

        Assert.Equal(14, dictionary.Count);
    }

    [Fact]
    public void Load_BinaryUidField_Test()
    {
        using var stream = File.OpenRead(Path.Combine("Resources", "uid-test.plist"));
        Assert.NotNull(Plist.Load(stream));
    }

    [Fact]
    public void Load_BinaryUid_Test()
    {
        using var stream = File.OpenRead(Path.Combine("Resources", "github-7-binary.plist"));
        Assert.NotNull(Plist.Load(stream));
    }

    [Fact]
    public void Load_BinaryUidValue_Test()
    {
        // this binary .plist file came from https://bugs.python.org/issue26707
        using var stream = File.OpenRead(Path.Combine("Resources", "github-7-binary-2.plist"));
        var root = Plist.Load(stream) as DictionaryNode;

        Assert.NotNull(root);
        Assert.Equal(4, root.Count);

        var dict = root["$top"] as DictionaryNode;
        Assert.NotNull(dict);

        var uid = dict["data"] as UidNode;
        Assert.NotNull(uid);

        Assert.Equal(1UL, uid.Value);
    }

    [Fact]
    public void Load_Binary16BitIntegers_Test()
    {
        using var stream = File.OpenRead(Path.Combine("Resources", "unity.binary.plist"));
        Assert.NotNull(Plist.Load(stream));
    }

    [Fact]
    public void Load_BinaryGitHubIssue9_Test()
    {
        using var stream = File.OpenRead(Path.Combine("Resources", "github-9.plist"));
        Assert.NotNull(Plist.Load(stream));
    }

    [Fact]
    public void Load_BinaryMediumDictionary_Test()
    {
        using var stream = File.OpenRead(Path.Combine("Resources", "github-15-medium-binary.plist"));
        var node = Plist.Load(stream);

        var dictNode = node as DictionaryNode;
        Assert.NotNull(dictNode);
        Assert.Equal(16384, dictNode.Keys.Count);
    }

    [Fact]
    public void Load_BinaryLargeDictionary_Test()
    {
        using var stream = File.OpenRead(Path.Combine("Resources", "github-15-large-binary.plist"));
        var node = Plist.Load(stream);

        var dictNode = node as DictionaryNode;
        Assert.NotNull(dictNode);
        Assert.Equal(32768, dictNode.Keys.Count);
    }
}
