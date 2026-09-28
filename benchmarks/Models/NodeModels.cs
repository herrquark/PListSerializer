using PlistSerializer.Nodes;

namespace PlistSerializer.Performance.Models;

public class NodeModel
{
    public PNode Any { get; set; } = new DictionaryNode { ["value"] = new StringNode("embedded") };

    public ArrayNode Array { get; set; } = new() { new IntegerNode(1), new StringNode("two") };

    public DictionaryNode Dictionary { get; set; } = new() { [""] = new BooleanNode(false) };

    public StringNode String { get; set; } = new("node text");

    public IntegerNode Integer { get; set; } = new(long.MaxValue);

    public RealNode Real { get; set; } = new(1.25);

    public BooleanNode Boolean { get; set; } = new(true);

    public DateNode Date { get; set; } = new(new DateTime(2020, 1, 1, 0, 0, 0, 500, DateTimeKind.Utc));

    public DataNode Data { get; set; } = new([1, 2, 3]);

    public UidNode Uid { get; set; } = new(uint.MaxValue);
}
