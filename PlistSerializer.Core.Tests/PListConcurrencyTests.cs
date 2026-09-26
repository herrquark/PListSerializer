using System.Collections.Concurrent;
using PListNet.Nodes;
using PListSerializer.Core.Tests.TestModels;

namespace PListSerializer.Core.Tests;

public class PListConcurrencyTests
{
    private const int Threads = 8;

    [Fact]
    public void Serialize_ManyNewTypesOnManyThreads_Test()
    {
        var objects = NewTypes(typeof(Holder<>)).Select(Activator.CreateInstance).ToArray();

        // each round, every thread serializes a different uncached type
        var errors = RunInRounds(objects.Length / Threads, (round, thread) => Serializer.Serialize(objects[round * Threads + thread]));

        Assert.Empty(errors);
    }

    [Fact]
    public void Deserialize_SameNewTypeOnManyThreads_Test()
    {
        var deserialize = typeof(Deserializer).GetMethod(nameof(Deserializer.Deserialize));
        var methods = NewTypes(typeof(ResolvedHolder<>)).Select(t => deserialize.MakeGenericMethod(t)).ToArray();
        var node = new DictionaryNode();

        // each round, every thread deserializes the same uncached type
        var errors = RunInRounds(methods.Length, (round, _) => methods[round].Invoke(null, [node]));

        Assert.Empty(errors);
    }

    // every closed generic type is new to the serializer's static caches
    private static IEnumerable<Type> NewTypes(Type genericDefinition)
        => typeof(object).Assembly.GetExportedTypes()
            .Where(t => t.IsClass && !t.ContainsGenericParameters)
            .Select(t => genericDefinition.MakeGenericType(t));

    // the barrier starts every thread on a round at the same moment
    private static ConcurrentQueue<Exception> RunInRounds(int rounds, Action<int, int> body)
    {
        var errors = new ConcurrentQueue<Exception>();
        using var barrier = new Barrier(Threads);

        var threads = Enumerable.Range(0, Threads)
            .Select(thread => new Thread(() =>
            {
                for (var round = 0; round < rounds; round++)
                {
                    barrier.SignalAndWait();
                    try { body(round, thread); }
                    catch (Exception e) { errors.Enqueue(e); }
                }
            }))
            .ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        return errors;
    }
}
