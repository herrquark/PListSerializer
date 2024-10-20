using PListSerializer.Core.Attributes;

namespace PListSerializer.Core.Tests.TestModels.Effects;

public class Root
{
    [PlistName("AdjustmentLayers")]
    public Dictionary<string, Layer> Layers { get; set; }
}
