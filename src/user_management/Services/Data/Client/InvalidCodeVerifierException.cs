using System.Runtime.Serialization;

namespace user_management.Services.Client;

[Serializable]
internal class InvalidCodeVerifierException : Exception
{
    public InvalidCodeVerifierException()
    {
    }

    public InvalidCodeVerifierException(string? message) : base(message)
    {
    }

    public InvalidCodeVerifierException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected InvalidCodeVerifierException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}