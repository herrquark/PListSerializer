using PlistSerializer.Core.Attributes;
using PlistSerializer.Core.Tests.TestModels.Effects;

namespace PlistSerializer.Core.Tests.TestModels;

public class RootPlist
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
