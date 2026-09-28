using System.Collections;
using System.Dynamic;
using PlistSerializer.Nodes;
using PlistSerializer.Performance.Models;

namespace PlistSerializer.Performance;

internal static class EdgeBenchmarks
{
    public static IEnumerable<BenchmarkCase> Create()
    {
        var groups = new[]
        {
            DictionaryInputs(),
            Conversions(),
            Widths(),
            DepthAndData(),
            Rejections()
        };

        foreach (var group in groups)
            foreach (var benchmark in group)
                yield return benchmark;
    }

    private static IEnumerable<BenchmarkCase> DictionaryInputs()
    {
        IDictionary legacy = new Hashtable { [7] = "seven", ["present"] = 42L, ["missing"] = null };
        var pairs = Enumerable.Range(0, 3)
            .Select(i => new KeyValuePair<string, object>("key-" + i, i == 1 ? null : (long)i));
        IDictionary<string, object> expando = new ExpandoObject();
        expando["number"] = 42L;
        expando["missing"] = null;

        var inputs = new (string Name, object Source, Dictionary<string, object> Expected)[]
        {
            ("legacy-dictionary", legacy, new() { ["7"] = "seven", ["present"] = 42L }),
            ("pair-enumerable", pairs, new() { ["key-0"] = 0L, ["key-2"] = 2L }),
            ("expando", expando, new() { ["number"] = 42L }),
            ("legacy-list", new ArrayList { 1L, null, "text", false }, null)
        };

        foreach (var (name, source, expected) in inputs)
        {
            foreach (var benchmark in Scenario.RoundTrips<object, object>(name, source, (_, result, format) =>
            {
                if (expected == null)
                {
                    Check.Equal(typeof(List<object>), result.GetType());
                    Check.Values(new object[] { 1L, "text", false }, result, format);
                }
                else
                {
                    Check.Equal(typeof(Dictionary<string, object>), result.GetType());
                    Check.Values(expected, result, format);
                }
            }))
                yield return benchmark;
        }
    }

    private static IEnumerable<BenchmarkCase> Conversions()
    {
        var node = new DictionaryNode
        {
            ["Integer"] = new StringNode("42"),
            ["Real"] = new StringNode("1.25"),
            ["Boolean"] = new IntegerNode(1),
            ["NamedEnum"] = new StringNode("read, write"),
            ["NumericEnum"] = new IntegerNode((long)Access.High),
            ["NullableEnum"] = new IntegerNode(2),
            ["UnsignedUid"] = new UidNode(uint.MaxValue),
            ["SignedUid"] = new UidNode(7),
            ["NullableUid"] = new UidNode(8),
            ["BoxedUid"] = new UidNode(9),
            ["MismatchedNode"] = new StringNode("not an integer node"),
            ["InvalidGuid"] = new StringNode("invalid"),
            ["InvalidDuration"] = new StringNode("invalid"),
            ["Custom"] = new StringNode("123"),
            ["IgnoredField"] = new IntegerNode(100),
            ["unknown_key"] = new ArrayNode { new StringNode("ignored") }
        };

        foreach (var benchmark in Scenario.Import<ConversionModel>("imported-conversions", node, result =>
        {
            Check.Equal(42, result.Integer);
            Check.Equal(1.25, result.Real);
            Check.Equal(true, result.Boolean);
            Check.Equal(Access.Read | Access.Write, result.NamedEnum);
            Check.Equal(Access.High, result.NumericEnum);
            Check.Equal<Access?>(Access.Write, result.NullableEnum);
            Check.Equal((ulong)uint.MaxValue, result.UnsignedUid);
            Check.Equal(7, result.SignedUid);
            Check.Equal<ulong?>(8UL, result.NullableUid);
            Check.Equal<object>(9UL, result.BoxedUid);
            Check.That(result.MismatchedNode == null && result.InvalidGuid == null && result.InvalidDuration == null,
                "Invalid values should become null.");
            Check.Equal(new Code(123), result.Custom);
            Check.Equal(99, result.Missing);
            Check.Equal(-7, result.IgnoredField);
        }))
            yield return benchmark;

        var collections = new DictionaryNode
        {
            ["Queue"] = new ArrayNode { new IntegerNode(1) },
            ["SortedDictionary"] = new DictionaryNode { ["value"] = new IntegerNode(2) }
        };

        foreach (var benchmark in Scenario.Import<UnsupportedCollectionModel>("unsupported-collections", collections, result =>
        {
            Check.That(result.Queue == null, "Unsupported array collection should be null.");
            Check.Equal(0, result.SortedDictionary.Count);
        }))
            yield return benchmark;

        foreach (var benchmark in Scenario.Import<object>("untyped-null", new NullNode(),
            result => Check.That(result == null, "Untyped null should be null."), [PlistFormat.Binary]))
            yield return benchmark;
    }

    private static IEnumerable<BenchmarkCase> Widths()
    {
        var integers = new long[]
        {
            long.MinValue, -1, 0, 255, 256, 65535, 65536, uint.MaxValue, (long)uint.MaxValue + 1, long.MaxValue
        };
        foreach (var benchmark in Scenario.RoundTrips<long[], long[]>("integer-widths", integers,
            (source, result, _) => Check.Sequence(source, result)))
            yield return benchmark;

        var reals = new[]
        {
            0.0, -0.0, double.Epsilon, double.MinValue, double.MaxValue,
            double.NaN, double.PositiveInfinity, double.NegativeInfinity
        };
        foreach (var benchmark in Scenario.RoundTrips<double[], double[]>("real-special-values", reals,
            (source, result, _) => Check.Sequence(source, result)))
            yield return benchmark;

        var uids = new ulong[] { 0, 255, 256, 65535, 65536, uint.MaxValue, (ulong)uint.MaxValue + 1, long.MaxValue };
        var nodes = new ArrayNode();
        foreach (var uid in uids)
            nodes.Add(new UidNode(uid));

        foreach (var benchmark in Scenario.RoundTrips<PNode, ulong[]>("uid-widths", nodes,
            (_, result, _) => Check.Sequence(uids, result)))
            yield return benchmark;

        var binaryOnly = new ArrayNode { new NullNode(), new FillNode(), new UidNode(ulong.MaxValue) };
        foreach (var benchmark in Scenario.RoundTrips<PNode, PNode>("binary-only-nodes", binaryOnly,
            (source, result, format) => Check.Nodes(source, result, format), formats: [PlistFormat.Binary]))
            yield return benchmark;

        foreach (var count in new[] { 14, 15, 127, 128, 255, 256, 32768 })
        {
            var dictionary = Enumerable.Range(0, count).ToDictionary(i => "key-" + i, i => i);
            foreach (var benchmark in Scenario.RoundTrips<Dictionary<string, int>, Dictionary<string, int>>(
                "reference-widths/" + count, dictionary, Check.Values))
                yield return benchmark;
        }
    }

    private static IEnumerable<BenchmarkCase> DepthAndData()
    {
        foreach (var depth in new[] { 16, 128, 512 })
        {
            var model = new DeepModel();
            for (var i = 1; i < depth; i++)
                model = new DeepModel { Child = model };

            foreach (var benchmark in Scenario.RoundTrips<DeepModel, DeepModel>("nesting/" + depth, model, (_, result, _) =>
            {
                var actualDepth = 0;
                for (var item = result; item != null; item = item.Child)
                    actualDepth++;
                Check.Equal(depth, actualDepth);
            }))
                yield return benchmark;
        }

        var data = new byte[1024 * 1024];
        Array.Fill(data, (byte)123);
        foreach (var benchmark in Scenario.RoundTrips<byte[], byte[]>("large-data/1MiB", data,
            (source, result, _) => Check.Sequence(source, result)))
            yield return benchmark;

        foreach (var benchmark in Scenario.RoundTrips<string, string>("root-string", "é😂<&>",
            (source, result, _) => Check.Equal(source, result)))
            yield return benchmark;

        foreach (var benchmark in Scenario.RoundTrips<int, int>("root-integer", 42,
            (source, result, _) => Check.Equal(source, result)))
            yield return benchmark;
    }

    private static IEnumerable<BenchmarkCase> Rejections()
    {
        yield return Scenario.Rejection<PlistFormatException>("serialize-conflicting-keys",
            () => Serializer.Serialize(new ConflictingModel()));
        yield return Scenario.Rejection<PlistFormatException>("deserialize-conflicting-keys",
            () => Deserializer.Deserialize<ConflictingModel>(new DictionaryNode()));
        yield return Scenario.Rejection<ArgumentException>("non-string-dictionary-key",
            () => Deserializer.Deserialize<Dictionary<int, int>>(new DictionaryNode { ["1"] = new IntegerNode(1) }));
        yield return Scenario.Rejection<NullReferenceException>("mismatched-value-type",
            () => Deserializer.Deserialize<int>(new ArrayNode()));
        yield return Scenario.Rejection<ArgumentException>("invalid-enum",
            () => Deserializer.Deserialize<Access>(new StringNode("invalid")));
        yield return Scenario.Rejection<OverflowException>("integer-overflow",
            () => Deserializer.Deserialize<int>(new IntegerNode(long.MaxValue)));
        yield return Scenario.Rejection<NullReferenceException>("null-root",
            () => Serializer.Serialize(null));
        yield return Scenario.Rejection<PlistFormatException>("xml-null",
            () => Plist.ToString(new NullNode()));
        yield return Scenario.Rejection<PlistFormatException>("xml-fill",
            () => Plist.ToString(new FillNode()));

        var cycle = new DeepModel();
        cycle.Child = cycle;
        yield return Scenario.Rejection<PlistFormatException>("object-cycle", () => Serializer.Serialize(cycle));

        var listCycle = new List<object>();
        listCycle.Add(listCycle);
        yield return Scenario.Rejection<PlistFormatException>("collection-cycle", () => Serializer.Serialize(listCycle));

        PNode tooDeep = new ArrayNode();
        for (var i = 1; i < 513; i++)
            tooDeep = new ArrayNode { tooDeep };

        foreach (var format in new[] { PlistFormat.Xml, PlistFormat.Binary })
        {
            using var output = new MemoryStream();
            Plist.Save(tooDeep, output, format);
            var bytes = output.ToArray();
            yield return Scenario.Rejection<PlistFormatException>(format + "-nesting-limit", () =>
            {
                using var input = new MemoryStream(bytes);
                return Plist.Load(input);
            });
        }
    }
}
