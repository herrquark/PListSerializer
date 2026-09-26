using PListSerializer.Core.Attributes;
using PListSerializer.Core.Tests.TestModels.Effects;

namespace PListSerializer.Core.Tests.TestModels;

public class RootPList
{
    [PlistName("group_identifier")]
    public string GroupIdentifier { get; set; }

    [PlistName("kMPPresetIdentifierKey")]
    public string PresetIdentifierKey { get; set; }

    [PlistName("priority")]
    public int Priority { get; set; }

    public bool Hidden { get; set; }

    [PlistName("uuid")]
    public string Id { get; set; }

    [PlistName("AdjustmentLayers")]
    public Layer[] AdjustmentLayers { get; set; }
}
