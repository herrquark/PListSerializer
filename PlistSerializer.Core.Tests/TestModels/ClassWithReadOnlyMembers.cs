namespace PlistSerializer.Core.Tests.TestModels;

public class ClassWithReadOnlyMembers
{
    private readonly Dictionary<string, string> _items = [];

    public string Name { get; set; }

    public string Computed => Name + "!";

    public string this[string key]
    {
        get => _items[key];
        set => _items[key] = value;
    }
}
