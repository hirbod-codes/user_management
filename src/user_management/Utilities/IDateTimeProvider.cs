namespace user_management.Utilities;

public interface IDateTimeProvider
{
    public DateTime ProvideUtcNow();
    public DateTime ProvideNow();
}