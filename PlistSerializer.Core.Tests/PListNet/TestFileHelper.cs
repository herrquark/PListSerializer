using System.Reflection;

namespace PlistSerializer.Core.Tests;

public static class TestFileHelper
{
    public static Stream GetTestFileStream(string relativeFilePath)
    {
        const char namespaceSeparator = '.';

        // get calling assembly
        var assembly = Assembly.GetCallingAssembly();

        // compute resource name suffix (replace Windows/Unix directory separators with namespace separator)
        var relativeName = "." + relativeFilePath
            .Replace('/', namespaceSeparator)
            .Replace('\\', namespaceSeparator)
            .Replace(' ', '_');

        // get resource stream
        var fullName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(relativeName, StringComparison.InvariantCulture))
            ?? throw new Exception($"Unable to find resource for path \"{relativeFilePath}\". Resource with name ending on \"{relativeName}\" was not found in assembly.");

        var stream = assembly.GetManifestResourceStream(fullName)
            ?? throw new Exception($"Unable to find resource for path \"{relativeFilePath}\". Resource named \"{fullName}\" was not found in assembly.");

        return stream;
    }
}
