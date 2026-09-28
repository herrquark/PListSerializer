using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using PlistSerializer.Extensions;
using PlistSerializer.Nodes;

namespace PlistSerializer;

/// <summary>
/// Maps .NET objects to plist nodes.
/// </summary>
public static class Serializer
{
    private static readonly ConcurrentDictionary<Type, GetterMember[]> MembersCache = [];

    /// <summary>
    /// Serializes an object into a plist node.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>The node representing <paramref name="obj"/>.</returns>
    public static PNode Serialize(object obj)
        => Serialize(obj, 1);

    // depth counts the node being built, so that a reference cycle throws instead of overflowing the stack
    private static PNode Serialize(object obj, int depth)
    {
        if (depth > Plist.MaxDepth)
            throw new PlistFormatException($"Objects nest deeper than {Plist.MaxDepth} levels at a {obj.GetType()}, which may be a reference cycle.");

        return obj switch
        {
            PNode node => node,
            bool b => new BooleanNode(b),
            int i => new IntegerNode(i),
            long l => new IntegerNode(l),
            string s => new StringNode(s),
            float f => new RealNode(f),
            double d => new RealNode(d),
            decimal dec => new RealNode(decimal.ToDouble(dec)),
            DateTime dt => new DateNode(dt),
            DateTimeOffset dto => new DateNode(dto.UtcDateTime),
            Enum en => new StringNode(en.ToString()),
            Guid g => new StringNode(g.ToString()),
            TimeSpan ts => new StringNode(ts.ToString()),
            Uri uri => new StringNode(uri.OriginalString),
            _ when obj.GetType().IsPrimitive => new StringNode(Convert.ToString(obj, CultureInfo.InvariantCulture)),
            byte[] bytes => new DataNode(bytes),
            IDictionary dict => SerializeDictionary(dict, depth),
            IEnumerable<KeyValuePair<string, object>> dict => SerializeDictionary(dict, depth),
            IEnumerable enumerable => SerializeEnumerable(enumerable, depth),
            _ => SerializeComplexType(obj, depth)
        };
    }

    private static PNode SerializeComplexType(object obj, int depth)
    {
        var dictNode = new DictionaryNode();

        foreach (var member in GetMembers(obj.GetType()))
        {
            var value = member.Get(obj);
            if (value != null && (member.DefaultValue == null || !value.Equals(member.DefaultValue)))
                dictNode.Add(member.Name, Serialize(value, depth + 1));
        }

        return dictNode;
    }

    private static PNode SerializeDictionary(IDictionary dict, int depth)
    {
        var dictNode = new DictionaryNode();

        foreach (var key in dict.Keys)
        {
            if (dict[key] is null)
                continue;

            dictNode.Add(key.ToString(), Serialize(dict[key], depth + 1));
        }

        return dictNode;
    }

    private static PNode SerializeDictionary(IEnumerable<KeyValuePair<string, object>> pairs, int depth)
    {
        var dictNode = new DictionaryNode();

        foreach (var (key, value) in pairs)
        {
            if (value is null)
                continue;

            dictNode.Add(key, Serialize(value, depth + 1));
        }

        return dictNode;
    }

    private static PNode SerializeEnumerable(IEnumerable list, int depth)
    {
        var node = new ArrayNode();
        node.AddRange(list.Cast<object>().Where(x => x is not null).Select(x => Serialize(x, depth + 1)));
        return node;
    }

    private static GetterMember[] GetMembers(Type type)
    {
        if (MembersCache.TryGetValue(type, out var members))
            return members;

        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.IsPlistMember());

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);

        members = [.. props.Concat<MemberInfo>(fields)
            .ResolvePlistKeys()
            .Select(BuildGetterMember)
            .OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase)];

        MembersCache[type] = members;

        return members;
    }

    private static GetterMember BuildGetterMember(MemberInfo m)
        => new()
        {
            Name = m.GetName(),
            Get = m is PropertyInfo p ? p.GetValue : ((FieldInfo)m).GetValue,
            DefaultValue = m.GetCustomAttribute<DefaultValueAttribute>(false)?.Value
        };

    private sealed class GetterMember
    {
        public string Name { get; set; }
        public Func<object, object> Get { get; set; }
        public object DefaultValue { get; set; }
    }
}
