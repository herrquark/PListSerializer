using System.Text;
using PlistSerializer.Core.Nodes;

namespace PlistSerializer.Core.Tests;

public class PlistXmlWriterTests
{
    [Fact]
    public void Save_XmlRoundTrip_Test()
    {
        using var stream = File.OpenRead(Path.Combine("Resources", "utf8-Info.plist"));

        var node = Plist.Load(stream);

        using var outStream = new MemoryStream();
        Plist.Save(node, outStream, PlistFormat.Xml);

        // rewind and reload
        outStream.Seek(0, SeekOrigin.Begin);
        var newNode = Plist.Load(outStream);

        // compare
        Assert.Equal(node.GetType().Name, newNode.GetType().Name);

        var oldDict = node as DictionaryNode;
        var newDict = newNode as DictionaryNode;

        Assert.NotNull(oldDict);
        Assert.NotNull(newDict);
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

    [Fact]
    public void Save_XmlBoolean_Test()
    {
        using var outStream = new MemoryStream();
        // create a basic plist containing a boolean value
        var node = new DictionaryNode { { "Test", new BooleanNode(true) } };

        // save and reset stream
        Plist.Save(node, outStream, PlistFormat.Xml);
        outStream.Seek(0, SeekOrigin.Begin);

        // check that boolean was written out without a space per spec (see also issue #11)
        using var reader = new StreamReader(outStream);
        var contents = reader.ReadToEnd();

        Assert.Contains("<true/>", contents);
    }

    [Fact]
    public void Save_XmlBooleanWhitespace_Test()
    {
        using var stream = File.OpenRead(Path.Combine("Resources", "github-20.plist"));

        // read in the source file and reset the stream so we can parse from it
        using var plistReader = new StreamReader(stream, Encoding.Default, true, 2048, true);
        var source = plistReader.ReadToEnd();

        stream.Seek(0, SeekOrigin.Begin);

        var root = Plist.Load(stream) as DictionaryNode;
        Assert.NotNull(root);

        // verify that we parsed expected content
        var node = root["ABool"] as BooleanNode;
        Assert.NotNull(node);
        Assert.True(node.Value);

        // write the file out to memory and check that there is still no space
        // in the written out boolean node
        using var outStream = new MemoryStream();
        // save and reset stream
        Plist.Save(root, outStream, PlistFormat.Xml);
        outStream.Seek(0, SeekOrigin.Begin);

        // check that boolean was written out without a space per spec (see also issue #11)
        using var outReader = new StreamReader(outStream);
        var contents = outReader.ReadToEnd();

        Assert.Equal(source, contents);
    }

    [Fact]
    public void Save_XmlUnicodeString_Test()
    {
        using var outStream = new MemoryStream();
        var utf16value = "😂test";

        // create a basic plist containing a string value
        var node = new DictionaryNode { ["Test"] = new StringNode(utf16value) };

        // save and reset stream
        Plist.Save(node, outStream, PlistFormat.Xml);
        outStream.Seek(0, SeekOrigin.Begin);

        // check that the string was written out inside a string tag
        using var reader = new StreamReader(outStream);
        var contents = reader.ReadToEnd();

        Assert.Contains($"<string>{utf16value}</string>", contents);
    }

    [Fact]
    public void ToString_WithoutPlistMeta_Test()
    {
        var node = new BooleanNode(true);

        var str = Plist.ToString(node, writePlistMeta: false);

        Assert.Multiple(() =>
        {
            Assert.DoesNotContain("<?xml version=\"1.0\" encoding=\"utf-8\"?>", str);
            Assert.Contains("<true/>", str);
        });
    }

    [Theory]
    [InlineData("asdf-Info.plist")]
    [InlineData("unity.xml.plist")]
    [InlineData("uid-test.xml.plist")]
    [InlineData("utf8-Info.plist")]
    [InlineData("github-7-xml.plist")]
    [InlineData("github-15-large-xml.plist")]
    public void ToString_SourceXml_Test(string fileName)
    {
        using var stream = File.OpenRead(Path.Combine("Resources", fileName));

        // read in the source file and reset the stream so we can parse from it
        using var plistReader = new StreamReader(stream, Encoding.Default, true, 2048, true);
        var source = plistReader.ReadToEnd();

        stream.Seek(0, SeekOrigin.Begin);

        var root = Plist.Load(stream) as DictionaryNode;
        var serialized = Plist.ToString(root);

        Assert.Equal(source, serialized);
    }
}
