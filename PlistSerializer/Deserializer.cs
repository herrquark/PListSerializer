using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using PlistSerializer.Extensions;
using PlistSerializer.Nodes;

namespace PlistSerializer;

/// <summary>
/// Maps plist nodes to .NET objects.
/// </summary>
public static class Deserializer
{
    /// <summary>
    /// Deserializes a plist node into an instance of <typeparamref name="TOut"/>.
    /// </summary>
    /// <typeparam name="TOut">The type to create.</typeparam>
    /// <param name="node">The node to read.</param>
    /// <returns>The deserialized value.</returns>
    public static TOut Deserialize<TOut>(PNode node)
        => (TOut)Deserialize(typeof(TOut), node);

    private static object Deserialize(Type type, PNode node)
        => type switch
        {
            // a node-typed target takes the node itself
            _ when typeof(PNode).IsAssignableFrom(type) => type.IsInstanceOfType(node) ? node : null,

            _ when type.IsDictionary() => DeserializeDictionary(type, node),
            _ when type.IsArray => DeserializeArray(type, node),
            _ when type.IsList() => DeserializeList(type, node),
            _ when type.IsHashSet() => DeserializeHashSet(type, node),
            _ when type.IsEnum => DeserializeEnum(type, node),

            _ when node is IntegerNode integerNode => ConvertToType(integerNode.Value, type),
            _ when node is RealNode realNode => ConvertToType(realNode.Value, type),
            _ when node is StringNode stringNode => ConvertToType(stringNode.Value, type),
            _ when node is BooleanNode booleanNode => ConvertToType(booleanNode.Value, type),
            _ when node is DateNode dateNode => ConvertToType(dateNode.Value, type),
            _ when node is UidNode uidNode => ConvertToType(uidNode.Value, type),

            _ => DeserializeObject(type, node)
        };

    private static object DeserializeDictionary(Type type, PNode node)
    {
        if (node is not DictionaryNode dictionaryNode)
            return default;

        // the declared type may be an interface, so always build the concrete collection
        var valueType = type.GenericTypeArguments[1];
        var dictionary = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(type.GenericTypeArguments));

        foreach (var (key, value) in dictionaryNode)
            dictionary.Add(key, Deserialize(valueType, value));

        return dictionary;
    }

    private static object DeserializeArray(Type type, PNode node)
    {
        // byte[] is an array
        if (node is DataNode dataNode)
            return dataNode.Value;

        if (node is not ArrayNode arrayNode)
            return default;

        var elementType = type.GetElementType();
        var array = Array.CreateInstance(elementType, arrayNode.Count);

        for (var i = 0; i < arrayNode.Count; i++)
            array.SetValue(Deserialize(elementType, arrayNode[i]), i);

        return array;
    }

    private static object DeserializeList(Type type, PNode node)
    {
        if (node is not ArrayNode arrayNode)
            return default;

        var elementType = type.GenericTypeArguments[0];
        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

        foreach (var itemNode in arrayNode)
            list.Add(Deserialize(elementType, itemNode));

        return list;
    }

    private static object DeserializeHashSet(Type type, PNode node)
    {
        if (node is not ArrayNode arrayNode)
            return default;

        var elementType = type.GenericTypeArguments[0];
        var hashSetType = typeof(HashSet<>).MakeGenericType(elementType);
        var hashSet = Activator.CreateInstance(hashSetType);
        var addMethod = hashSetType.GetMethod("Add");

        foreach (var itemNode in arrayNode)
            addMethod.Invoke(hashSet, [Deserialize(elementType, itemNode)]);

        return hashSet;
    }

    private static object DeserializeEnum(Type type, PNode node)
    {
        if (node is not StringNode stringNode)
            return default;

        return Enum.Parse(type, stringNode.Value);
    }

    private static object DeserializeObject(Type type, PNode node)
    {
        if (node is not DictionaryNode dictionaryNode)
            return default;

        var resolvedType = type.GetResolver()?.ResolveType(dictionaryNode) ?? type;

        var instance = Activator.CreateInstance(resolvedType);
        var properties = resolvedType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.IsPlistMember())
            .ToArray();

        foreach (var (key, value) in dictionaryNode)
        {
            var property = properties.FirstOrDefault(x => x.GetName() == key);
            if (property == null)
                continue;

            property.SetValue(instance, Deserialize(property.PropertyType, value));
        }

        return instance;
    }

    private static object ConvertToType(object value, Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        // plist values do not depend on the culture, so neither does parsing them
        var culture = CultureInfo.InvariantCulture;

        return type switch
        {
            _ when value is null => null,
            _ when type == typeof(TimeSpan) => TimeSpan.TryParse(value.ToString(), culture, out var result) ? result : null,
            _ when type == typeof(Uri) => new Uri(value.ToString(), UriKind.RelativeOrAbsolute),
            _ when type == typeof(Guid) => Guid.TryParse(value.ToString(), out var result) ? result : null,
            _ when TypeDescriptor.GetConverter(type).CanConvertFrom(value.GetType()) => TypeDescriptor.GetConverter(type).ConvertFrom(null, culture, value),
            _ => Convert.ChangeType(value, type, culture)
        };
    }
}
