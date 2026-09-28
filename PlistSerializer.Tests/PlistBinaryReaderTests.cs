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

    [Theory]
    [InlineData("8005", 5UL)]
    [InlineData("810100", 256UL)]
    [InlineData("82010000", 65536UL)]
    [InlineData("8300010000", 65536UL)]
    [InlineData("83FFFFFFFF", 4294967295UL)]
    [InlineData("840100000000", 4294967296UL)]
    [InlineData("870000000000000007", 7UL)]
    [InlineData("8F00000000000000000000000000000009", 9UL)]
    [InlineData("8F0000000000000000FFFFFFFFFFFFFFFF", 18446744073709551615UL)]
    public void Load_BinaryUidWidths_Test(string uid, ulong expected)
    {
        using var stream = new MemoryStream(WrapInBinaryPlist(Convert.FromHexString(uid)));
        var node = Assert.IsType<UidNode>(Plist.Load(stream));

        Assert.Equal(expected, node.Value);
    }

    [Fact]
    public void Load_BinaryUidOverflow_Test()
    {
        // a 9-byte UID whose leading byte does not fit in 64 bits
        using var stream = new MemoryStream(WrapInBinaryPlist(Convert.FromHexString("88010000000000000000")));

        Assert.Throws<PlistFormatException>(() => Plist.Load(stream));
    }

    [Fact]
    public void Load_BinaryAppleUids_Test()
    {
        // written by plutil, which stores UIDs in 1, 2 or 4 bytes and never merges equal ones
        using var stream = File.OpenRead(Path.Combine("Resources", "uid-widths.plist"));
        var root = Assert.IsType<DictionaryNode>(Plist.Load(stream));
        var list = Assert.IsType<ArrayNode>(root["list"]);

        Assert.Multiple(() =>
        {
            Assert.Equal(1UL, Assert.IsType<UidNode>(root["byte"]).Value);
            Assert.Equal(256UL, Assert.IsType<UidNode>(root["short"]).Value);
            Assert.Equal(65536UL, Assert.IsType<UidNode>(root["int"]).Value);
            Assert.Equal(4294967295UL, Assert.IsType<UidNode>(root["max"]).Value);
            Assert.Equal(0UL, Assert.IsType<UidNode>(list[0]).Value);
            Assert.Equal(0UL, Assert.IsType<UidNode>(list[1]).Value);
            Assert.Equal(0L, Assert.IsType<IntegerNode>(list[2]).Value);
        });
    }

    [Theory]
    [InlineData("10C8", 200L)]
    [InlineData("119C40", 40000L)]
    [InlineData("12B2D05E00", 3000000000L)]
    [InlineData("13FFFFFFFFFFFFFFFF", -1L)]
    public void Load_BinaryIntegerWidths_Test(string integer, long expected)
    {
        // plutil writes these widths, where 1, 2 and 4-byte integers are unsigned and only 8-byte ones are signed
        using var stream = new MemoryStream(WrapInBinaryPlist(Convert.FromHexString(integer)));
        var node = Assert.IsType<IntegerNode>(Plist.Load(stream));

        Assert.Equal(expected, node.Value);
    }

    [Fact]
    public void Load_BinaryCycle_Test()
    {
        // an array whose only item is the array itself
        using var stream = new MemoryStream(WrapInBinaryPlist(Convert.FromHexString("A100")));

        Assert.Throws<PlistFormatException>(() => Plist.Load(stream));
    }

    // wraps a single object in the header, a one-entry offset table and the trailer of a binary plist
    private static byte[] WrapInBinaryPlist(byte[] rootObject)
    {
        var header = "bplist00"u8.ToArray();
        var trailer = new byte[32];
        trailer[6] = 1; // offset size
        trailer[7] = 1; // object reference size
        trailer[15] = 1; // object count
        trailer[31] = (byte)(header.Length + rootObject.Length); // offset table position

        return [.. header, .. rootObject, (byte)header.Length, .. trailer];
    }
}
