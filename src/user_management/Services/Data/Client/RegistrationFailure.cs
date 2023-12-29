using System.Runtime.Serialization;

namespace user_management.Services.Data.Client;

[Serializable]
public class RegistrationFailure : Exception
{
    public RegistrationFailure()
    {
    }

    public RegistrationFailure(string? message) : base(message)
    {
    }

    public RegistrationFailure(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected RegistrationFailure(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
