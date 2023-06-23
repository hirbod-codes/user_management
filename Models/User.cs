namespace user_management.Models;

using System.Dynamic;
using AutoMapper;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using user_management.Data;
using user_management.Dtos.User;
using user_management.Utilities;

[BsonIgnoreExtraElements]
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonRequired]
    public ObjectId? Id { get; set; }

    [BsonElement(PRIVILEGES)]
    [BsonRequired]
    public Privilege[] Privileges { get; set; } = new Privilege[] { };
    public const string PRIVILEGES = "privileges";

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

    public static List<object> GetReadables(List<User> users, ObjectId actorId, IMapper IMapper, bool forClients = false)
    {
        if (users == null || users.Count == 0)
            return new List<object>() { };

        IEnumerable<object> rawUsers = users.ConvertAll<object?>(user => user.GetReadable(actorId, IMapper, forClients)) as IEnumerable<object>;

        return rawUsers.Where<object>(o => o != null).ToList();
    }

    public object? GetReadable(ObjectId actorId, IMapper IMapper, bool forClients = false)
    {
        var userRetrieveDto = new ExpandoObject() as IDictionary<string, Object?>;
        List<Field>? fields = new List<Field>() { };
        Action<Field> action = field =>
            {
                if (GetHiddenFields().FirstOrDefault<Field?>(f => f != null && f.Name == field.Name, null) != null) return;

                dynamic? value;
                if (field.Name == "_id")
                    value = typeof(User).GetProperty("Id")!.GetValue(this);
                else
                    value = typeof(User).GetProperty(field.Name.ToPascalCase())!.GetValue(this);

                if (value != null && field.Name == "_id")
                    value = value!.ToString();

                if (value != null && field.Name.ToPascalCase() == CLIENTS.ToPascalCase())
                    value = (value as UserClient[])!.ToList().ConvertAll<UserClientRetrieveDto>(v => IMapper.Map<UserClientRetrieveDto>(v));

                if (value != null && field.Name.ToPascalCase() == USER_PRIVILEGES.ToPascalCase())
                    value = IMapper.Map<UserPrivilegesRetrieveDto>(value);

                if (field.IsPermitted && !userRetrieveDto.ContainsKey(field.Name))
                    userRetrieveDto.Add(field.Name, value);
            };

        try
        {
            if (UserPrivileges!.AllReaders != null && UserPrivileges!.AllReaders.ArePermitted)
                fields = fields.Concat(UserPrivileges!.AllReaders.Fields).ToList();
        }
        catch (NullReferenceException) { }
        catch (InvalidOperationException) { }

        try
        {
            if (UserPrivileges!.Readers.Length != 0)
                fields = fields.Concat(UserPrivileges!.Readers.First(r => r != null && r.Author == (forClients ? Reader.CLIENT : Reader.USER) && r.AuthorId == actorId && r.IsPermitted == true).Fields).ToList();
        }
        catch (NullReferenceException) { }
        catch (InvalidOperationException) { }

        if (fields.Count == 0)
            return null;

        fields.ForEach(action);

        return userRetrieveDto;
    }

    public static Privilege[] GetDefaultPrivileges(ObjectId userId) => StaticData.GetDefaultUserPrivileges().ToArray();
    public static UserPrivileges GetDefaultUserPrivileges(ObjectId userId) => new()
    {
        Readers = new Reader[] { new Reader() { Author = Reader.USER, AuthorId = userId, IsPermitted = true, Fields = GetDefaultReadableFields().ToArray() } },
        AllReaders = new AllReaders() { ArePermitted = false },
        Updaters = new Updater[] { new Updater() { Author = Updater.USER, AuthorId = userId, IsPermitted = true, Fields = GetDefaultUpdatableFields().ToArray() } },
        AllUpdaters = new AllUpdaters() { ArePermitted = false },
        Deleters = new Deleter[] { new Deleter() { Author = Deleter.USER, AuthorId = userId, IsPermitted = true } }
    };
    public static List<Field> GetHiddenFields() => GetFields().Where(f => f.Name == PASSWORD || f.Name == VERIFICATION_SECRET || f.Name == VERIFICATION_SECRET_UPDATED_AT || f.Name == LOGGED_OUT_AT).ToList();
    public static List<Field> GetUnHiddenFields() => GetFields().Where(f => GetHiddenFields().FirstOrDefault<Field?>(hf => hf != null && hf.Name == f.Name, null) == null).ToList();
    public static List<Field> GetDefaultReadableFields() => GetUnHiddenFields().ToList();
    public static List<Field> GetDefaultUpdatableFields() => GetDefaultReadableFields().Where(f => f.Name == PASSWORD || f.Name == USERNAME || f.Name == PHONE_NUMBER || f.Name == EMAIL || f.Name == FIRST_NAME || f.Name == MIDDLE_NAME || f.Name == LAST_NAME || f.Name == CLIENTS || f.Name == USER_PRIVILEGES).ToList();
    public static List<Field> GetProtectedFieldsAgainstMassUpdating() => GetDefaultUpdatableFields().Where(f => f.Name == PASSWORD || f.Name == USERNAME || f.Name == PHONE_NUMBER || f.Name == EMAIL || f.Name == CLIENTS || f.Name == USER_PRIVILEGES).ToList();
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