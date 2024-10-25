using PListNet;
using PListSerializer.Core.Converters;
using PListSerializer.Core.Extensions;

namespace PListSerializer.Core;

public class Deserializer
{
    internal static Dictionary<Type, IPlistConverter> Converters { get; private set; } = [];
    internal static Dictionary<Type, IPlistTypeResolver> TypeResolvers { get; private set; } = [];

    static Deserializer()
    {
        Converters = new Dictionary<Type, IPlistConverter>()
        {
            {typeof(bool), new PrimitiveConverter<bool>()},
            {typeof(int), new IntegerConverter()},
            {typeof(long), new PrimitiveConverter<long>()},
            {typeof(string), new PrimitiveConverter<string>()},
            {typeof(double), new PrimitiveConverter<double>()},
            {typeof(byte[]), new PrimitiveConverter<byte[]>()},
            {typeof(DateTime), new PrimitiveConverter<DateTime>()},
        };

        //Assembly.GetEntryAssembly()
        //    .GetTypes()
        //    .Where(x => x.IsClass && !x.IsAbstract && x.GetInterfaces().Contains(typeof(IPlistTypeResolver<>)))
        //    .ToList()
        //    .ForEach(x =>
        //    {
        //        var instance = (IPlistTypeResolver)Activator.CreateInstance(x);
        //        TypeResolvers.Add(x.GenericTypeArguments[0], instance);
        //    });
    }

    public Deserializer()
    {
    }

    public TOut Deserialize<TOut>(PNode source)
    {
        var outType = typeof(TOut);
        var converter = GetOrBuildConverter(outType);
        var typedConverter = (IPlistConverter<TOut>)converter;
        return typedConverter.Deserialize(source);
    }

    internal static IPlistConverter GetOrBuildConverter(Type type)
    {
        return Converters.GetOrAdd(type, () => BuildConverter(type));
    }

    internal static IPlistConverter BuildConverter(Type type)
    {
        return type switch
        {
            _ when type.IsDictionary() => BuildDictionaryConverter(type),
            _ when type.IsArray => BuildArrayConverter(type),
            _ when type.IsList() => BuildListConverter(type),
            _ when type.IsHashSet() => BuildHashSetConverter(type),
            _ when type.IsEnum => BuildEnumConverter(type),
            _ => BuildObjectConverter(type)
        };
    }

    internal static IPlistConverter BuildDictionaryConverter(Type type)
    {
        var valueType = type.GenericTypeArguments[1];
        var valueConverter = GetOrBuildConverter(valueType);
        var converterType = typeof(DictionaryConverter<>).MakeGenericType(valueType);
        var dictionaryConverter = (IPlistConverter)Activator.CreateInstance(converterType, valueConverter);
        return dictionaryConverter;
    }

    internal static IPlistConverter BuildArrayConverter(Type type)
    {
        var valueType = type.GetElementType();
        var arrayElementConverter = GetOrBuildConverter(valueType);
        var converterType = typeof(ArrayConverter<>).MakeGenericType(valueType);
        var arrayConverter = (IPlistConverter)Activator.CreateInstance(converterType, arrayElementConverter);
        return arrayConverter;
    }

    internal static IPlistConverter BuildListConverter(Type type)
    {
        var valueType = type.GenericTypeArguments[0];
        var arrayElementConverter = GetOrBuildConverter(valueType);
        var converterType = typeof(ListConverter<>).MakeGenericType(valueType);
        var arrayConverter = (IPlistConverter)Activator.CreateInstance(converterType, arrayElementConverter);
        return arrayConverter;
    }

    internal static IPlistConverter BuildHashSetConverter(Type type)
    {
        var valueType = type.GenericTypeArguments[0];
        var hashSetElementConverter = GetOrBuildConverter(valueType);
        var converterType = typeof(HashSetConverter<>).MakeGenericType(valueType);
        var hashSetConverter = (IPlistConverter)Activator.CreateInstance(converterType, hashSetElementConverter);
        return hashSetConverter;
    }

    internal static IPlistConverter BuildObjectConverter(Type type)
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

    internal static IPlistConverter BuildEnumConverter(Type type)
        => (IPlistConverter)Activator.CreateInstance(typeof(EnumConverter<>).MakeGenericType(type));
}
