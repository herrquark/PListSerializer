using System.Globalization;
using System.Text;
using PlistSerializer.Nodes;

namespace PlistSerializer.Tests;

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
    [InlineData("uid-widths.xml.plist")]
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

    [Fact]
    public void ToString_Date_Test()
    {
        // Apple and Python reject fractional seconds, and the Thai culture counts years in the Buddhist era
        var node = new DateNode(new DateTime(2020, 1, 1, 0, 0, 0, 500, DateTimeKind.Utc));
        var culture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("th-TH");
            Assert.Equal("<date>2020-01-01T00:00:00Z</date>\n", Plist.ToString(node, writePlistMeta: false));
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Fact]
    public void ToString_NullAndFill_Test()
    {
        // Python writes None to binary plists only, and Apple reads nulls from binary only
        using var binary = new MemoryStream();
        Plist.Save(new ArrayNode { new NullNode() }, binary, PlistFormat.Binary);
        binary.Seek(0, SeekOrigin.Begin);

        Assert.Multiple(() =>
        {
            Assert.IsType<NullNode>(Assert.IsType<ArrayNode>(Plist.Load(binary))[0]);
            Assert.Throws<PlistFormatException>(() => Plist.ToString(new ArrayNode { new NullNode() }));
            Assert.Throws<PlistFormatException>(() => Plist.ToString(new ArrayNode { new FillNode() }));
        });
    }

    [Fact]
    public void ToString_Uid_Test()
    {
        var node = new DictionaryNode { ["a"] = new UidNode(256) };

        // the layout plutil writes, since XML has no UID element
        Assert.Equal(
            "<dict>\n\t<key>a</key>\n\t<dict>\n\t\t<key>CF$UID</key>\n\t\t<integer>256</integer>\n\t</dict>\n</dict>\n",
            Plist.ToString(node, writePlistMeta: false));
    }
}
