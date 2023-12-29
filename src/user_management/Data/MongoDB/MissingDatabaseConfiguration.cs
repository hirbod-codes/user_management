using System.Runtime.Serialization;

namespace user_management.Data.MongoDB;

[Serializable]
public class MissingDatabaseConfiguration : Exception
{
    public MissingDatabaseConfiguration()
    {
    }

    public MissingDatabaseConfiguration(string? message) : base(message)
    {
    }

    public MissingDatabaseConfiguration(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected MissingDatabaseConfiguration(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
