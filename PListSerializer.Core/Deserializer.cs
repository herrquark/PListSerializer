﻿using PListNet;
using PListSerializer.Core.Converters;
using PListSerializer.Core.Extensions;

namespace PListSerializer.Core;

public class Deserializer
{
    private readonly Dictionary<Type, IPlistConverter> _converters;

    public Deserializer()
    {
        _converters = new Dictionary<Type, IPlistConverter>()
        {
            {typeof(bool), new PrimitiveConverter<bool>()},
            {typeof(int), new IntegerConverter()},
            {typeof(long), new PrimitiveConverter<long>()},
            {typeof(string), new PrimitiveConverter<string>()},
            {typeof(double), new PrimitiveConverter<double>()},
            {typeof(byte[]), new PrimitiveConverter<byte[]>()},
            {typeof(DateTime), new PrimitiveConverter<DateTime>()},
        };
    }

    public TOut Deserialize<TOut>(PNode source)
    {
        var outType = typeof(TOut);
        var converter = GetOrBuildConverter(outType);
        var typedConverter = (IPlistConverter<TOut>)converter;
        return typedConverter.Deserialize(source);
    }

    public PNode Serialize<TObj>(TObj obj)
    {
        var objType = typeof(TObj);
        var converter = GetOrBuildConverter(objType);
        var typedConverter = (IPlistConverter<TObj>)converter;
        return typedConverter.Serialize(obj);
    }

    private IPlistConverter GetOrBuildConverter(Type type)
    {
        return _converters.GetOrAdd(type, () => BuildConverter(type));
    }

    private IPlistConverter BuildConverter(Type type)
    {
        if (type.IsDictionary())
        {
            var dictionaryConverter = BuildDictionaryConverter(type);
            return dictionaryConverter;
        }
        else if (type.IsArray)
        {
            var arrayConverter = BuildArrayConverter(type);
            return arrayConverter;
        }
        else if (type.IsList())
        {
            var arrayConverter = BuildListConverter(type);
            return arrayConverter;
        }

        var objectConverter = BuildObjectConverter(type);
        return objectConverter;
    }

    private IPlistConverter BuildDictionaryConverter(Type type)
    {
        var valueType = type.GenericTypeArguments[1];
        var valueConverter = GetOrBuildConverter(valueType);
        var converterType = typeof(DictionaryConverter<>).MakeGenericType(valueType);
        var dictionaryConverter = (IPlistConverter)Activator.CreateInstance(converterType, valueConverter);
        return dictionaryConverter;
    }

    private IPlistConverter BuildArrayConverter(Type type)
    {
        var valueType = type.GetElementType();
        var arrayElementConverter = GetOrBuildConverter(valueType);
        var converterType = typeof(ArrayConverter<>).MakeGenericType(valueType);
        var arrayConverter = (IPlistConverter)Activator.CreateInstance(converterType, arrayElementConverter);
        return arrayConverter;
    }

    private IPlistConverter BuildListConverter(Type type)
    {
        var valueType = type.GenericTypeArguments[0];
        var arrayElementConverter = GetOrBuildConverter(valueType);
        var converterType = typeof(ListConverter<>).MakeGenericType(valueType);
        var arrayConverter = (IPlistConverter)Activator.CreateInstance(converterType, arrayElementConverter);
        return arrayConverter;
    }

    private IPlistConverter BuildObjectConverter(Type type)
    {
        var properties = type.GetProperties();

        var propertyInfos = properties
            .Where(x => x.PropertyType != type)
            .Where(x => !x.GetGenericSubTypes().Contains(type))
            .ToDictionary(p => p, p => GetOrBuildConverter(p.PropertyType));

        var objectConverterType = typeof(ObjectConverter<>).MakeGenericType(type);
        var plistConverter = (IPlistConverter)Activator.CreateInstance(objectConverterType, propertyInfos);
        return plistConverter;
    }
}
