namespace user_management.Data.Client;

using MongoDB.Driver;
using user_management.Models;
using user_management.Services.Client;
using MongoDB.Bson;
using user_management.Services.Data;
using System.Collections.Generic;

public class ClientRepository : IClientRepository
{
    private readonly IMongoCollection<Client> _clientCollection;
    private readonly IMongoClient _mongoClient;

    public ClientRepository(IMongoClient mongoClient, MongoCollections mongoCollections)
    {
        _mongoClient = mongoClient;
        _clientCollection = mongoCollections.Clients;
    }

    public async Task<IEnumerable<Client>> RetrieveFirstPartyClients() => (await _clientCollection.FindAsync(Builders<Client>.Filter.Eq(Client.IS_FIRST_PARTY, true))).ToList();

    public async Task<Client> Create(Client client, IClientSessionHandle? session = null)
    {
        client.Id = ObjectId.GenerateNewId().ToString();

        try { await (session == null ? _clientCollection.InsertOneAsync(client) : _clientCollection.InsertOneAsync(session, client)); }
        catch (MongoDuplicateKeyException) { throw new DuplicationException(); }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { throw new DuplicationException(); }
        catch (Exception) { throw new DatabaseServerException(); }

        return client;
    }

    public async Task<Client?> RetrieveById(string clientId) => (await _clientCollection.FindAsync(Builders<Client>.Filter.Eq("_id", ObjectId.Parse(clientId)))).FirstOrDefault<Client?>();

    public async Task<Client?> RetrieveBySecret(string secret) => (await _clientCollection.FindAsync(Builders<Client>.Filter.Eq(Client.SECRET, secret))).FirstOrDefault<Client?>();

    public async Task<Client?> RetrieveByIdAndSecret(string clientId, string hashedSecret) => (await _clientCollection.FindAsync(Builders<Client>.Filter.And(Builders<Client>.Filter.Eq("_id", ObjectId.Parse(clientId)), Builders<Client>.Filter.Eq(Client.SECRET, hashedSecret)))).FirstOrDefault<Client?>();

    public async Task<Client?> RetrieveByIdAndRedirectUrl(string clientId, string redirectUrl) => (await _clientCollection.FindAsync(Builders<Client>.Filter.And(Builders<Client>.Filter.Eq("_id", ObjectId.Parse(clientId)), Builders<Client>.Filter.Eq(Client.REDIRECT_URL, redirectUrl)))).FirstOrDefault<Client?>();

    public async Task<bool> UpdateRedirectUrl(string redirectUrl, string clientId, string hashedSecret)
    {
        var filter = Builders<Client>.Filter.And(Builders<Client>.Filter.Eq("_id", ObjectId.Parse(clientId)), Builders<Client>.Filter.Eq(Client.SECRET, hashedSecret));
        var update = Builders<Client>.Update.Set(Client.REDIRECT_URL, redirectUrl).Set<Client, DateTime>(Client.UPDATED_AT, DateTime.UtcNow);

        UpdateResult r;
        try { r = await _clientCollection.UpdateOneAsync(filter, update); }
        catch (MongoDuplicateKeyException) { throw new DuplicationException(); }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { throw new DuplicationException(); }
        catch (Exception) { throw new DatabaseServerException(); }

        return r.IsAcknowledged && r.ModifiedCount == 1 && r.MatchedCount == 1;
    }

    public async Task<bool> DeleteBySecret(string secret, IClientSessionHandle? session = null)
    {
        DeleteResult r;
        try { r = await (session == null ? _clientCollection.DeleteOneAsync(Builders<Client>.Filter.Eq(Client.SECRET, secret)) : _clientCollection.DeleteOneAsync(session, Builders<Client>.Filter.Eq(Client.SECRET, secret))); }
        catch (Exception) { throw new DatabaseServerException(); }

        return r.IsAcknowledged && r.DeletedCount == 1;
    }

    public async Task<bool> DeleteById(string clientId, IClientSessionHandle? session = null)
    {
        DeleteResult r;
        try { r = await (session == null ? _clientCollection.DeleteOneAsync(Builders<Client>.Filter.Eq("_id", ObjectId.Parse(clientId))) : _clientCollection.DeleteOneAsync(session, Builders<Client>.Filter.Eq("_id", ObjectId.Parse(clientId)))); }
        catch (Exception) { throw new DatabaseServerException(); }

        return r.IsAcknowledged && r.DeletedCount == 1;
    }

    public async Task<bool?> ClientExposed(string clientId, string hashedSecret, string newHashedSecret, IClientSessionHandle? session = null)
    {
        Client? client = await RetrieveByIdAndSecret(clientId, hashedSecret);
        if (client == null) return null;
        return await ClientExposed(client, newHashedSecret, session);
    }

    public async Task<bool?> ClientExposed(Client client, string newHashedSecret, IClientSessionHandle? session = null)
    {
        try
        {
            if (session == null) session = await _mongoClient.StartSessionAsync();

            session.StartTransaction();

            if (!(await DeleteBySecret(client.Secret, session))) return null;

            client.Secret = newHashedSecret;
            client.ExposedCount++;
            client.TokensExposedAt = DateTime.UtcNow;
            client.UpdatedAt = DateTime.UtcNow;
            client.CreatedAt = DateTime.UtcNow;
            await Create(client, session);

            await session.CommitTransactionAsync();
        }
        catch (MongoDuplicateKeyException) { throw new DuplicationException(); }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { throw new DuplicationException(); }
        catch (Exception) { if (session != null) { await session.AbortTransactionAsync(); } throw new DatabaseServerException(); }
        finally { if (session != null) session.Dispose(); }

        return true;
    }
}
