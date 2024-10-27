using System.Collections;
using System.ComponentModel;
using PListNet;
using PListNet.Nodes;
using PListSerializer.Core.Extensions;

namespace PListSerializer.Core;

public class Deserializer
{
    public static TOut Deserialize<TOut>(PNode node)
        => (TOut)Deserialize(typeof(TOut), node);

    private static object Deserialize(Type type, PNode node)
        => type switch
        {
            _ when type.IsDictionary() => DeserializeDictionary(type, node),
            _ when type.IsArray => DeserializeArray(type, node),
            _ when type.IsList() => DeserializeList(type, node),
            _ when type.IsHashSet() => DeserializeHashSet(type, node),
            _ when type.IsEnum => DeserializeEnum(type, node),

            _ when node is IntegerNode integerNode => ConvertToType(integerNode.Value, type),
            _ when node is RealNode realNode => ConvertToType(realNode.Value, type),
            _ when node is StringNode stringNode => ConvertToType(stringNode.Value, type),
            _ when node is BooleanNode booleanNode => ConvertToType(booleanNode.Value, type),
            _ when node is DataNode dataNode => ConvertToType(dataNode.Value, type),
            _ when node is DateNode dateNode => ConvertToType(dateNode.Value, type),

            _ => DeserializeObject(type, node)
        };

    private static object DeserializeDictionary(Type type, PNode node)
    {
        if (node is not DictionaryNode dictionaryNode)
            return default;

        var valueType = type.GenericTypeArguments[1];
        var dictionary = (IDictionary)Activator.CreateInstance(type);

        foreach (var kvp in dictionaryNode)
        {
            var key = kvp.Key;
            var value = kvp.Value;

            dictionary.Add(key.ToString(), Deserialize(valueType, value));
        }

        return dictionary;
    }

    private static object DeserializeArray(Type type, PNode node)
    {
        if (node is not ArrayNode arrayNode)
            return default;

        var elementType = type.GetElementType();
        var array = Array.CreateInstance(elementType, arrayNode.Count);

        for (var i = 0; i < arrayNode.Count; i++)
        {
            var itemNode = arrayNode[i];
            array.SetValue(Deserialize(elementType, itemNode), i);
        }

        return array;
    }

    private static object DeserializeList(Type type, PNode node)
    {
        if (node is not ArrayNode arrayNode)
            return default;

        var elementType = type.GenericTypeArguments[0];
        var list = (IList)Activator.CreateInstance(type);

        foreach (var itemNode in arrayNode)
            list.Add(Deserialize(elementType, itemNode));

        return list;
    }

    private static object DeserializeHashSet(Type type, PNode node)
    {
        if (node is not ArrayNode arrayNode)
            return default;

        var elementType = type.GenericTypeArguments[0];
        var hashSet = Activator.CreateInstance(type);
        var addMethod = type.GetMethod("Add");

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
        var properties = resolvedType.GetProperties();

        foreach (var kvp in dictionaryNode)
        {
            var key = kvp.Key;
            var value = kvp.Value;

            var property = properties.FirstOrDefault(x => x.GetName() == key);
            if (property == null)
                continue;

            var propertyType = property.PropertyType;
            var propertyValue = Deserialize(propertyType, value);

            property.SetValue(instance, propertyValue);
        }

        return instance;
    }

    // private static object DeserializePrimitive(Type type, PNode node)
    //     => node switch
    //     {

    //         _ => default
    //     };

    private static object ConvertToType(object value, Type type)
        => value != null
            ? Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? type)
            : null;
}