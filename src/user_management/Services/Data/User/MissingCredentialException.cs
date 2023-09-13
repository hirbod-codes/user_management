using System.Runtime.Serialization;

namespace user_management.Services.Data.User;

[Serializable]
public class MissingCredentialException : Exception
{
    public MissingCredentialException()
    {
    }

    public MissingCredentialException(string? message) : base(message)
    {
    }

    public MissingCredentialException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected MissingCredentialException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}