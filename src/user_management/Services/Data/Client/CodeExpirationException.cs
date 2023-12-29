using System.Runtime.Serialization;

namespace user_management.Services.Data.Client;

[Serializable]
public class CodeExpirationException : Exception
{
    public CodeExpirationException()
    {
    }

    public CodeExpirationException(string? message) : base(message)
    {
    }

    public CodeExpirationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CodeExpirationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
