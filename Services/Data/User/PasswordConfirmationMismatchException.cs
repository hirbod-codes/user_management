using System.Runtime.Serialization;

namespace user_management.Services.Data.User;

[Serializable]
public class PasswordConfirmationMismatchException : Exception
{
    public PasswordConfirmationMismatchException()
    {
    }

    public PasswordConfirmationMismatchException(string? message) : base(message)
    {
    }

    public PasswordConfirmationMismatchException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected PasswordConfirmationMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}