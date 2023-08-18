using System.Runtime.Serialization;

namespace user_management.Services.Data.User;

[Serializable]
public class UnverifiedUserException : Exception
{
    public UnverifiedUserException()
    {
    }

    public UnverifiedUserException(string? message) : base(message)
    {
    }

    public UnverifiedUserException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected UnverifiedUserException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}