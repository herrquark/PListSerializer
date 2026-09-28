using System.ComponentModel;
using System.Globalization;
using PlistSerializer.Nodes;

namespace PlistSerializer.Performance.Models;

[TypeConverter(typeof(CodeConverter))]
public readonly struct Code(int value) : IEquatable<Code>
{
    public int Value { get; } = value;

    public bool Equals(Code other) => Value == other.Value;

    public override bool Equals(object other) => other is Code code && Equals(code);

    public override int GetHashCode() => Value;
}

public class CodeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        => value is string text
            ? new Code(int.Parse(text, CultureInfo.InvariantCulture))
            : base.ConvertFrom(context, culture, value);
}

public class ConversionModel
{
    public int Integer { get; set; }

    public double Real { get; set; }

    public bool Boolean { get; set; }

    public Access NamedEnum { get; set; }

    public Access NumericEnum { get; set; }

    public Access? NullableEnum { get; set; }

    public ulong UnsignedUid { get; set; }

    public int SignedUid { get; set; }

    public ulong? NullableUid { get; set; }

    public object BoxedUid { get; set; }

    public IntegerNode MismatchedNode { get; set; }

    public Guid? InvalidGuid { get; set; }

    public TimeSpan? InvalidDuration { get; set; }

    public Code Custom { get; set; }

    public int Missing { get; set; } = 99;

    public int IgnoredField = -7;
}

public class UnsupportedCollectionModel
{
    public Queue<int> Queue { get; set; }

    public SortedDictionary<string, int> SortedDictionary { get; set; }
}

public class DeepModel
{
    public DeepModel Child { get; set; }
}
