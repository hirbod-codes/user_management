using System.Runtime.Serialization;

namespace user_management.Services;

[Serializable]
public class OperationException : Exception
{
    public OperationException()
    {
    }

    public OperationException(string? message) : base(message)
    {
    }

    public OperationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected OperationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}