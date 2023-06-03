namespace user_management.Authorization
{
    internal interface IIdentifiable
    {
        Guid Identifier { get; }
    }
}