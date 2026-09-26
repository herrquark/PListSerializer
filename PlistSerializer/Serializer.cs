using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
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
        => obj switch
        {
            bool b => new BooleanNode(b),
            int i => new IntegerNode(i),
            long l => new IntegerNode(l),
            string s => new StringNode(s),
            float f => new RealNode(f),
            double d => new RealNode(d),
            decimal dec => new RealNode(decimal.ToDouble(dec)),
            DateTime dt => new DateNode(dt),
            Enum en => new StringNode(en.ToString()),
            Guid g => new StringNode(g.ToString()),
            _ when obj.GetType().IsPrimitive => new StringNode(obj.ToString()),
            byte[] bytes => new DataNode(bytes),
            IDictionary dict => SerializeDictionary(dict),
            IEnumerable<KeyValuePair<string, object>> dict => SerializeDictionary(dict),
            IEnumerable enumerable => SerializeEnumerable(enumerable),
            _ => SerializeComplexType(obj)
        };

    private static PNode SerializeComplexType(object obj)
    {
        var dictNode = new DictionaryNode();

        foreach (var member in GetMembers(obj.GetType()))
        {
            var value = member.Get(obj);
            if (value != null && (member.DefaultValue == null || !value.Equals(member.DefaultValue)))
                dictNode.Add(member.Name, Serialize(value));
        }

        return dictNode;
    }

    private static PNode SerializeDictionary(IDictionary dict)
    {
        var dictNode = new DictionaryNode();

        foreach (var key in dict.Keys)
        {
            if (dict[key] is null)
                continue;

            dictNode.Add(key.ToString(), Serialize(dict[key]));
        }

        return dictNode;
    }

    private static PNode SerializeDictionary(IEnumerable<KeyValuePair<string, object>> pairs)
    {
        var dictNode = new DictionaryNode();

        foreach (var (key, value) in pairs)
        {
            if (value is null)
                continue;

            dictNode.Add(key, Serialize(value));
        }

        return dictNode;
    }

    private static PNode SerializeEnumerable(IEnumerable list)
    {
        var node = new ArrayNode();
        node.AddRange(list.Cast<object>().Where(x => x is not null).Select(Serialize));
        return node;
    }

    private static GetterMember[] GetMembers(Type type)
    {
        if (MembersCache.TryGetValue(type, out var members))
            return members;

        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.IsPlistMember())
            .Select(BuildGetterMember);

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Select(BuildGetterMember);

        members = [.. props.Concat(fields).OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase)];

        MembersCache[type] = members;

        return members;
    }

    private static GetterMember BuildGetterMember(PropertyInfo p)
        => new()
        {
            Name = p.GetName(),
            Get = p.GetValue,
            DefaultValue = p.GetCustomAttribute<DefaultValueAttribute>(false)?.Value
        };

    private static GetterMember BuildGetterMember(FieldInfo f)
        => new()
        {
            Name = f.GetName(),
            Get = f.GetValue,
            DefaultValue = f.GetCustomAttribute<DefaultValueAttribute>(false)?.Value
        };

    private sealed class GetterMember
    {
        public string Name { get; set; }
        public Func<object, object> Get { get; set; }
        public object DefaultValue { get; set; }
    }
}
