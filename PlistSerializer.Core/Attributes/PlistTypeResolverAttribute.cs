namespace PListSerializer.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class PlistTypeResolverAttribute(Type resolver) : Attribute
{
    public Type Resolver { get; set; } = resolver;
}
