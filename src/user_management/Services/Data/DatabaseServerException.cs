using System.Runtime.Serialization;

namespace user_management.Services.Data;

[Serializable]
public class DatabaseServerException : Exception
{
    public DatabaseServerException()
    {
    }

    public DatabaseServerException(string? message) : base(message)
    {
    }

    public DatabaseServerException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected DatabaseServerException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
