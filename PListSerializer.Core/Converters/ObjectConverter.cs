using System.Reflection;
using System.Linq.Expressions;
using PListNet;
using PListNet.Nodes;
using PListSerializer.Core.Extensions;

namespace PListSerializer.Core.Converters;

internal class ObjectConverter<TObject> : IPlistConverter<TObject>
{
    private readonly Func<TObject> _activator;
    private readonly Dictionary<string, (Action<TObject, PNode> Deserialize, Func<TObject, PNode> Serialize)> _serializationMethods;

    public ObjectConverter(Dictionary<PropertyInfo, IPlistConverter> propertyConverters)
    {
        var outInstanceConstructor = typeof(TObject).GetConstructor([]);

        _activator = outInstanceConstructor == null
            ? throw new Exception($"Default constructor for {typeof(TObject).Name} not found")
            : Expression.Lambda<Func<TObject>>(Expression.New(outInstanceConstructor)).Compile();

        _serializationMethods = propertyConverters.ToDictionary(
            pair => pair.Key.GetName(),
            pair => (Deserialize: BuildDeserializeMethod(pair.Key, pair.Value), Serialize: BuildSerializeMethod(pair.Key, pair.Value))
        );

        AddSelfTypeDeserializeMethods();
    }

    public TObject Deserialize(PNode rootNode)
    {
        var instance = _activator();
        if (rootNode is DictionaryNode dictionaryNode)
        {
            using var enumerator = dictionaryNode.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var token = enumerator.Current;
                var propertyName = token.Key;

                if (_serializationMethods.TryGetValue(propertyName, out var methods))
                    methods.Deserialize(instance, token.Value);
            }
        }

        return instance;
    }

    public PNode Serialize(TObject obj)
    {
        var dn = new DictionaryNode();
        foreach (var propertyName in _serializationMethods.Keys)
        {
            var propertyValue = typeof(TObject).GetProperty(propertyName)?.GetValue(obj);
            if (propertyValue == null)
                continue;

            if (_serializationMethods.TryGetValue(propertyName, out var methods))
                dn.Add(propertyName, methods.Serialize(obj));
        }

        return dn;
    }

    private static Action<TObject, PNode> BuildDeserializeMethod(PropertyInfo property, IPlistConverter propertyValueConverter)
    {
        const string deserializeMethodName = nameof(IPlistConverter<object>.Deserialize);

        var instance = Expression.Parameter(typeof(TObject));
        var parameter = Expression.Parameter(typeof(PNode));

        var converterType = propertyValueConverter.GetType();
        var deserializeMethod = converterType.GetMethod(deserializeMethodName) ?? throw new InvalidOperationException($"Bad converter for type {property.PropertyType}");

        var converter = Expression.Constant(propertyValueConverter, converterType);
        var propertyValue = Expression.Call(converter, deserializeMethod, parameter);

        var body = Expression.Assign(Expression.Property(instance, property), propertyValue);
        return Expression
            .Lambda<Action<TObject, PNode>>(body, instance, parameter)
            .Compile();
    }

    private static Func<TObject, PNode> BuildSerializeMethod(PropertyInfo property, IPlistConverter propertyValueConverter)
    {
        const string serializeMethodName = nameof(IPlistConverter<object>.Serialize);

        var obj = Expression.Parameter(typeof(TObject));

        var converterType = propertyValueConverter.GetType();
        var serializeMethod = converterType.GetMethod(serializeMethodName) ?? throw new InvalidOperationException($"Bad converter for type {property.PropertyType}");

        var propertyValue = Expression.Property(obj, property);

        var converter = Expression.Constant(propertyValueConverter, converterType);
        var convertCall = Expression.Call(converter, serializeMethod, propertyValue);

        // var body = Expression.Assign(Expression.Property(node, property), propertyValue);
        return Expression
            .Lambda<Func<TObject, PNode>>(convertCall, obj)
            .Compile();
    }

    private void AddSelfTypeDeserializeMethods()
    {
        var type = typeof(TObject);
        var properties = type.GetProperties();

        AddSelfTypeDeserializeMethods(properties, type);
        AddSelfArrayTypeDeserializeMethods(properties, type);
        AddSelfListTypeDeserializeMethods(properties, type);
        AddSelfDictionaryTypeDeserializeMethods(properties, type);
    }

    private void AddSelfTypeDeserializeMethods(PropertyInfo[] properties, Type type)
    {
        var selfTypeProperties = properties.Where(x => x.PropertyType == type);
        foreach (var propertyInfo in selfTypeProperties)
            AddDeserializeMethodForProperty(propertyInfo, this);
    }

    private void AddSelfListTypeDeserializeMethods(PropertyInfo[] properties, Type type)
    {
        var selfTypeDictionaryProperties = properties
            .Where(x => x.PropertyType.IsList())
            .Where(x => x.PropertyType.GenericTypeArguments.Contains(type));

        foreach (var propertyInfo in selfTypeDictionaryProperties)
        {
            var elementType = propertyInfo.PropertyType.GenericTypeArguments.FirstOrDefault(x => x == type);
            var dictionaryConverterType = typeof(ListConverter<>).MakeGenericType(elementType);
            var converter = (IPlistConverter)Activator.CreateInstance(dictionaryConverterType, this);
            AddDeserializeMethodForProperty(propertyInfo, converter);
        }
    }

    private void AddSelfDictionaryTypeDeserializeMethods(PropertyInfo[] properties, Type type)
    {
        var selfTypeDictionaryProperties = properties
            .Where(x => x.PropertyType.IsDictionary())
            .Where(x => x.PropertyType.GenericTypeArguments.Contains(type));

        foreach (var propertyInfo in selfTypeDictionaryProperties)
        {
            var elementType = propertyInfo.PropertyType.GenericTypeArguments.FirstOrDefault(x => x == type);
            var dictionaryConverterType = typeof(DictionaryConverter<>).MakeGenericType(elementType);
            var converter = (IPlistConverter)Activator.CreateInstance(dictionaryConverterType, this);
            AddDeserializeMethodForProperty(propertyInfo, converter);
        }
    }

    private void AddSelfArrayTypeDeserializeMethods(PropertyInfo[] properties, Type type)
    {
        var selfTypeArrayProperties = properties
            .Where(x => x.PropertyType.IsArray)
            .Where(x => x.PropertyType.GetElementType() == type);

        foreach (var propertyInfo in selfTypeArrayProperties)
        {
            var elementType = propertyInfo.PropertyType.GetElementType();
            var dictionaryConverterType = typeof(ArrayConverter<>).MakeGenericType(elementType);
            var converter = (IPlistConverter)Activator.CreateInstance(dictionaryConverterType, this);
            AddDeserializeMethodForProperty(propertyInfo, converter);
        }
    }

    private void AddDeserializeMethodForProperty(PropertyInfo propertyInfo, IPlistConverter plistConverter)
    {
        _serializationMethods.Add(
            propertyInfo.Name,
            (
                Deserialize: BuildDeserializeMethod(propertyInfo, plistConverter),
                Serialize: BuildSerializeMethod(propertyInfo, plistConverter)
            )
        );
    }
}
