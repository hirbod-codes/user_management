namespace user_management.Utilities;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime ProvideUtcNow() => DateTime.UtcNow;
    public DateTime ProvideNow() => DateTime.Now;
}