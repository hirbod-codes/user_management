using System.Runtime.Serialization;

namespace user_management.Services.Data.Client;

[Serializable]
public class BannedClientException : Exception
{
    public BannedClientException()
    {
    }

    public BannedClientException(string? message) : base(message)
    {
    }

    public BannedClientException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected BannedClientException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
