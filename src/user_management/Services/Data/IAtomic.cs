namespace user_management.Services;

public interface IAtomic
{
    public Task StartTransaction();
    public Task CommitTransaction();
    public Task AbortTransaction();
}
