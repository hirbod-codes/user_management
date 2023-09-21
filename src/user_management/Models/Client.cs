namespace user_management.Models;

using Bogus;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using user_management.Utilities;

[BsonIgnoreExtraElements]
public class Client
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonRequired]
    public ObjectId Id { get; set; }

    [BsonElement(SECRET)]
    [BsonRequired]
    public string Secret { get; set; } = null!;
    public const string SECRET = "secret";

    [BsonElement(REDIRECT_URL)]
    [BsonRequired]
    public string RedirectUrl { get; set; } = null!;
    public const string REDIRECT_URL = "redirect_url";

    [BsonElement(TOKENS_EXPOSED_AT)]
    [BsonRequired]
    public DateTime? TokensExposedAt { get; set; } = null;
    public const string TOKENS_EXPOSED_AT = "tokens_exposed_at";

    [BsonElement(EXPOSED_COUNT)]
    [BsonRequired]
    public int ExposedCount { get; set; } = 0;
    public const string EXPOSED_COUNT = "exposed_count";

    [BsonElement(CREATED_AT)]
    [BsonRequired]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public const string CREATED_AT = "created_at";

    [BsonElement(UPDATED_AT)]
    [BsonRequired]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public const string UPDATED_AT = "updated_at";

    public static Client FakeClient(out string secret, IEnumerable<Client>? clients = null, DateTime? creationDateTime = null)
    {
        if (clients == null) clients = new Client[] { };
        if (creationDateTime == null) creationDateTime = DateTime.UtcNow;
        ObjectId id = ObjectId.GenerateNewId();

        Faker faker = new Faker("en");

        int safety = 0;
        string redirectUrl;
        string? hashedSecret;
        do
        {
            secret = (new StringHelper()).GenerateRandomString(128);
            hashedSecret = (new StringHelper()).HashWithoutSalt(secret);
            redirectUrl = faker.Internet.Url();

            if (
                clients.FirstOrDefault<Client?>(c => c != null && c.Secret == hashedSecret) == null
                && clients.FirstOrDefault<Client?>(c => c != null && c.RedirectUrl == redirectUrl) == null
                && clients.FirstOrDefault<Client?>(c => c != null && c.Id == id) == null
            )
                break;

            safety++;
        } while (safety < 200);

        return new Client() { Id = id, Secret = hashedSecret!, RedirectUrl = redirectUrl, CreatedAt = (DateTime)creationDateTime, UpdatedAt = (DateTime)creationDateTime };
    }
}
