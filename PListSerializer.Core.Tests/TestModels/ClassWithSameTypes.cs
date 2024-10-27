namespace PListSerializer.Core.Tests.TestModels;

public class BaseClassWithSameTypes
{
    public ClassWithSameTypes ClassSameType3 { get; set; }
    public ClassWithSameTypes[] ArraySameType3 { get; set; }
    public Dictionary<string, ClassWithSameTypes> DictionarySameType3 { get; set; }
    public List<ClassWithSameTypes> List3 { get; set; }
}

public class ClassWithSameTypes : BaseClassWithSameTypes
{
    public string Id { get; set; }
    public ClassWithSameTypes ClassSameType { get; set; }
    public ClassWithSameTypes ClassSameType2 { get; set; }
    public ClassWithSameTypes[] ArraySameType { get; set; }
    public ClassWithSameTypes[] ArraySameType2 { get; set; }
    public Dictionary<string, ClassWithSameTypes> DictionarySameType { get; set; }
    public Dictionary<string, ClassWithSameTypes> DictionarySameType2 { get; set; }

    public List<ClassWithSameTypes> List1 { get; set; }
    public List<ClassWithSameTypes> List2 { get; set; }

    public HashSet<string> HashSetOfStrings { get; set; }
    public HashSet<ClassWithSameTypes> HashSetOfSelf { get; set; }

    public int? NullableInt { get; set; }
    public int?[] NullableIntArray { get; set; }
    public List<int?> NullableIntList { get; set; }
    public Dictionary<string, int?> NullableIntDictionary { get; set; }

    public long? NullableLong { get; set; }
    public long?[] NullableLongArray { get; set; }
    public List<long?> NullableLongList { get; set; }
    public Dictionary<string, long?> NullableLongDictionary { get; set; }

    public float? NullableFloat { get; set; }
    public float?[] NullableFloatArray { get; set; }
    public List<float?> NullableFloatList { get; set; }
    public Dictionary<string, float?> NullableFloatDictionary { get; set; }

    public double? NullableDouble { get; set; }
    public double?[] NullableDoubleArray { get; set; }
    public List<double?> NullableDoubleList { get; set; }
    public Dictionary<string, double?> NullableDoubleDictionary { get; set; }

    public decimal? NullableDecimal { get; set; }
    public decimal?[] NullableDecimalArray { get; set; }
    public List<decimal?> NullableDecimalList { get; set; }
    public Dictionary<string, decimal?> NullableDecimalDictionary { get; set; }

    public bool? NullableBool { get; set; }
    public bool?[] NullableBoolArray { get; set; }
    public List<bool?> NullableBoolList { get; set; }
    public Dictionary<string, bool?> NullableBoolDictionary { get; set; }

    public DateTime? NullableDateTime { get; set; }
    public DateTime?[] NullableDateTimeArray { get; set; }
    public List<DateTime?> NullableDateTimeList { get; set; }
    public Dictionary<string, DateTime?> NullableDateTimeDictionary { get; set; }

    public TestEnum? NullableEnum { get; set; }
    public TestEnum?[] NullableEnumArray { get; set; }
    public List<TestEnum?> NullableEnumList { get; set; }
    public Dictionary<string, TestEnum?> NullableEnumDictionary { get; set; }
}

public enum TestEnum
{
    Value1,
    Value2,
    Value3,
    Value4,
    Value5
}