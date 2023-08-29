namespace user_management.Models;

using System;
using System.Collections;
using System.Dynamic;
using System.Reflection;
using AutoMapper;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using user_management.Data;
using user_management.Dtos.User;
using user_management.Utilities;

[BsonIgnoreExtraElements]
public class User : IEquatable<User>
{
    public User()
    {
        UserPrivileges = new()
        {
            Readers = new Reader[] { new Reader() { Author = Reader.USER, AuthorId = Id, IsPermitted = true, Fields = GetReadableFields().ToArray() } },
            AllReaders = new AllReaders() { ArePermitted = false },
            Updaters = new Updater[] { new Updater() { Author = Updater.USER, AuthorId = Id, IsPermitted = true, Fields = GetUpdatableFields().ToArray() } },
            AllUpdaters = new AllUpdaters() { ArePermitted = false },
            Deleters = new Deleter[] { new Deleter() { Author = Deleter.USER, AuthorId = Id, IsPermitted = true } }
        };
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonRequired]
    public ObjectId Id { get; set; }

    [BsonElement(PRIVILEGES)]
    [BsonRequired]
    public Privilege[] Privileges { get; set; } = StaticData.GetDefaultUserPrivileges().ToArray();
    public const string PRIVILEGES = "privileges";

    [BsonElement(USER_PRIVILEGES)]
    [BsonRequired]
    public UserPrivileges UserPrivileges { get; set; }
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
    public string Email { get; set; } = null!;
    public const string EMAIL = "email";

    [BsonElement(PHONE_NUMBER)]
    public string? PhoneNumber { get; set; }
    public const string PHONE_NUMBER = "phone_number";

    [BsonElement(USERNAME)]
    [BsonRequired]
    public string Username { get; set; } = null!;
    public const string USERNAME = "username";

    [BsonElement(PASSWORD)]
    [BsonRequired]
    public string Password { get; set; } = null!;
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
    public bool? IsVerified { get; set; } = false;
    public const string IS_VERIFIED = "is_verified";

    [BsonElement(LOGGED_OUT_AT)]
    public DateTime? LoggedOutAt { get; set; }
    public const string LOGGED_OUT_AT = "logged_out_at";

    [BsonElement(UPDATED_AT)]
    [BsonRequired]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public const string UPDATED_AT = "updated_at";

    [BsonElement(CREATED_AT)]
    [BsonRequired]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public const string CREATED_AT = "created_at";

    public static List<object> GetReadables(List<User> users, ObjectId actorId, IMapper IMapper, bool forClients = false)
    {
        if (users == null || users.Count == 0)
            return new List<object>() { };

        IEnumerable<object> rawUsers = users.ConvertAll<object?>(user => user.GetReadable(actorId, IMapper, forClients)) as IEnumerable<object>;

        return rawUsers.Where<object>(o => o != null).ToList();
    }

    public object GetReadable(ObjectId actorId, IMapper IMapper, bool forClients = false)
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

        if (fields.Count == 0 || fields.FirstOrDefault<Field?>(f => f != null && f.Name == "_id", null) == null)
            fields.Add(new() { Name = "_id", IsPermitted = true });

        fields.ForEach(action);

        return userRetrieveDto;
    }

    public static List<Field> GetHiddenFields() => GetFields().Where(f => f.Name == PASSWORD || f.Name == VERIFICATION_SECRET || f.Name == VERIFICATION_SECRET_UPDATED_AT || f.Name == LOGGED_OUT_AT).ToList();
    public static List<Field> GetUnHiddenFields() => GetFields().Where(f => GetHiddenFields().FirstOrDefault<Field?>(hf => hf != null && hf.Name == f.Name, null) == null).ToList();
    public static List<Field> GetReadableFields() => GetUnHiddenFields().ToList();
    public static List<Field> GetUpdatableFields() => GetReadableFields().Where(f => f.Name != PASSWORD || f.Name != IS_VERIFIED || f.Name != CREATED_AT || f.Name != UPDATED_AT || f.Name != "_id").ToList();
    public static List<Field> GetProtectedFieldsAgainstMassUpdating() => GetUpdatableFields().Where(f => f.Name == USERNAME || f.Name == PHONE_NUMBER || f.Name == EMAIL || f.Name == CLIENTS || f.Name == USER_PRIVILEGES).ToList();
    public static List<Field> GetMassUpdatableFields() => GetUpdatableFields().Where(f => GetProtectedFieldsAgainstMassUpdating().FirstOrDefault<Field?>(ff => ff != null && ff.Name == f.Name) == null).ToList();
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

    public bool Equals(User? other)
    {
        if (other == null) return false;

        Func<PropertyInfo, string> GetKeyFromProperty = p =>
        {
            CustomAttributeData ca = p.CustomAttributes.First(c => c != null && (c.AttributeType == typeof(BsonIdAttribute) || c.AttributeType == typeof(BsonElementAttribute)));

            string key = "";
            if (ca.AttributeType == typeof(BsonIdAttribute))
                key = "_id";
            else if (ca.AttributeType == typeof(BsonElementAttribute))
                key = (ca.ConstructorArguments[0].Value as string)!;

            return key;
        };

        Func<object, List<PropertyInfo>> GetProperties = o =>
        {
            return o
            .GetType()
            .GetProperties()
            .Where(p => p.CustomAttributes.FirstOrDefault(c => c != null && (c.AttributeType == typeof(BsonElementAttribute) || c.AttributeType == typeof(BsonIdAttribute))) != null)
            .ToList();
        };

        List<PropertyInfo> otherProperties = GetProperties(other);
        for (int i = 0; i < otherProperties.Count(); i++)
        {
            PropertyInfo p = otherProperties[i];
            var otherObjectValue = p.GetValue(other);

            CustomAttributeData ca = p.CustomAttributes.First(c => c != null && (c.AttributeType == typeof(BsonIdAttribute) || c.AttributeType == typeof(BsonElementAttribute)));

            PropertyInfo? thisObjectProperty = this.GetType().GetProperty(p.Name);
            if (thisObjectProperty == null) return false;

            var thisObjectValue = thisObjectProperty.GetValue(this);

            if (thisObjectValue == null && otherObjectValue == null) return true;
            else if (thisObjectValue == null || otherObjectValue == null) return false;

            IEnumerable<object>? iterableObject = thisObjectValue as IEnumerable<object>;
            if (iterableObject != null)
            {
                for (int j = 0; j < iterableObject.Count(); j++)
                    if (iterableObject.ElementAt(j).GetType() == typeof(DateTime) && (
                        (otherObjectValue as IEnumerable<object>) == null ||
                        Math.Floor((decimal)((DateTime)iterableObject.ElementAt(j)).Ticks / 10000) != Math.Floor((decimal)((DateTime)(otherObjectValue as IEnumerable<object>)!.ElementAt(j)).Ticks / 10000))
                    )
                        return false;
                    else if (iterableObject.ElementAt(j).GetType() != typeof(DateTime) && (
                        (otherObjectValue as IEnumerable<object>) == null ||
                        !iterableObject.ElementAt(j).Equals((otherObjectValue as IEnumerable<object>)!.ElementAt(j)))
                    )
                        return false;

                return true;
            }
            else if (
                (
                    thisObjectValue.GetType() == typeof(DateTime) ?
                    Math.Floor((decimal)((DateTime)thisObjectValue).Ticks / 10000) == Math.Floor((decimal)((DateTime)otherObjectValue).Ticks / 10000) :
                    otherObjectValue.Equals(thisObjectValue)
                ) == false) return false;
        }

        return true;
    }

    public override bool Equals(object? obj) => obj != null && Equals((User)obj);
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}