using System.Collections;
using System.ComponentModel;
using System.Reflection;
using PListNet;
using PListNet.Nodes;

namespace PListSerializer.Core;

public class Serializer
{
    public static PNode Serialize(object obj)
    {
        return obj switch
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
            // IList list => SerializeList(list),
            IDictionary dict => SerializeDictionary(dict),
            IEnumerable<KeyValuePair<string, object>> dict => SerializeDictionary(dict),
            IEnumerable enumerable => SerializeEnumerable(enumerable),
            _ => SerializeComplexType(obj)
        };
    }

    private static PNode SerializeComplexType(object obj)
    {
        var type = obj.GetType();
        var members = GetMembers(type);
        var dictNode = new DictionaryNode();

        foreach (var member in members)
        {
            var val = member.Get(obj);
            if (val != null && (member.DefaultValue == null || !val.Equals(member.DefaultValue)))
                dictNode.Add(member.Name, Serialize(val));
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

        foreach (var kvp in pairs)
        {
            if (kvp.Value is null)
                continue;

            dictNode.Add(kvp.Key, Serialize(kvp.Value));
        }

        return dictNode;
    }

    private static PNode SerializeEnumerable(IEnumerable list)
    {
        var node = new ArrayNode();
        node.AddRange(list.Cast<object>().Where(x => x is not null).Select(Serialize));
        return node;
    }

    class GetterMember
    {
        public string Name { get; set; }
        public Func<object, object> Get { get; set; }
        public object DefaultValue { get; set; }
    }

    private static readonly Dictionary<Type, GetterMember[]> MembersCache = [];

    private static GetterMember[] GetMembers(Type type)
    {

        if (MembersCache.TryGetValue(type, out GetterMember[] members))
            return members;

        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanWrite)
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
            Name = p.Name,
            Get = p.GetValue,
            DefaultValue = p.GetCustomAttributes(typeof(DefaultValueAttribute), true).FirstOrDefault() is DefaultValueAttribute defaultAttribute
                    ? defaultAttribute.Value
                    : GetDefaultValueForType(p.PropertyType)
        };

    private static GetterMember BuildGetterMember(FieldInfo f)
        => new()
        {
            Name = f.Name,
            Get = f.GetValue,
            DefaultValue = f.GetCustomAttributes(typeof(DefaultValueAttribute), true).FirstOrDefault() is DefaultValueAttribute defaultAttribute
                ? defaultAttribute.Value
                : GetDefaultValueForType(f.FieldType)
        };

    private static object GetDefaultValueForType(Type type)
        => type.IsValueType ? Activator.CreateInstance(type) : null;
}
