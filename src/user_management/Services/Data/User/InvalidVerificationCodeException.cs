using System.Runtime.Serialization;

namespace user_management.Services.Data.User;

[Serializable]
public class InvalidVerificationCodeException : Exception
{
    public InvalidVerificationCodeException()
    {
    }

    public InvalidVerificationCodeException(string? message) : base(message)
    {
    }

    public InvalidVerificationCodeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected InvalidVerificationCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}