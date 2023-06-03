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

    [BsonElement(USER_PRIVILEGES)]
    [BsonRequired]
    public UserPrivileges? UserPrivileges { get; set; }
    public const string USER_PRIVILEGES = "user_privileges";

    [BsonElement(CLIENTS)]
    [BsonRequired]
    public UserClient[] Clients { get; set; } = new UserClient[] { };
    public const string CLIENTS = "clients";

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
    public static UserPrivileges GetDefaultUserPrivileges(ObjectId userId) => new UserPrivileges()
    {
        Privileges = StaticData.GetDefaultUserPrivileges().ToArray(),
        Readers = new Reader[] { new Reader() { Author = Reader.USER, AuthorId = userId, IsPermitted = true, Fields = GetDefaultReadableFields().ToArray() } },
        AllReaders = new AllReaders() { ArePermitted = false, Fields = GetDefaultReadableFields().ToArray() },
        Updaters = new Updater[] { new Updater() { Author = Updater.USER, AuthorId = userId, IsPermitted = true, Fields = GetDefaultUpdatableFields().ToArray() } },
        AllUpdaters = new AllUpdaters() { ArePermitted = false, Fields = GetDefaultReadableFields().ToArray() },
        Deleters = new Deleter[] { new Deleter() { Author = Deleter.USER, AuthorId = userId, IsPermitted = true } }
    };
    public static List<Field> GetDefaultReadableFields() => GetFields().Where(f => f.Name != PASSWORD || f.Name != VERIFICATION_SECRET || f.Name != VERIFICATION_SECRET_UPDATED_AT || f.Name != LOGGED_OUT_AT).ToList();
    public static List<Field> GetDefaultUpdatableFields() => GetDefaultReadableFields().Where(f => f.Name == FIRST_NAME || f.Name == MIDDLE_NAME || f.Name == LAST_NAME).ToList();
    public static List<Field> GetFields() => new List<Field>()
        {
            new Field() { Name = "_id", IsPermitted = true },
            new Field() { Name = USER_PRIVILEGES, IsPermitted = true },
            new Field() { Name = CLIENTS, IsPermitted = true },
            new Field() { Name = FIRST_NAME, IsPermitted = true },
            new Field() { Name = MIDDLE_NAME, IsPermitted = true },
            new Field() { Name = LAST_NAME, IsPermitted = true },
            new Field() { Name = EMAIL, IsPermitted = true },
            new Field() { Name = PHONE_NUMBER, IsPermitted = true },
            new Field() { Name = USERNAME, IsPermitted = true },
            new Field() { Name = PASSWORD, IsPermitted = true },
            new Field() { Name = VERIFICATION_SECRET, IsPermitted = true },
            new Field() { Name = VERIFICATION_SECRET_UPDATED_AT, IsPermitted = true },
            new Field() { Name = IS_VERIFIED, IsPermitted = true },
            new Field() { Name = LOGGED_OUT_AT, IsPermitted = true },
            new Field() { Name = UPDATED_AT, IsPermitted = true },
            new Field() { Name = CREATED_AT, IsPermitted = true }
        };
}