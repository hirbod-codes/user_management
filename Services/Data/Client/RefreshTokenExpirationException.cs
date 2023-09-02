using System.Runtime.Serialization;

namespace user_management.Services.Client;

[Serializable]
internal class RefreshTokenExpirationException : Exception
{
    public RefreshTokenExpirationException()
    {
    }

    public RefreshTokenExpirationException(string? message) : base(message)
    {
    }

    public RefreshTokenExpirationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected RefreshTokenExpirationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}