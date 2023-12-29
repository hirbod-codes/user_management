namespace user_management.Data.MongoDB;

using global::MongoDB.Driver;
using user_management.Services;

public class MongoDBAtomicity : IAtomic
{
    protected static IClientSessionHandle? _session = null;
    protected readonly IMongoClient _mongoClient;

    public MongoDBAtomicity(IMongoClient mongoClient) => _mongoClient = mongoClient;

    public async Task StartTransaction() => _session = await _mongoClient.StartSessionAsync();

    public async Task CommitTransaction()
    {
        if (_session is null)
            throw new InvalidOperationException();

        await _session.CommitTransactionAsync();
    }

    public async Task AbortTransaction()
    {
        if (_session is not null)
            await _session.AbortTransactionAsync();

        _session = null;
    }
}
