namespace PListSerializer.Core.Tests.TestModels;

public class ClassWithInterfaceCollections
{
    public IList<string> List { get; set; }
    public ICollection<string> Collection { get; set; }
    public IEnumerable<string> Enumerable { get; set; }
    public IReadOnlyList<string> ReadOnlyList { get; set; }
    public IReadOnlyCollection<string> ReadOnlyCollection { get; set; }
    public ISet<string> Set { get; set; }
    public IDictionary<string, int> Dictionary { get; set; }
    public IReadOnlyDictionary<string, int> ReadOnlyDictionary { get; set; }
}
