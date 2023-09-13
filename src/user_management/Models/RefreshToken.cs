namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class RefreshToken : IEquatable<RefreshToken>
{
    [BsonElement(TOKEN_PRIVILEGES)]
    [BsonRequired]
    public TokenPrivileges TokenPrivileges { get; set; } = new();
    public const string TOKEN_PRIVILEGES = "token_privileges";

    [BsonElement(VALUE)]
    [BsonRequired]
    public string Value { get; set; } = null!; // hashed, MUST BE UNIQUE
    public const string VALUE = "value";

    [BsonElement(EXPIRATION_DATE)]
    [BsonRequired]
    public DateTime ExpirationDate { get; set; }
    public const string EXPIRATION_DATE = "expiration_date";

    public bool Equals(RefreshToken? other) =>
        other != null &&
        Value == other.Value;

    public override bool Equals(object? obj) => obj != null && Equals(obj as RefreshToken);
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}