namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class Token
{
    [BsonElement(VALUE)]
    [BsonRequired]
    public string? Value { get; set; }
    public const string VALUE = "value";

    [BsonElement(EXPIRATION_DATE)]
    [BsonRequired]
    public DateTime? ExpirationDate { get; set; }
    public const string EXPIRATION_DATE = "expiration_date";

    [BsonElement(IS_REVOKED)]
    [BsonRequired]
    public bool? IsRevoked { get; set; }
    public const string IS_REVOKED = "is_revoked";
}