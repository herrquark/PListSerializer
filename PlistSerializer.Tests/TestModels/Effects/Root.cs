using PlistSerializer.Core.Attributes;

namespace PlistSerializer.Core.Tests.TestModels.Effects;

public class Root
{
    [PlistName("AdjustmentLayers")]
    public Dictionary<string, Layer> Layers { get; set; }
}
