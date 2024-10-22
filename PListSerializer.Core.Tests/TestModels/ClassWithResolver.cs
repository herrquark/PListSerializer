using PListNet.Nodes;
using PListNet;
using PListSerializer.Core.Attributes;

namespace PListSerializer.Core.Tests.TestModels;


internal class ResolverTestClass
{
    public ResolverTestBaseClass[] Arr { get; set; }
    public List<ResolverTestBaseClass> Lst { get; set; }
    public ResolverTestBaseClass Cls { get; set; }
}

internal class ResolverTestBaseClassResolver : IPlistTypeResolver<ResolverTestBaseClass>
{
    public Type ResolveType(PNode node)
    {
        var type = node is DictionaryNode dictNode && dictNode["Type"] is StringNode stringNode
            ? stringNode.Value
            : null;

        return type switch
        {
            "One" => typeof(ResolverTestOne),
            "Two" => typeof(ResolverTestTwo),
            "Three" => typeof(ResolverTestThree),
            _ => null
            //_ => throw new NotSupportedException()
        };
    }
}


[PlistTypeResolver(typeof(ResolverTestBaseClassResolver))]
internal class ResolverTestBaseClass
{
    public string Type { get; set; }
}

internal class ResolverTestOne : ResolverTestBaseClass
{
    public string Name_One { get; set; }
}

internal class ResolverTestTwo : ResolverTestBaseClass
{
    public string Name_Two { get; set; }
}

internal class ResolverTestThree : ResolverTestBaseClass
{
    public string Name_Three { get; set; }
}
