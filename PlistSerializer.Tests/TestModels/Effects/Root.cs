using PlistSerializer.Attributes;

namespace PlistSerializer.Tests.TestModels.Effects;

public class Root
{
    [PlistName("AdjustmentLayers")]
    public Dictionary<string, Layer> Layers { get; set; }
}
