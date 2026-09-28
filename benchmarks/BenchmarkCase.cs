namespace PlistSerializer.Performance;

internal sealed class BenchmarkCase(string name, Func<object> run, Action<object> validate)
{
    public string Name { get; } = name;

    public Func<object> Run { get; } = run;

    public Action<object> Validate { get; } = validate;
}
