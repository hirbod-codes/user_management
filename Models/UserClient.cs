using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace user_management.Models;

public class UserClient : IEquatable<UserClient>
{
    [BsonElement(CLIENT_ID)]
    [BsonRequired]
    public ObjectId ClientId { get; set; }
    public const string CLIENT_ID = "client_id";

    [BsonElement(REFRESH_TOKEN)]
    public RefreshToken RefreshToken { get; set; } = null!;
    public const string REFRESH_TOKEN = "refresh_token";

    [BsonElement(TOKEN)]
    public Token Token { get; set; } = null!;
    public const string TOKEN = "token";

    public bool Equals(UserClient? other) =>
        other != null &&
        ClientId.ToString() == other.ClientId.ToString() &&
        Object.Equals(RefreshToken, other.RefreshToken) &&
        Object.Equals(Token, other.Token);

    public override bool Equals(object? obj) => obj != null && Equals((UserClient)obj);
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}