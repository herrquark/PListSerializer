using System.Text;

namespace PlistSerializer.Performance;

internal static class Scenario
{
    public static IEnumerable<BenchmarkCase> Models<T>(string name, Func<int, T> create,
        Action<T, T, PlistFormat?> validate, Action<T, PNode> validateNode = null)
    {
        foreach (var count in new[] { 1, 1000 })
        {
            var source = Enumerable.Range(0, count).Select(create).ToArray();
            var size = count == 1 ? "small" : "large";

            foreach (var benchmark in RoundTrips<T[], T[]>($"{name}/{size}", source, (expected, actual, format) =>
            {
                Check.Equal(expected.Length, actual.Length);
                for (var i = 0; i < expected.Length; i++)
                    validate(expected[i], actual[i], format);
            }, (models, node) =>
            {
                if (validateNode == null)
                    return;

                var array = (Nodes.ArrayNode)node;
                for (var i = 0; i < models.Length; i++)
                    validateNode(models[i], array[i]);
            }))
                yield return benchmark;
        }
    }

    public static IEnumerable<BenchmarkCase> RoundTrips<TSource, TResult>(string name, TSource source,
        Action<TSource, TResult, PlistFormat?> validate,
        Action<TSource, PNode> validateNode = null, PlistFormat[] formats = null)
    {
        var node = Serializer.Serialize(source);
        validateNode?.Invoke(source, node);

        yield return new(name + "/serialize", () => Serializer.Serialize(source),
            result => Check.Nodes(node, (PNode)result));
        yield return new(name + "/deserialize", () => Deserializer.Deserialize<TResult>(node),
            result => validate(source, (TResult)result, null));

        foreach (var format in formats ?? [PlistFormat.Xml, PlistFormat.Binary])
        {
            using var stream = new MemoryStream();
            Plist.Save(node, stream, format);
            var bytes = stream.ToArray();

            if (format == PlistFormat.Xml)
            {
                var xml = Encoding.UTF8.GetString(bytes);
                yield return new(name + "/to-string", () => Plist.ToString(node),
                    result => Check.Equal(xml, (string)result));

                const string start = "<plist version=\"1.0\">\n";
                var fragment = xml[(xml.IndexOf(start, StringComparison.Ordinal) + start.Length)..^"</plist>\n".Length];
                yield return new(name + "/to-string-fragment", () => Plist.ToString(node, false),
                    result => Check.Equal(fragment, (string)result));
            }

            yield return new(name + "/" + format + "/save", () =>
            {
                using var output = new MemoryStream();
                Plist.Save(node, output, format);
                return output.Length;
            }, result => Check.Equal((long)bytes.Length, (long)result));

            yield return new(name + "/" + format + "/load", () =>
            {
                using var input = new MemoryStream(bytes);
                return Plist.Load(input);
            }, result => Check.Nodes(node, (PNode)result, format));

            yield return new(name + "/" + format + "/roundtrip", () =>
            {
                using var output = new MemoryStream();
                Plist.Save(Serializer.Serialize(source), output, format);
                output.Position = 0;
                return Deserializer.Deserialize<TResult>(Plist.Load(output));
            }, result => validate(source, (TResult)result, format));
        }
    }

    public static IEnumerable<BenchmarkCase> Import<T>(string name, PNode node, Action<T> validate,
        PlistFormat[] formats = null)
    {
        yield return new(name + "/deserialize", () => Deserializer.Deserialize<T>(node),
            result => validate((T)result));

        foreach (var format in formats ?? [PlistFormat.Xml, PlistFormat.Binary])
        {
            using var output = new MemoryStream();
            Plist.Save(node, output, format);
            var bytes = output.ToArray();

            yield return new(name + "/" + format + "/load-deserialize", () =>
            {
                using var input = new MemoryStream(bytes);
                return Deserializer.Deserialize<T>(Plist.Load(input));
            }, result => validate((T)result));
        }
    }

    public static BenchmarkCase Rejection<TException>(string name, Func<object> run) where TException : Exception
        => new("rejections/" + name, () =>
        {
            try
            {
                return run();
            }
            catch (Exception error)
            {
                return error;
            }
        }, result => Check.That(result?.GetType() == typeof(TException), $"Expected {typeof(TException).Name}."));
}
