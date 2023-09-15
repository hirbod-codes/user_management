using System.Runtime.Serialization;

namespace user_management.Services.Data.Client;

[Serializable]
public class UnverifiedRefreshTokenException : Exception
{
    public UnverifiedRefreshTokenException()
    {
    }

    public UnverifiedRefreshTokenException(string? message) : base(message)
    {
    }

    public UnverifiedRefreshTokenException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected UnverifiedRefreshTokenException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
