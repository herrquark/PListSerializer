namespace PlistSerializer.Performance.Models;

public class BasicModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public bool Enabled { get; set; }

    public int[] Values { get; set; }

    public byte[] Data { get; set; }
}

[Flags]
public enum Access : long
{
    None = 0,
    Read = 1,
    Write = 2,
    High = 1L << 40
}

public class ScalarModel
{
    public bool Boolean { get; set; } = true;

    public bool False { get; set; }

    public byte Byte { get; set; } = byte.MaxValue;

    public sbyte SignedByte { get; set; } = sbyte.MinValue;

    public short Short { get; set; } = short.MinValue;

    public ushort UnsignedShort { get; set; } = ushort.MaxValue;

    public int Integer { get; set; } = int.MinValue;

    public uint UnsignedInteger { get; set; } = uint.MaxValue;

    public long Long { get; set; } = long.MinValue;

    public ulong UnsignedLong { get; set; } = ulong.MaxValue;

    public char Character { get; set; } = 'é';

    public string Text { get; set; } = "ASCII & <xml> \"quotes\" Україна 😂";

    public float Single { get; set; } = 1.25f;

    public double Double { get; set; } = Math.PI;

    public decimal Decimal { get; set; } = 1.2345678901234567890123456789m;

    public DateTime Utc { get; set; } = new(2020, 1, 2, 3, 4, 5, 500, DateTimeKind.Utc);

    public DateTime Local { get; set; } = new DateTime(2020, 1, 2, 3, 4, 5, 500, DateTimeKind.Utc).ToLocalTime();

    public DateTimeOffset Offset { get; set; } = new(2020, 1, 2, 3, 4, 5, 500, TimeSpan.FromHours(3));

    public TimeSpan Duration { get; set; } = new(1, 2, 3, 4, 5);

    public Guid Identifier { get; set; } = new("00112233-4455-6677-8899-aabbccddeeff");

    public Uri AbsoluteUri { get; set; } = new("https://example.com/a%20b?q=x#part");

    public Uri RelativeUri { get; set; } = new("relative/path", UriKind.Relative);

    public Access Flags { get; set; } = Access.Read | Access.High;

    public Access UnknownEnumValue { get; set; } = (Access)99;
}

public class NullableModel
{
    public int? Integer { get; set; }

    public long? Long { get; set; }

    public uint? Unsigned { get; set; }

    public float? Single { get; set; }

    public double? Double { get; set; }

    public decimal? Decimal { get; set; }

    public bool? Boolean { get; set; }

    public DateTime? Date { get; set; }

    public DateTimeOffset? Offset { get; set; }

    public TimeSpan? Duration { get; set; }

    public Guid? Identifier { get; set; }

    public Access? Enum { get; set; }

    public int? Missing { get; set; }

    public int?[] Array { get; set; } = [0, null, 42];

    public List<Access?> List { get; set; } = [Access.None, null, Access.Write];

    public Dictionary<string, decimal?> Dictionary { get; set; } = new()
    {
        ["zero"] = 0m,
        ["missing"] = null,
        ["value"] = 1.25m
    };
}
