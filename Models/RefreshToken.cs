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

    [BsonElement(CODE)]
    [BsonRequired]
    public string? Code { get; set; } // MUST BE UNIQUE
    public const string CODE = "code";

    [BsonElement(CODE_EXPIRES_AT)]
    [BsonRequired]
    public DateTime? CodeExpiresAt { get; set; }
    public const string CODE_EXPIRES_AT = "code_expires_at";

    [BsonElement(CODE_CHALLENGE)]
    [BsonRequired]
    public string CodeChallenge { get; set; } = null!;
    public const string CODE_CHALLENGE = "code_challenge";

    [BsonElement(CODE_CHALLENGE_METHOD)]
    [BsonRequired]
    public string CodeChallengeMethod { get; set; } = null!;
    public const string CODE_CHALLENGE_METHOD = "code_challenge_method";

    [BsonElement(VALUE)]
    [BsonRequired]
    public string Value { get; set; } = null!; // Not hashed, MUST BE UNIQUE
    public const string VALUE = "value";

    [BsonElement(IS_VERIFIED)]
    [BsonRequired]
    public bool IsVerified { get; set; } = false;
    public const string IS_VERIFIED = "is_verified";

    [BsonElement(EXPIRATION_DATE)]
    [BsonRequired]
    public DateTime ExpirationDate { get; set; }
    public const string EXPIRATION_DATE = "expiration_date";

    public bool Equals(RefreshToken? other) =>
        other != null &&
        Code == other.Code &&
        CodeExpiresAt == other.CodeExpiresAt &&
        CodeChallenge == other.CodeChallenge &&
        CodeChallengeMethod == other.CodeChallengeMethod &&
        Value == other.Value &&
        IsVerified == other.IsVerified;

    public override bool Equals(object? obj) => obj != null && Equals((RefreshToken)obj);
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}