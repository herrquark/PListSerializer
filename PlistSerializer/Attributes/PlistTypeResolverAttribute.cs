namespace PlistSerializer.Attributes;

/// <summary>
/// Makes the deserializer ask an <see cref="IPlistTypeResolver"/> for the concrete type whenever the decorated class is the declared type.
/// </summary>
/// <param name="resolver">The <see cref="IPlistTypeResolver"/> implementation to create.</param>
[AttributeUsage(AttributeTargets.Class)]
public class PlistTypeResolverAttribute(Type resolver) : Attribute
{
    /// <summary>
    /// Gets or sets the <see cref="IPlistTypeResolver"/> implementation to create.
    /// </summary>
    public Type Resolver { get; set; } = resolver;
}
