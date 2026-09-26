namespace PlistSerializer.Core;

/// <summary>
/// Picks the concrete type a dictionary node deserializes into. Attach it to a class with <see cref="Attributes.PlistTypeResolverAttribute"/>.
/// </summary>
/// <remarks>
/// One instance is created per decorated class and shared across threads, so implementations must be stateless.
/// </remarks>
public interface IPlistTypeResolver
{
    /// <summary>
    /// Returns the type to create for the node, or <c>null</c> to use the declared type.
    /// </summary>
    /// <param name="node">The dictionary node being deserialized.</param>
    /// <returns>The type to create, or <c>null</c>.</returns>
    Type ResolveType(PNode node);
}
