using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace user_management.Models;

public class UserClient
{
    [BsonElement(CLIENT_ID)]
    [BsonRequired]
    public ObjectId ClientId { get; set; }
    public const string CLIENT_ID = "client_id";

    [BsonElement(REFRESH_TOKEN)]
    [BsonRequired]
    public RefreshToken? RefreshToken { get; set; }
    public const string REFRESH_TOKEN = "refresh_token";

    [BsonElement(TOKEN)]
    public Token? Token { get; set; }
    public const string TOKEN = "token";
}