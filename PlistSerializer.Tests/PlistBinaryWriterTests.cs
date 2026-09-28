using System.Buffers.Binary;
using PlistSerializer.Nodes;

namespace PlistSerializer.Tests;

public class PlistBinaryWriterTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(255)]
    [InlineData(256)]
    [InlineData(32768)]
    public void Save_BinaryReferenceWidths_Test(int count)
    {
        var node = new DictionaryNode();
        for (var i = 0; i < count; i++)
            node.Add("key-" + i, new IntegerNode(i));

        using var stream = new MemoryStream();
        Plist.Save(node, stream, PlistFormat.Binary);
        stream.Position = 0;
        var result = Assert.IsType<DictionaryNode>(Plist.Load(stream));

        Assert.Equal(count, result.Count);
        Assert.All(node, pair => Assert.Equal(pair.Value, result[pair.Key]));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(127)]
    [InlineData(128)]
    [InlineData(129)]
    [InlineData(255)]
    [InlineData(256)]
    [InlineData(257)]
    [InlineData(100000)]
    public void Save_BinaryStringBufferBoundaries_Test(int length)
    {
        var strings = new[] { new string('x', length), new string('é', length) };
        var node = Serializer.Serialize(strings);
        using var stream = new MemoryStream();
        Plist.Save(node, stream, PlistFormat.Binary);
        stream.Position = 0;

        Assert.Equal(strings, Deserializer.Deserialize<string[]>(Plist.Load(stream)));
    }

    [Fact]
    public void Save_BinaryEqualBooleans_Test()
    {
        var values = new[] { true, true, false, false, true };
        using var stream = new MemoryStream();
        Plist.Save(Serializer.Serialize(values), stream, PlistFormat.Binary);
        var bytes = stream.ToArray();
        stream.Position = 0;

        Assert.Multiple(() =>
        {
            Assert.Equal(3UL, BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(bytes.Length - 24)));
            Assert.Equal(values, Deserializer.Deserialize<bool[]>(Plist.Load(stream)));
        });
    }

    [Fact]
    public void Save_BinaryRoundTrip_Test()
    {
        using var stream = File.OpenRead(Path.Combine("Resources", "asdf-Info.plist"));
        var node = Plist.Load(stream);

        using var outStream = new MemoryStream();
        Plist.Save(node, outStream, PlistFormat.Binary);

        // rewind and reload
        outStream.Seek(0, SeekOrigin.Begin);
        var newNode = Plist.Load(outStream);

        var oldDict = Assert.IsType<DictionaryNode>(node);
        var newDict = Assert.IsType<DictionaryNode>(newNode);

        Assert.Equal(oldDict.Count, newDict.Count);

        Assert.All(oldDict, pair =>
        {
            var newValue = Assert.Contains(pair.Key, newDict);

            Assert.Multiple(
                () => Assert.IsType(pair.Value.GetType(), newValue),
                () => Assert.Equal(pair.Value, newValue));
        }, throwIfEmpty: true);
    }

    [Theory]
    [InlineData(0UL, "8000")]
    [InlineData(255UL, "80FF")]
    [InlineData(256UL, "810100")]
    [InlineData(65535UL, "81FFFF")]
    [InlineData(65536UL, "8300010000")]
    [InlineData(4294967295UL, "83FFFFFFFF")]
    [InlineData(4294967296UL, "870000000100000000")]
    [InlineData(18446744073709551615UL, "87FFFFFFFFFFFFFFFF")]
    public void Save_BinaryUid_Test(ulong value, string expected)
    {
        using var stream = new MemoryStream();
        Plist.Save(new UidNode(value), stream, PlistFormat.Binary);

        // the root object sits between the 8-byte header and a 1-byte offset table followed by the 32-byte trailer
        var bytes = stream.ToArray();
        stream.Seek(0, SeekOrigin.Begin);
        var uid = Assert.IsType<UidNode>(Plist.Load(stream));

        Assert.Multiple(() =>
        {
            Assert.Equal(expected, Convert.ToHexString(bytes, 8, bytes.Length - 8 - 1 - 32));
            Assert.Equal(value, uid.Value);
        });
    }

    [Theory]
    [InlineData(0L, "1000")]
    [InlineData(255L, "10FF")]
    [InlineData(256L, "110100")]
    [InlineData(40000L, "119C40")]
    [InlineData(65536L, "1200010000")]
    [InlineData(3000000000L, "12B2D05E00")]
    [InlineData(4294967296L, "130000000100000000")]
    [InlineData(-1L, "13FFFFFFFFFFFFFFFF")]
    [InlineData(-40000L, "13FFFFFFFFFFFF63C0")]
    public void Save_BinaryInteger_Test(long value, string expected)
    {
        using var stream = new MemoryStream();
        Plist.Save(new IntegerNode(value), stream, PlistFormat.Binary);

        // the widths plutil writes, where only the 8-byte form is signed
        var bytes = stream.ToArray();
        stream.Seek(0, SeekOrigin.Begin);
        var integer = Assert.IsType<IntegerNode>(Plist.Load(stream));

        Assert.Multiple(() =>
        {
            Assert.Equal(expected, Convert.ToHexString(bytes, 8, bytes.Length - 8 - 1 - 32));
            Assert.Equal(value, integer.Value);
        });
    }

    [Theory]
    [InlineData("ascii")]
    [InlineData("é")]
    [InlineData("a�b")]
    [InlineData("😂test")]
    public void Save_BinaryString_Test(string value)
    {
        using var stream = new MemoryStream();
        Plist.Save(new StringNode(value), stream, PlistFormat.Binary);

        stream.Seek(0, SeekOrigin.Begin);
        Assert.Equal(value, Assert.IsType<StringNode>(Plist.Load(stream)).Value);
    }

    [Fact]
    public void Save_BinaryLocalDate_Test()
    {
        // a local time is only off by its UTC offset where that offset is not zero
        var utc = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        using var stream = new MemoryStream();
        Plist.Save(new DateNode(utc.ToLocalTime()), stream, PlistFormat.Binary);

        stream.Seek(0, SeekOrigin.Begin);
        Assert.Equal(utc, Assert.IsType<DateNode>(Plist.Load(stream)).Value);
    }

    [Fact]
    public void Save_BinaryEqualUids_Test()
    {
        var node = new ArrayNode { new UidNode(7), new UidNode(7), new IntegerNode(7), new IntegerNode(7) };

        using var stream = new MemoryStream();
        Plist.Save(node, stream, PlistFormat.Binary);

        // the trailer's object count: the array, both UIDs, and the equal integers merged into one
        var bytes = stream.ToArray();
        Assert.Equal(4UL, BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(bytes.Length - 24)));
    }
}
