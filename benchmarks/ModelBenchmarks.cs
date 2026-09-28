using PlistSerializer.Nodes;
using PlistSerializer.Performance.Models;

namespace PlistSerializer.Performance;

internal static class ModelBenchmarks
{
    public static IEnumerable<BenchmarkCase> Create()
    {
        var families = new[]
        {
            Scenario.Models("basic", CreateBasic, Check.Properties),
            Scenario.Models("scalars", i => new ScalarModel
            {
                Integer = i,
                Text = "device-" + i + " & <xml> Україна 😂"
            }, Check.Properties),
            Scenario.Models("nullable", CreateNullable, Check.Properties, ValidateNullableNode),
            Scenario.Models("collections", _ => new CollectionModel(), ValidateCollections),
            Scenario.Models("members", _ => new MemberModel
            {
                Field = 42,
                HiddenField = "serialized field",
                Missing = null
            }, ValidateMembers, ValidateMemberNode),
            Scenario.Models("resolvers", CreateResolver, ValidateResolver),
            Scenario.Models("untyped", CreateUntyped, ValidateUntyped),
            Scenario.Models("nodes", _ => new NodeModel(), ValidateNodes, ValidateNodeIdentity),
            Scenario.Models("nested-shared", CreateBranch, ValidateBranch),
            Scenario.Models("payloads", CreatePayload, Check.Properties)
        };

        foreach (var family in families)
            foreach (var benchmark in family)
                yield return benchmark;
    }

    private static BasicModel CreateBasic(int i)
        => new()
        {
            Id = i,
            Name = "device-" + i,
            Enabled = true,
            Values = [1, 2, 3],
            Data = new byte[32]
        };

    private static NullableModel CreateNullable(int i)
        => new()
        {
            Integer = i,
            Long = i % 2 == 0 ? long.MaxValue : null,
            Unsigned = uint.MaxValue,
            Single = 1.25f,
            Double = Math.PI,
            Decimal = 1.25m,
            Boolean = false,
            Date = new DateTime(2020, 1, 1, 0, 0, 0, 500, DateTimeKind.Utc),
            Offset = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.FromHours(3)),
            Duration = TimeSpan.FromSeconds(1.5),
            Identifier = Guid.Empty,
            Enum = Access.Read | Access.Write
        };

    private static void ValidateNullableNode(NullableModel source, PNode node)
    {
        var dictionary = (DictionaryNode)node;
        Check.That(!dictionary.ContainsKey(nameof(NullableModel.Missing)), "Null member was written.");
        Check.Equal(2, ((ArrayNode)dictionary[nameof(NullableModel.Array)]).Count);
        Check.Equal(2, ((ArrayNode)dictionary[nameof(NullableModel.List)]).Count);
        Check.Equal(2, ((DictionaryNode)dictionary[nameof(NullableModel.Dictionary)]).Count);
        Check.That(dictionary.ContainsKey(nameof(NullableModel.Boolean)), "Nullable false was omitted.");
    }

    private static void ValidateCollections(CollectionModel source, CollectionModel result, PlistFormat? format)
    {
        Check.Properties(source, result, format);
        Check.That(result.HashSet.SetEquals(source.HashSet), "HashSet contents differ.");
        Check.That(result.SetInterface.SetEquals(source.SetInterface), "ISet contents differ.");
        Check.Equal(typeof(List<string>), result.EnumerableInterface.GetType());
        Check.Equal(typeof(List<string>), result.ReadOnlyList.GetType());
        Check.Equal(typeof(Dictionary<string, int>), result.ReadOnlyDictionary.GetType());
        Check.Sequence(source.Dictionary.Keys, result.Dictionary.Keys);
    }

    private static void ValidateMemberNode(MemberModel source, PNode node)
    {
        var dictionary = (DictionaryNode)node;
        foreach (var omitted in new[] { "Default", "OmittedZero", "Missing", "GetOnly", "Item", "old_name", "Inherited" })
            Check.That(!dictionary.ContainsKey(omitted), $"Unexpected member {omitted}.");

        Check.Equal("derived", ((StringNode)dictionary["Hidden"]).Value);
        Check.Equal("serialized field", ((StringNode)dictionary["HiddenField"]).Value);
        Check.Equal(42L, ((IntegerNode)dictionary["public_field"]).Value);
        Check.Equal(0L, ((IntegerNode)dictionary["WrittenZero"]).Value);
        Check.Equal(false, ((BooleanNode)dictionary["WrittenFalse"]).Value);
        Check.Sequence(dictionary.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase), dictionary.Keys);
    }

    private static void ValidateMembers(MemberModel source, MemberModel result, PlistFormat? format)
    {
        Check.Equal(source.Hidden, result.Hidden);
        Check.Equal(13, ((MemberBase)result).Hidden);
        Check.Equal(17, ((MemberBase)result).HiddenField);
        Check.Equal("field initializer", result.HiddenField);
        Check.Equal(-7, result.Field);
        Check.Equal(source.Inherited, result.Inherited);
        Check.Equal(source.Renamed, result.Renamed);
        Check.Equal(7, result.Default);
        Check.Equal(0, result.OmittedZero);
        Check.Equal(8, result.NonDefault);
        Check.Equal(0, result.WrittenZero);
        Check.Equal(false, result.WrittenFalse);
        Check.Equal("initializer", result.Missing);
        Check.Equal("upper", result.UpperKey);
        Check.Equal("lower", result.LowerKey);
    }

    private static ResolverModel CreateResolver(int i)
    {
        Variant[] variants =
        [
            new NamedVariant { Kind = "name", Name = "name-" + i },
            new NumberedVariant { Kind = "number", Number = i },
            new Variant { Kind = "fallback" }
        ];

        return new()
        {
            Single = variants[i % variants.Length],
            Array = variants,
            List = [.. variants],
            Dictionary = variants.ToDictionary(value => value.Kind)
        };
    }

    private static void ValidateResolver(ResolverModel source, ResolverModel result, PlistFormat? format)
    {
        ValidateVariant(source.Single, result.Single);
        Check.Equal(source.Array.Length, result.Array.Length);
        Check.Equal(source.List.Count, result.List.Count);
        Check.Equal(source.Dictionary.Count, result.Dictionary.Count);

        for (var i = 0; i < source.Array.Length; i++)
        {
            ValidateVariant(source.Array[i], result.Array[i]);
            ValidateVariant(source.List[i], result.List[i]);
            ValidateVariant(source.Array[i], result.Dictionary[source.Array[i].Kind]);
        }
    }

    private static void ValidateVariant(Variant source, Variant result)
    {
        Check.Equal(source.GetType(), result.GetType());
        Check.Equal(source.Kind, result.Kind);
        if (source is NamedVariant named)
            Check.Equal(named.Name, ((NamedVariant)result).Name);
        if (source is NumberedVariant numbered)
            Check.Equal(numbered.Number, ((NumberedVariant)result).Number);
    }

    private static UntypedModel CreateUntyped(int i)
        => new()
        {
            Dictionary = new Dictionary<string, object>
            {
                ["integer"] = (long)i,
                ["real"] = 1.25,
                ["nested"] = new Dictionary<string, object> { ["flag"] = false },
                ["missing"] = null
            },
            Array = new List<object> { "text", true, null, new List<object> { (long)i, new byte[] { 1, 2 } } },
            Data = new byte[] { 1, 2, 3 },
            Scalar = new DateTime(2020, 1, 1, 0, 0, 0, 500, DateTimeKind.Utc)
        };

    private static void ValidateUntyped(UntypedModel source, UntypedModel result, PlistFormat? format)
    {
        Check.Properties(source, result, format);
        Check.Equal(typeof(Dictionary<string, object>), result.Dictionary.GetType());
        Check.Equal(typeof(List<object>), result.Array.GetType());
        Check.Equal(typeof(byte[]), result.Data.GetType());
    }

    private static void ValidateNodeIdentity(NodeModel source, PNode node)
    {
        var dictionary = (DictionaryNode)node;
        foreach (var property in typeof(NodeModel).GetProperties())
            Check.That(ReferenceEquals(property.GetValue(source), dictionary[property.Name]), "Node was not passed through.");
    }

    private static void ValidateNodes(NodeModel source, NodeModel result, PlistFormat? format)
    {
        Check.Properties(source, result, format);
        if (format == null)
            foreach (var property in typeof(NodeModel).GetProperties())
                Check.That(ReferenceEquals(property.GetValue(source), property.GetValue(result)), "Node identity changed.");
    }

    private static Branch CreateBranch(int i)
    {
        var leaf = new Branch { Id = i };
        return new()
        {
            Id = i,
            Child = new Branch { Id = i + 1, Child = new Branch { Id = i + 2, Child = leaf } },
            Array = [leaf, leaf],
            List = [leaf, leaf],
            Dictionary = new() { ["left"] = leaf, ["right"] = leaf }
        };
    }

    private static void ValidateBranch(Branch source, Branch result, PlistFormat? format)
    {
        Check.Equal(source.Id, result.Id);
        if (source.Child != null)
            ValidateBranch(source.Child, result.Child, format);
        else
            Check.That(result.Child == null, "Unexpected child.");

        if (source.Array == null)
            return;

        Check.Equal(2, result.Array.Length);
        Check.Equal(2, result.List.Count);
        Check.Equal(2, result.Dictionary.Count);
        for (var i = 0; i < 2; i++)
        {
            ValidateBranch(source.Array[i], result.Array[i], format);
            ValidateBranch(source.List[i], result.List[i], format);
        }
        ValidateBranch(source.Dictionary["left"], result.Dictionary["left"], format);
        ValidateBranch(source.Dictionary["right"], result.Dictionary["right"], format);
        Check.That(!ReferenceEquals(result.Array[0], result.Array[1]), "Object aliases should expand into independent models.");
    }

    private static PayloadModel CreatePayload(int i)
        => new()
        {
            Text = new[] { 0, 14, 15, 127, 128, 129, 255, 256, 257 }
                .SelectMany(length => new[] { new string('x', length), new string('é', length) })
                .Concat(["duplicate", "duplicate", "😂<&>\"", "unique-" + i])
                .ToArray(),
            Data = Enumerable.Range(0, 256).Select(value => (byte)value).ToArray()
        };
}
