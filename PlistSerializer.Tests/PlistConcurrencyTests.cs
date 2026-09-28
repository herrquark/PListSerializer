using System.Collections.Concurrent;
using PlistSerializer.Nodes;
using PlistSerializer.Tests.TestModels;

namespace PlistSerializer.Tests;

public class PlistConcurrencyTests
{
    private const int Threads = 8;

    [Theory]
    [InlineData(PlistFormat.Xml)]
    [InlineData(PlistFormat.Binary)]
    public void RoundTrip_ManySmallPlistsOnManyThreads_Test(PlistFormat format)
    {
        var errors = RunInRounds(1000, (round, thread) =>
        {
            var model = new SimpleClass { Id = round * Threads + thread, Name = $"device-{thread}-{round}" };
            using var stream = new MemoryStream();
            Plist.Save(Serializer.Serialize(model), stream, format);
            stream.Position = 0;
            var result = Deserializer.Deserialize<SimpleClass>(Plist.Load(stream));
            Assert.Equal(model.Id, result.Id);
            Assert.Equal(model.Name, result.Name);
        });

        Assert.Empty(errors);
    }

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

    [Fact]
    public void Deserialize_NewPropertyMapsOnManyThreads_Test()
    {
        var deserialize = typeof(Deserializer).GetMethod(nameof(Deserializer.Deserialize));
        var methods = NewTypes(typeof(Holder<>)).Select(t => deserialize.MakeGenericMethod(t)).ToArray();
        var node = new DictionaryNode { ["Value"] = new NullNode() };

        var errors = RunInRounds(methods.Length, (round, _) => methods[round].Invoke(null, [node]));

        Assert.Empty(errors);
    }

    [Fact]
    public void Serialize_SameNewTypeOnManyThreads_Test()
    {
        var objects = NewTypes(typeof(Holder<>))
            .Select(t => typeof(Holder<>).MakeGenericType(t)).Select(Activator.CreateInstance).ToArray();

        var errors = RunInRounds(objects.Length, (round, _) => Serializer.Serialize(objects[round]));

        Assert.Empty(errors);
    }

    [Fact]
    public void Deserialize_ConstructsSharedResolverOnce_Test()
    {
        var node = new DictionaryNode();
        var errors = RunInRounds(1, (_, _) => Deserializer.Deserialize<CountedResolvedHolder>(node));

        Assert.Multiple(() =>
        {
            Assert.Empty(errors);
            Assert.Equal(1, CountingResolver.Instances);
        });
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
