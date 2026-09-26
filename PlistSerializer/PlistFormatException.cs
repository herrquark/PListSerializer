using System.Runtime.Serialization;

namespace PlistSerializer;

/// <summary>
/// The exception thrown when a plist cannot be read or written.
/// </summary>
public class PlistFormatException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlistFormatException"/> class.
    /// </summary>
    public PlistFormatException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlistFormatException"/> class with a message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PlistFormatException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlistFormatException"/> class with a message and the exception that caused it.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The exception that caused this one.</param>
    public PlistFormatException(string message, Exception inner) : base(message, inner)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlistFormatException"/> class with serialized data.
    /// </summary>
    /// <param name="info">The serialized object data.</param>
    /// <param name="context">The source or destination of the data.</param>
    protected PlistFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
