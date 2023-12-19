using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace user_management.Models;

public class AuthorizingClient : IEquatable<AuthorizingClient>
{
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement(CLIENT_ID)]
    public string ClientId { get; set; } = null!;
    public const string CLIENT_ID = "client_id";

    [BsonElement(TOKEN_PRIVILEGES)]
    [BsonRequired]
    public TokenPrivileges TokenPrivileges { get; set; } = new();
    public const string TOKEN_PRIVILEGES = "token_privileges";

    [BsonElement(CODE)]
    public string Code { get; set; } = null!; // MUST BE UNIQUE
    public const string CODE = "code";

    [BsonElement(CODE_EXPIRES_AT)]
    [BsonRequired]
    public DateTime CodeExpiresAt { get; set; }
    public const string CODE_EXPIRES_AT = "code_expires_at";

    [BsonElement(CODE_CHALLENGE)]
    [BsonRequired]
    public string CodeChallenge { get; set; } = null!;
    public const string CODE_CHALLENGE = "code_challenge";

    [BsonElement(CODE_CHALLENGE_METHOD)]
    [BsonRequired]
    public string CodeChallengeMethod { get; set; } = null!;
    public const string CODE_CHALLENGE_METHOD = "code_challenge_method";

    public bool Equals(AuthorizingClient? other) =>
        other != null
        && Code == other.Code
        && CodeChallenge == other.CodeChallenge
        && CodeChallengeMethod == other.CodeChallengeMethod
        && CodeExpiresAt == other.CodeExpiresAt;

    public override bool Equals(object? obj) => obj != null && Equals(obj as AuthorizingClient);

    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}
