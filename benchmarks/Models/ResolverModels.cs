using PlistSerializer.Attributes;
using PlistSerializer.Nodes;

namespace PlistSerializer.Performance.Models;

[PlistTypeResolver(typeof(VariantResolver))]
public class Variant
{
    public string Kind { get; set; }
}

public class NamedVariant : Variant
{
    public string Name { get; set; }
}

public class NumberedVariant : Variant
{
    public int Number { get; set; }
}

public class VariantResolver : IPlistTypeResolver
{
    public Type ResolveType(PNode node)
        => ((DictionaryNode)node)["Kind"] is StringNode kind
            ? kind.Value switch
            {
                "name" => typeof(NamedVariant),
                "number" => typeof(NumberedVariant),
                _ => null
            }
            : null;
}

public class ResolverModel
{
    public Variant Single { get; set; }

    public Variant[] Array { get; set; }

    public List<Variant> List { get; set; }

    public Dictionary<string, Variant> Dictionary { get; set; }
}
