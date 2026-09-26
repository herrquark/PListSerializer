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
}
