namespace user_management.Authorization
{
    public interface IIdentifiable
    {
        Guid Identifier { get; }
    }
}