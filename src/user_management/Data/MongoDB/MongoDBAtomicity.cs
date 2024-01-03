namespace user_management.Data.MongoDB;

using global::MongoDB.Driver;
using user_management.Services;

public class MongoDBAtomicity : IAtomic
{
    protected static IClientSessionHandle? _session = null;
    protected readonly IMongoClient _mongoClient;

    public MongoDBAtomicity(IMongoClient mongoClient) => _mongoClient = mongoClient;

    public async Task StartTransaction()
    {
        _session = await _mongoClient.StartSessionAsync();
        _session.StartTransaction();
    }

    public async Task CommitTransaction()
    {
        if (_session is null || !_session.IsInTransaction)
            throw new InvalidOperationException();

        await _session.CommitTransactionAsync();
        _session.Dispose();
    }

    public async Task AbortTransaction()
    {
        if (_session is not null)
        {
            if (_session.IsInTransaction)
                await _session.AbortTransactionAsync();
            _session.Dispose();
        }

    }
}
