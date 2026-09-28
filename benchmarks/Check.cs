using System.Collections;
using PlistSerializer.Nodes;

namespace PlistSerializer.Performance;

internal static class Check
{
    public static void That(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    public static void Equal<T>(T expected, T actual)
        => That(EqualityComparer<T>.Default.Equals(expected, actual), $"Expected {expected}, got {actual}.");

    public static void Sequence<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        => That(actual != null && expected.SequenceEqual(actual), "Sequences differ.");

    public static DateTime Date(DateTime value, PlistFormat? format)
    {
        if (format == null)
            return value;

        var utc = value.ToUniversalTime();
        return format == PlistFormat.Xml
            ? new DateTime(utc.Ticks - utc.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc)
            : utc;
    }

    public static void Nodes(PNode expected, PNode actual, PlistFormat? format = null)
    {
        Equal(expected.GetType(), actual?.GetType());

        switch (expected)
        {
            case DictionaryNode dictionary:
                var result = (DictionaryNode)actual;
                Sequence(dictionary.Keys, result.Keys);
                foreach (var pair in dictionary)
                    Nodes(pair.Value, result[pair.Key], format);
                break;
            case ArrayNode array:
                var items = (ArrayNode)actual;
                Equal(array.Count, items.Count);
                for (var i = 0; i < array.Count; i++)
                    Nodes(array[i], items[i], format);
                break;
            case DataNode data:
                Sequence(data.Value, ((DataNode)actual).Value);
                break;
            case DateNode date:
                Equal(Date(date.Value, format), ((DateNode)actual).Value);
                break;
            case NullNode or FillNode:
                break;
            default:
                Equal(expected, actual);
                break;
        }
    }

    // Used only during validation, never inside a timed operation.
    public static void Values(object expected, object actual, PlistFormat? format)
    {
        if (expected == null)
        {
            That(actual == null, "Expected null.");
            return;
        }

        That(actual != null, $"Missing {expected.GetType().Name} value.");

        switch (expected)
        {
            case DateTime date:
                Equal(Date(date, format), (DateTime)actual);
                break;
            case DateTimeOffset offset:
                var result = (DateTimeOffset)actual;
                Equal(new DateTimeOffset(Date(offset.UtcDateTime, format)), result);
                Equal(TimeSpan.Zero, result.Offset);
                break;
            case decimal number:
                Equal(Convert.ToDecimal(decimal.ToDouble(number)), (decimal)actual);
                break;
            case byte[] bytes:
                Sequence(bytes, (byte[])actual);
                break;
            case PNode node:
                Nodes(node, (PNode)actual, format);
                break;
            case ISet<string> set:
                That(((ISet<string>)actual).SetEquals(set), "Sets differ.");
                break;
            case IDictionary dictionary:
                var output = (IDictionary)actual;
                Equal(dictionary.Values.Cast<object>().Count(value => value != null), output.Count);
                foreach (DictionaryEntry pair in dictionary)
                    if (pair.Value != null)
                        Values(pair.Value, output[pair.Key], format);
                break;
            case IEnumerable sequence when expected is not string:
                var left = sequence.Cast<object>().Where(value => value != null).ToArray();
                var right = ((IEnumerable)actual).Cast<object>().ToArray();
                Equal(left.Length, right.Length);
                for (var i = 0; i < left.Length; i++)
                    Values(left[i], right[i], format);
                break;
            default:
                Equal(expected, actual);
                break;
        }
    }

    public static void Properties<T>(T expected, T actual, PlistFormat? format)
    {
        That(actual != null, $"Missing {typeof(T).Name} model.");
        foreach (var property in typeof(T).GetProperties())
            Values(property.GetValue(expected), property.GetValue(actual), format);
    }
}
