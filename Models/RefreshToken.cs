namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class RefreshToken
{
    [BsonElement(TOKEN_PRIVILEGES)]
    [BsonRequired]
    public TokenPrivileges? TokenPrivileges { get; set; }
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
    public string? CodeChallenge { get; set; }
    public const string CODE_CHALLENGE = "code_challenge";

    [BsonElement(CODE_CHALLENGE_METHOD)]
    [BsonRequired]
    public string? CodeChallengeMethod { get; set; }
    public const string CODE_CHALLENGE_METHOD = "code_challenge_method";

    [BsonElement(VALUE)]
    [BsonRequired]
    public string? Value { get; set; } // Not hashed, MUST BE UNIQUE
    public const string VALUE = "value";

    [BsonElement(IS_VERIFIED)]
    [BsonRequired]
    public bool IsVerified { get; set; } = false;
    public const string IS_VERIFIED = "is_verified";

    [BsonElement(EXPIRATION_DATE)]
    [BsonRequired]
    public DateTime? ExpirationDate { get; set; }
    public const string EXPIRATION_DATE = "expiration_date";
}