namespace user_management.Data.Client;

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using user_management.Models;
using user_management.Data;
using MongoDB.Bson;

public class ClientRepository : IClientRepository
{
    private readonly IMongoCollection<Client> _clientCollection;

    public ClientRepository(IOptions<MongoContext> mongoContext)
    {
        var mongoClient = MongoContext.GetMongoClient(mongoContext.Value);

        var mongoDatabase = mongoClient.GetDatabase(mongoContext.Value.DatabaseName);

        _clientCollection = mongoDatabase.GetCollection<Client>(mongoContext.Value.Collections.Clients);
    }

    public async Task<Client> Create(Client client)
    {
        client.Id = ObjectId.GenerateNewId();

        DateTime dt = DateTime.UtcNow;
        client.UpdatedAt = dt;
        client.CreatedAt = dt;

        await _clientCollection.InsertOneAsync(client);

        return client;
    }

    public async Task<Client?> RetrieveBySecret(string secret) => (await _clientCollection.FindAsync(Builders<Client>.Filter.Eq(Client.SECRET, secret))).FirstOrDefault<Client?>();

    public async Task<Client?> RetrieveByIdAndRedirectUrl(ObjectId id, string redirectUrl) => (await _clientCollection.FindAsync(Builders<Client>.Filter.And(Builders<Client>.Filter.Eq("_id", id), Builders<Client>.Filter.Eq(Client.REDIRECT_URL, redirectUrl)))).FirstOrDefault<Client?>();

    public async Task<Client?> RetrieveById(ObjectId id) => (await _clientCollection.FindAsync(Builders<Client>.Filter.Eq("_id", id))).FirstOrDefault<Client?>();

    public async Task<Client?> RetrieveByIdAndSecret(ObjectId clientId, string hashedSecret) => (await _clientCollection.FindAsync(Builders<Client>.Filter.And(Builders<Client>.Filter.Eq("_id", clientId), Builders<Client>.Filter.Eq(Client.SECRET, hashedSecret)))).FirstOrDefault<Client?>();

    public async Task<bool> UpdateRedirectUrl(string redirectUrl, ObjectId id, string hashedSecret)
    {
        UpdateResult r = await _clientCollection.UpdateOneAsync(Builders<Client>.Filter.And(Builders<Client>.Filter.Eq("_id", id), Builders<Client>.Filter.Eq(Client.SECRET, hashedSecret)), Builders<Client>.Update.Set(Client.REDIRECT_URL, redirectUrl));
        return r.IsAcknowledged && r.ModifiedCount == 1 && r.MatchedCount == 1;
    }

    public async Task<bool> DeleteBySecret(string secret)
    {
        DeleteResult r = await _clientCollection.DeleteOneAsync(Builders<Client>.Filter.Eq(Client.SECRET, secret));
        return r.IsAcknowledged && r.DeletedCount == 1;
    }
}