namespace PlistSerializer.Core;

/// <summary>
/// The format of a plist file.
/// </summary>
public enum PlistFormat
{
    /// <summary>
    /// The binary format, which starts with <c>bplist00</c>.
    /// </summary>
    Binary,

    /// <summary>
    /// The XML format.
    /// </summary>
    Xml
}
