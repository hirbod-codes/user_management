namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class Client
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonRequired]
    public ObjectId? Id { get; set; }

    [BsonElement(SECRET)]
    [BsonRequired]
    public string? Secret { get; set; }
    public const string SECRET = "secret";

    [BsonElement(REDIRECT_URL)]
    [BsonRequired]
    public string RedirectUrl { get; set; } = null!;
    public const string REDIRECT_URL = "redirect_url";

    [BsonElement(CREATED_AT)]
    [BsonRequired]
    public DateTime? CreatedAt { get; set; }
    public const string CREATED_AT = "created_at";

    [BsonElement(UPDATED_AT)]
    [BsonRequired]
    public DateTime? UpdatedAt { get; set; }
    public const string UPDATED_AT = "updated_at";
}