namespace PlistSerializer.Performance.Models;

public class CollectionModel
{
    public int[] Array { get; set; } = [0, 1, -1];

    public List<string> List { get; set; } = ["first", null, "last"];

    public HashSet<string> HashSet { get; set; } = ["one", "two"];

    public Dictionary<string, int> Dictionary { get; set; } = new() { ["z"] = 1, ["A"] = 2, [""] = 3 };

    public IList<string> ListInterface { get; set; } = new[] { "a", "b" };

    public ICollection<string> CollectionInterface { get; set; } = new[] { "a", "b" };

    public IEnumerable<string> EnumerableInterface { get; set; } = Enumerable.Range(0, 3).Select(i => "lazy-" + i);

    public IReadOnlyList<string> ReadOnlyList { get; set; } = new[] { "a", "b" };

    public IReadOnlyCollection<string> ReadOnlyCollection { get; set; } = new[] { "a", "b" };

    public ISet<string> SetInterface { get; set; } = new HashSet<string> { "one", "two" };

    public IDictionary<string, int> DictionaryInterface { get; set; } = new Dictionary<string, int> { ["a"] = 1 };

    public IReadOnlyDictionary<string, int> ReadOnlyDictionary { get; set; } = new Dictionary<string, int> { ["b"] = 2 };

    public int[][] Jagged { get; set; } = [[1, 2], [], [3]];

    public List<Dictionary<string, List<int?>>> Nested { get; set; } =
    [
        new() { ["items"] = [1, null, 2], ["empty"] = [] }
    ];

    public string[] Empty { get; set; } = [];

    public byte[] EmptyData { get; set; } = [];
}

public class Branch
{
    public int Id { get; set; }

    public Branch Child { get; set; }

    public Branch[] Array { get; set; }

    public List<Branch> List { get; set; }

    public Dictionary<string, Branch> Dictionary { get; set; }
}

public class PayloadModel
{
    public string[] Text { get; set; }

    public byte[] Data { get; set; }
}

public class UntypedModel
{
    public object Dictionary { get; set; }

    public object Array { get; set; }

    public object Data { get; set; }

    public object Scalar { get; set; }
}
