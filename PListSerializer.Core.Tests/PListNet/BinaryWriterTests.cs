using PListNet.Nodes;

namespace PListNet.Tests;

public class BinaryWriterTests
{
    [Fact]
    public void Save_BinaryRoundTrip_Test()
    {
        using var stream = TestFileHelper.GetTestFileStream("TestFiles/asdf-Info.plist");
        var node = PList.Load(stream);

        using var outStream = new MemoryStream();
        PList.Save(node, outStream, PListFormat.Binary);

        // rewind and reload
        outStream.Seek(0, SeekOrigin.Begin);
        var newNode = PList.Load(outStream);

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
