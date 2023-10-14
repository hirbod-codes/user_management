using System.Runtime.Serialization;

namespace user_management.Services.Data.Client;

[Serializable]
public class ExpiredRefreshTokenException : Exception
{
    public ExpiredRefreshTokenException()
    {
    }

    public ExpiredRefreshTokenException(string? message) : base(message)
    {
    }

    public ExpiredRefreshTokenException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ExpiredRefreshTokenException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
