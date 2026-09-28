using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using PlistSerializer.Attributes;
using PlistSerializer.Nodes;
using PlistSerializer.Tests.TestModels;

namespace PlistSerializer.Tests;

public class PlistMemoryTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public void Mapping_CollectibleType_Test(bool serialize, bool resolver)
    {
        var references = UseCollectibleType(serialize, resolver);

        Collect();

        Assert.All(references, reference => Assert.False(reference.IsAlive));
    }

    [Theory]
    [InlineData(PlistFormat.Xml, 1, 1000)]
    [InlineData(PlistFormat.Binary, 1, 1000)]
    [InlineData(PlistFormat.Xml, 4096, 4)]
    [InlineData(PlistFormat.Binary, 4096, 4)]
    public void RoundTrip_ReleasesInputsAndOutputs_Test(PlistFormat format, int count, int iterations)
    {
        var references = Enumerable.Range(0, iterations)
            .SelectMany(_ => RoundTrip(format, count)).ToArray();

        Collect();

        Assert.All(references, reference => Assert.False(reference.IsAlive));
    }

    [Theory]
    [InlineData(PlistFormat.Xml)]
    [InlineData(PlistFormat.Binary)]
    public void RoundTrip_ReleasesLargeData_Test(PlistFormat format)
    {
        var references = Enumerable.Range(0, 4).SelectMany(_ => RoundTripData(format)).ToArray();

        Collect();

        Assert.All(references, reference => Assert.False(reference.IsAlive));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference[] RoundTripData(PlistFormat format)
    {
        var data = new byte[1024 * 1024];
        Array.Fill(data, (byte)123);
        var node = Serializer.Serialize(data);
        using var stream = new MemoryStream();
        Plist.Save(node, stream, format);
        stream.Position = 0;
        var loaded = Plist.Load(stream);
        var result = Deserializer.Deserialize<byte[]>(loaded);
        Assert.Equal(data, result);

        return [new(data), new(node), new(stream), new(stream.GetBuffer()), new(loaded), new(result)];
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference[] UseCollectibleType(bool serialize, bool resolver)
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("CollectibleModel"), AssemblyBuilderAccess.RunAndCollect);
        var builder = assembly.DefineDynamicModule("Models").DefineType("Model", TypeAttributes.Public, typeof(SimpleClass));
        // The cached PropertyInfo and getter refer back to the collectible type itself.
        var self = builder.DefineProperty("Self", PropertyAttributes.None, builder, null);
        var getter = builder.DefineMethod("get_Self", MethodAttributes.Public | MethodAttributes.SpecialName, builder, Type.EmptyTypes);
        var getterIl = getter.GetILGenerator();
        getterIl.Emit(OpCodes.Ldnull);
        getterIl.Emit(OpCodes.Ret);
        self.SetGetMethod(getter);
        var setter = builder.DefineMethod("set_Self", MethodAttributes.Public | MethodAttributes.SpecialName, typeof(void), [builder]);
        setter.GetILGenerator().Emit(OpCodes.Ret);
        self.SetSetMethod(setter);
        if (resolver)
            builder.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(PlistTypeResolverAttribute).GetConstructor([typeof(Type)]), [typeof(NullResolver)]));
        var type = builder.CreateType();
        var model = Activator.CreateInstance(type);
        ((SimpleClass)model).Name = "collectible";
        var node = new DictionaryNode { ["Name"] = new StringNode("collectible"), ["Self"] = new NullNode() };

        if (serialize)
            Assert.Equal("collectible", Assert.IsType<StringNode>(Assert.IsType<DictionaryNode>(Serializer.Serialize(model))["Name"]).Value);
        else
        {
            var method = typeof(Deserializer).GetMethod(nameof(Deserializer.Deserialize)).MakeGenericMethod(type);
            Assert.Equal("collectible", ((SimpleClass)method.Invoke(null, [node])).Name);
        }

        return [new WeakReference(type), new WeakReference(model)];
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference[] RoundTrip(PlistFormat format, int count)
    {
        var models = Enumerable.Range(0, count).Select(i => new SimpleClass { Id = i, Name = "device-" + i }).ToList();
        var node = Serializer.Serialize(models);
        using var stream = new MemoryStream();
        Plist.Save(node, stream, format);
        stream.Position = 0;
        var loaded = Plist.Load(stream);
        var result = Deserializer.Deserialize<List<SimpleClass>>(loaded);
        Assert.Equal(count, result.Count);
        Assert.Equal(models[^1].Name, result[^1].Name);

        var originalItem = (DictionaryNode)((ArrayNode)node)[0];
        var loadedItem = (DictionaryNode)((ArrayNode)loaded)[0];
        return
        [
            new(models), new(models[0]), new(models[0].Name), new(node), new(originalItem),
            new(originalItem["Name"]), new(originalItem["Id"]), new(stream), new(stream.GetBuffer()),
            new(loaded), new(loadedItem), new(loadedItem["Name"]), new(loadedItem["Id"]),
            new(result), new(result[0]), new(result[0].Name)
        ];
    }

    private static void Collect()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
