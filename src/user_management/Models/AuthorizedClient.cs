using Bogus;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using user_management.Data;

namespace user_management.Models;

/// <summary>
/// AKA Authorized clients by the user the has an object of this class.
/// </summary>
public class AuthorizedClient : IEquatable<AuthorizedClient>
{
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement(CLIENT_ID)]
    [BsonRequired]
    public string ClientId { get; set; } = null!;
    public const string CLIENT_ID = "client_id";

    [BsonElement(REFRESH_TOKEN)]
    public RefreshToken RefreshToken { get; set; } = null!;
    public const string REFRESH_TOKEN = "refresh_token";

    [BsonElement(TOKEN)]
    public Token Token { get; set; } = null!;
    public const string TOKEN = "token";

    public static AuthorizedClient FakeAuthorizedClient(Client client, IEnumerable<Privilege>? privileges = null, bool includeAllPrivileges = false)
    {
        Faker faker = new();

        if (privileges == null) privileges = StaticData.Privileges;

        Privilege[] pickedPrivileges;
        if (includeAllPrivileges) pickedPrivileges = privileges.ToArray();
        else pickedPrivileges = faker.PickRandom<Privilege>(privileges, faker.Random.Int(0, privileges.Count())).ToArray();

        // the ObjectId's GenerateNewId static method is utilized in order to ensure uniqueness of unique fields
        return new()
        {
            ClientId = client.Id,
            RefreshToken = new()
            {
                ExpirationDate = faker.Date.Between(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(14)),
                Value = ObjectId.GenerateNewId().ToString(),
                TokenPrivileges = new()
                {
                    Privileges = pickedPrivileges,
                    ReadsFields = pickedPrivileges.FirstOrDefault(p => p != null && (p.Name == StaticData.READ_ACCOUNT || p.Name == StaticData.READ_ACCOUNTS) && p.Value != null && (bool)p.Value == true) == null ? new Field[] { } : faker.PickRandom<Field>(User.GetReadableFields(), faker.Random.Int(0, User.GetReadableFields().Count())).ToArray(),
                    UpdatesFields = pickedPrivileges.FirstOrDefault(p => p != null && (p.Name == StaticData.UPDATE_ACCOUNT || p.Name == StaticData.UPDATE_ACCOUNTS) && p.Value != null && (bool)p.Value == true) == null ? new Field[] { } : faker.PickRandom<Field>(User.GetUpdatableFields(), faker.Random.Int(0, User.GetUpdatableFields().Count())).ToArray(),
                    DeletesUser = pickedPrivileges.FirstOrDefault(p => p != null && (p.Name == StaticData.DELETE_ACCOUNT || p.Name == StaticData.DELETE_ACCOUNTS) && p.Value != null && (bool)p.Value == true) == null ? false : faker.Random.Bool(0.3f)
                }
            },
            Token = new()
            {
                IsRevoked = faker.Random.Bool(0.2f),
                ExpirationDate = faker.Date.Between(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(14)),
                Value = ObjectId.GenerateNewId().ToString()
            }
        };
    }

    public bool Equals(AuthorizedClient? other) =>
        other != null &&
        ClientId.ToString() == other.ClientId.ToString() &&
        Object.Equals(RefreshToken, other.RefreshToken) &&
        Object.Equals(Token, other.Token);

    public override bool Equals(object? obj) => obj != null && Equals(obj as AuthorizedClient);

    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}
