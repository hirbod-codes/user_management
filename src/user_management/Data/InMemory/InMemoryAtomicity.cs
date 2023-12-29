using user_management.Services;

namespace user_management.Data.InMemory;

public class InMemoryAtomicity : IAtomic
{
    public InMemoryContext InMemoryContext { get; }

    public InMemoryAtomicity(InMemoryContext inMemoryContext) => InMemoryContext = inMemoryContext;

    public async Task AbortTransaction() => await InMemoryContext.Database.RollbackTransactionAsync();

    public async Task CommitTransaction() => await InMemoryContext.Database.CommitTransactionAsync();

    public async Task StartTransaction() => await InMemoryContext.Database.BeginTransactionAsync();
}
