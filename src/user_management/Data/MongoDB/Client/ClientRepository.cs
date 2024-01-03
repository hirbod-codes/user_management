namespace user_management.Data.MongoDB.Client;

using global::MongoDB.Driver;
using user_management.Models;
using user_management.Services.Data.Client;
using global::MongoDB.Bson;
using user_management.Services.Data;
using System.Collections.Generic;
using user_management.Data.MongoDB;

public class ClientRepository : MongoDBAtomicity, IClientRepository
{
    private readonly IMongoCollection<Client> _clientCollection;

    public ClientRepository(IMongoClient mongoClient, MongoCollections mongoCollections) : base(mongoClient) => _clientCollection = mongoCollections.Clients;

    public async Task<IEnumerable<Client>> RetrieveFirstPartyClients() => (await _clientCollection.FindAsync(Builders<Client>.Filter.Eq(Client.IS_FIRST_PARTY, true))).ToList();

    public async Task<Client> Create(Client client)
    {
        client.Id = ObjectId.GenerateNewId().ToString();

        try { await _clientCollection.InsertOneAsync(client); }
        catch (MongoDuplicateKeyException) { throw new DuplicationException(); }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { throw new DuplicationException(); }
        catch (Exception) { throw new DatabaseServerException(); }

        return client;
    }

    public async Task<Client> CreateWithTransaction(Client client)
    {
        client.Id = ObjectId.GenerateNewId().ToString();

        try { await _clientCollection.InsertOneAsync(_session, client); }
        catch (MongoDuplicateKeyException) { throw new DuplicationException(); }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { throw new DuplicationException(); }
        catch (Exception) { throw new DatabaseServerException(); }

        return client;
    }

    public async Task<Client?> RetrieveById(string clientId) => (await _clientCollection.FindAsync(Builders<Client>.Filter.Eq("_id", ObjectId.Parse(clientId)))).FirstOrDefault<Client?>();

    public async Task<Client?> RetrieveBySecret(string hashedSecret) => (await _clientCollection.FindAsync(Builders<Client>.Filter.Eq(Client.SECRET, hashedSecret))).FirstOrDefault<Client?>();

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

    public async Task<bool> DeleteBySecret(string hashedSecret)
    {
        DeleteResult r;
        try { r = await _clientCollection.DeleteOneAsync(Builders<Client>.Filter.Eq(Client.SECRET, hashedSecret)); }
        catch (Exception) { throw new DatabaseServerException(); }

        return r.IsAcknowledged && r.DeletedCount == 1;
    }

    public async Task<bool> DeleteBySecretWithTransaction(string hashedSecret)
    {
        DeleteResult r;
        try { r = await _clientCollection.DeleteOneAsync(_session, Builders<Client>.Filter.Eq(Client.SECRET, hashedSecret)); }
        catch (Exception) { throw new DatabaseServerException(); }

        return r.IsAcknowledged && r.DeletedCount == 1;
    }

    public async Task<bool> DeleteById(string clientId)
    {
        DeleteResult r;
        try { r = await _clientCollection.DeleteOneAsync(Builders<Client>.Filter.Eq("_id", ObjectId.Parse(clientId))); }
        catch (Exception) { throw new DatabaseServerException(); }

        return r.IsAcknowledged && r.DeletedCount == 1;
    }

    public async Task<bool> DeleteByIdWithTransaction(string clientId)
    {
        DeleteResult r;
        try { r = await _clientCollection.DeleteOneAsync(_session, Builders<Client>.Filter.Eq("_id", ObjectId.Parse(clientId))); }
        catch (Exception) { throw new DatabaseServerException(); }

        return r.IsAcknowledged && r.DeletedCount == 1;
    }

    public async Task<bool?> ClientExposed(string clientId, string hashedSecret, string newHashedSecret)
    {
        Client? client = await RetrieveByIdAndSecret(clientId, hashedSecret);
        if (client == null) return null;
        return await ClientExposed(client, newHashedSecret);
    }

    public async Task<bool?> ClientExposedWithTransaction(string clientId, string hashedSecret, string newHashedSecret)
    {
        Client? client = await RetrieveByIdAndSecret(clientId, hashedSecret);
        if (client == null) return null;
        return await ClientExposedWithTransaction(client, newHashedSecret);
    }

    public async Task<bool?> ClientExposed(Client client, string newHashedSecret)
    {
        try
        {
            await StartTransaction();

            if (!await DeleteBySecretWithTransaction(client.Secret)) return null;

            client.Secret = newHashedSecret;
            client.ExposedCount++;
            client.TokensExposedAt = DateTime.UtcNow;
            client.UpdatedAt = DateTime.UtcNow;
            client.CreatedAt = DateTime.UtcNow;
            await CreateWithTransaction(client);

            await CommitTransaction();
        }
        catch (MongoDuplicateKeyException) { throw new DuplicationException(); }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { throw new DuplicationException(); }
        catch (Exception) { await AbortTransaction(); throw new DatabaseServerException(); }

        return true;
    }

    public async Task<bool?> ClientExposedWithTransaction(Client client, string newHashedSecret)
    {
        try
        {
            if (!await DeleteBySecretWithTransaction(client.Secret)) return null;

            client.Secret = newHashedSecret;
            client.ExposedCount++;
            client.TokensExposedAt = DateTime.UtcNow;
            client.UpdatedAt = DateTime.UtcNow;
            client.CreatedAt = DateTime.UtcNow;
            await CreateWithTransaction(client);
        }
        catch (MongoDuplicateKeyException) { throw new DuplicationException(); }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { throw new DuplicationException(); }
        catch (Exception) { throw new DatabaseServerException(); }

        return true;
    }
}
