namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonRequired]
    public ObjectId? Id { get; set; }

    [BsonElement(FIRST_NAME)]
    public string? FirstName { get; set; }
    public const string FIRST_NAME = "first_name";

    [BsonElement(MIDDLE_NAME)]
    public string? MiddleName { get; set; }
    public const string MIDDLE_NAME = "middle_name";

    [BsonElement(LAST_NAME)]
    public string? LastName { get; set; }
    public const string LAST_NAME = "last_name";

    [BsonElement(EMAIL)]
    [BsonRequired]
    public string? Email { get; set; }
    public const string EMAIL = "email";

    [BsonElement(PHONE_NUMBER)]
    public string? PhoneNumber { get; set; }
    public const string PHONE_NUMBER = "phone_number";

    [BsonElement(USERNAME)]
    [BsonRequired]
    public string? Username { get; set; }
    public const string USERNAME = "username";

    [BsonElement(PASSWORD)]
    [BsonRequired]
    public string? Password { get; set; }
    public const string PASSWORD = "password";

    [BsonElement(VERIFICATION_SECRET)]
    [BsonRequired]
    public string? VerificationSecret { get; set; }
    public const string VERIFICATION_SECRET = "verification_secret";

    [BsonElement(VERIFICATION_SECRET_UPDATED_AT)]
    [BsonRequired]
    public DateTime? VerificationSecretUpdatedAt { get; set; }
    public const string VERIFICATION_SECRET_UPDATED_AT = "verification_secret_updated_at";

    [BsonElement(IS_VERIFIED)]
    [BsonRequired]
    public bool? IsVerified { get; set; }
    public const string IS_VERIFIED = "is_verified";

    [BsonElement(LOGGED_OUT_AT)]
    public DateTime? LoggedOutAt { get; set; }
    public const string LOGGED_OUT_AT = "logged_out_at";

    [BsonElement(UPDATED_AT)]
    [BsonRequired]
    public DateTime? UpdatedAt { get; set; }
    public const string UPDATED_AT = "updated_at";

    [BsonElement(CREATED_AT)]
    [BsonRequired]
    public DateTime? CreatedAt { get; set; }
    public const string CREATED_AT = "created_at";
}