using System.Buffers.Binary;
using PlistSerializer.Nodes;

namespace PlistSerializer.Tests;

public class PlistBinaryWriterTests
{
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

        // compare
        Assert.Equal(node.GetType().Name, newNode.GetType().Name);

        var oldDict = node as DictionaryNode;
        var newDict = newNode as DictionaryNode;

        Assert.Equal(oldDict.Count, newDict.Count);

        foreach (var key in oldDict.Keys)
        {
            Assert.Contains(key, newDict);

            var oldValue = oldDict[key];
            var newValue = newDict[key];

            Assert.Multiple(() =>
            {
                Assert.Equal(oldValue.GetType().Name, newValue.GetType().Name);
                Assert.Equal(oldValue, newValue);
            });
        }
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
