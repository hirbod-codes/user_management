using System.Runtime.Serialization;

namespace user_management.Services.Data.Client;

[Serializable]
public class ExposedClientException : Exception
{
    public ExposedClientException()
    {
    }

    public ExposedClientException(string? message) : base(message)
    {
    }

    public ExposedClientException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ExposedClientException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
