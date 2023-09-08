namespace user_management.Models;

using System;
using System.Dynamic;
using System.Reflection;
using AutoMapper;
using Bogus;
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

    [BsonElement(AUTHORIZING_CLIENT)]
    [BsonRequired]
    public AuthorizingClient? AuthorizingClient { get; set; }
    public const string AUTHORIZING_CLIENT = "authorizing_client";

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

    public static List<Field> GetHiddenFields() => GetFields().Where(f => f.Name == PASSWORD || f.Name == AUTHORIZING_CLIENT || f.Name == VERIFICATION_SECRET || f.Name == VERIFICATION_SECRET_UPDATED_AT || f.Name == LOGGED_OUT_AT).ToList();
    public static List<Field> GetUnHiddenFields() => GetFields().Where(f => GetHiddenFields().FirstOrDefault<Field?>(hf => hf != null && hf.Name == f.Name, null) == null).ToList();
    public static List<Field> GetReadableFields() => GetUnHiddenFields().ToList();
    public static List<Field> GetUpdatableFields() => GetReadableFields().Where(f => f.Name != PASSWORD || f.Name != IS_VERIFIED || f.Name != CREATED_AT || f.Name != UPDATED_AT || f.Name != "_id").ToList();
    public static List<Field> GetProtectedFieldsAgainstMassUpdating() => GetUpdatableFields().Where(f => f.Name == USERNAME || f.Name == PHONE_NUMBER || f.Name == EMAIL || f.Name == CLIENTS || f.Name == USER_PRIVILEGES).ToList();
    public static List<Field> GetMassUpdatableFields() => GetUpdatableFields().Where(f => GetProtectedFieldsAgainstMassUpdating().FirstOrDefault<Field?>(ff => ff != null && ff.Name == f.Name) == null).ToList();
    public static List<Field> GetFields() => new List<Field>()
        {
            new Field() { Name = "_id", IsPermitted = true },
            new Field() { Name = USER_PRIVILEGES, IsPermitted = true },
            new Field() { Name = AUTHORIZING_CLIENT, IsPermitted = true },
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

    public static User FakeUser(IEnumerable<User>? users = null, IEnumerable<Client>? clients = null, FakeUserOptions? fakeUserOptions = null)
    {
        if (users == null) users = new User[] { };
        if (clients == null) clients = new Client[] { };
        if (fakeUserOptions == null) fakeUserOptions = new();

        List<Privilege> privileges = StaticData.Privileges;

        Faker faker = new();

        User user = new()
        {
            Id = ObjectId.GenerateNewId(),
            FirstName = faker.Random.Bool(0.6f) ? faker.Name.FirstName() : null,
            MiddleName = faker.Random.Bool(0.6f) ? faker.Name.FirstName() : null,
            LastName = faker.Random.Bool(0.6f) ? faker.Name.LastName() : null,
            Email = faker.Internet.ExampleEmail(),
            Username = faker.Internet.UserName(),
            Password = faker.Internet.Password(),
            PhoneNumber = faker.Random.Bool(0.4f) ? faker.Phone.PhoneNumber() : null,
            IsVerified = faker.Random.Bool(0.8f),
            LoggedOutAt = faker.Date.Between(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(+14)),
            CreatedAt = faker.Date.Between(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(+14))
        };

        do
        {
            if (users.FirstOrDefault<User?>(u => u != null && u.Username == user.Username) != null)
                user.Username = faker.Random.String2(50);
            else if (users.FirstOrDefault<User?>(u => u != null && user.FirstName != null && u.FirstName == user.FirstName) != null)
                user.FirstName = faker.Random.Bool(0.6f) ? faker.Random.String2(50) : null;
            else if (users.FirstOrDefault<User?>(u => u != null && user.MiddleName != null && u.MiddleName == user.MiddleName) != null)
                user.MiddleName = faker.Random.Bool(0.6f) ? faker.Random.String2(50) : null;
            else if (users.FirstOrDefault<User?>(u => u != null && user.LastName != null && u.LastName == user.LastName) != null)
                user.LastName = faker.Random.Bool(0.6f) ? faker.Random.String2(50) : null;
            else if (users.FirstOrDefault<User?>(u => u != null && user.PhoneNumber != null && u.PhoneNumber == user.PhoneNumber) != null)
                user.PhoneNumber = faker.Random.Bool(0.6f) ? faker.Random.String2(50) : null;
            else break;
        } while (true);

        user.UpdatedAt = user.CreatedAt.AddHours(faker.Random.Int(2, 96));

        IEnumerable<Privilege> userPrivileges = null!;
        if (fakeUserOptions.RandomPrivileges) user.Privileges = faker.PickRandom<Privilege>(privileges, faker.Random.Int(0, privileges.Count())).ToArray();
        else user.Privileges = privileges.ToArray();

        if (fakeUserOptions.RandomClients && clients.Count() > 1)
        {
            IEnumerable<Client> pickedClients = faker.PickRandom<Client>(clients, faker.Random.Int(0, clients.Count()));
            for (int i = 0; i < pickedClients.Count(); i++)
                user.Clients = user.Clients.Append(UserClient.FakeUserClient(pickedClients.ElementAt(i), userPrivileges)).ToArray();
        }
        else if (fakeUserOptions.RandomClients && clients.Count() == 1 && faker.Random.Bool())
            user.Clients = user.Clients.Append(UserClient.FakeUserClient(clients.ElementAt(0), userPrivileges)).ToArray();
        else if (!fakeUserOptions.RandomClients && clients.Count() > 1)
            for (int i = 0; i < clients.Count(); i++)
                user.Clients = user.Clients.Append(UserClient.FakeUserClient(clients.ElementAt(i), userPrivileges)).ToArray();
        else if (!fakeUserOptions.RandomClients && clients.Count() == 1 && faker.Random.Bool())
            user.Clients = user.Clients.Append(UserClient.FakeUserClient(clients.ElementAt(0), userPrivileges)).ToArray();

        if (fakeUserOptions.GiveUserPrivilegesToRandomUsers)
        {
            IEnumerable<User> pickedUsers = faker.PickRandom<User>(users, faker.Random.Int(0, users.Count()));
            for (int i = 0; i < pickedUsers.Count(); i++)
            {
                if (faker.Random.Bool())
                    user.UserPrivileges.Readers = user.UserPrivileges.Readers.Append(new()
                    {
                        Author = Reader.USER,
                        AuthorId = pickedUsers.ElementAt(i).Id,
                        IsPermitted = faker.Random.Bool(0.8f),
                        Fields = faker.PickRandom(User.GetReadableFields(), faker.Random.Int(0, User.GetReadableFields().Count())).ToArray()
                    }).ToArray();

                if (faker.Random.Bool())
                {
                    Field[] acceptableFields = User.GetUpdatableFields().Where(f =>
                        user.UserPrivileges.Readers.FirstOrDefault(r =>
                            r.AuthorId == pickedUsers.ElementAt(i).Id
                            && r.IsPermitted
                            && r.Fields.FirstOrDefault(readerField => readerField.Name == f.Name && readerField.IsPermitted) != null
                        ) != null).ToArray();

                    user.UserPrivileges.Updaters = user.UserPrivileges.Updaters.Append(new()
                    {
                        Author = Updater.USER,
                        AuthorId = pickedUsers.ElementAt(i).Id,
                        IsPermitted = faker.Random.Bool(0.8f),
                        Fields = faker.PickRandom(acceptableFields, faker.Random.Int(0, acceptableFields.Count())).ToArray()
                    }).ToArray();
                }

                if (faker.Random.Bool()) user.UserPrivileges.Deleters = user.UserPrivileges.Deleters.Append(new()
                {
                    Author = Deleter.USER,
                    AuthorId = pickedUsers.ElementAt(i).Id,
                    IsPermitted = faker.Random.Bool()
                }).ToArray();

                if (faker.Random.Bool()) user.UserPrivileges.AllReaders = new() { ArePermitted = faker.Random.Bool(0.8f), Fields = faker.PickRandom<Field>(User.GetReadableFields(), faker.Random.Int(0, User.GetReadableFields().Count())).ToArray() };

                if (faker.Random.Bool())
                {
                    Field[] acceptableFields = User.GetUpdatableFields().Where(f =>
                        user.UserPrivileges.AllReaders != null
                        && user.UserPrivileges.AllReaders.ArePermitted
                        && user.UserPrivileges.AllReaders.Fields.FirstOrDefault(allReadersField => allReadersField.Name == f.Name && allReadersField.IsPermitted) != null).ToArray();

                    user.UserPrivileges.AllUpdaters = new() { ArePermitted = faker.Random.Bool(0.8f), Fields = faker.PickRandom<Field>(acceptableFields, faker.Random.Int(0, acceptableFields.Count())).ToArray() };
                }
            }
        }

        if (fakeUserOptions.GiveUserPrivilegesToItSelf) user.UserPrivileges = new()
        {
            Readers = new Reader[] { new() { Author = Reader.USER, AuthorId = user.Id, IsPermitted = true, Fields = User.GetReadableFields().ToArray() } },
            AllReaders = new() { ArePermitted = false, Fields = new Field[] { } },
            Updaters = new Updater[] { new() { Author = Updater.USER, AuthorId = user.Id, IsPermitted = true, Fields = User.GetUpdatableFields().ToArray() } },
            AllUpdaters = new() { ArePermitted = false, Fields = new Field[] { } },
        };

        // Giving privileges to authorized clients.
        for (int j = 0; j < user.Clients.Count(); j++)
        {
            if (user.Clients[j].RefreshToken == null) continue;

            if (user.Clients[j].RefreshToken!.TokenPrivileges.ReadsFields.Length > 0)
                user.UserPrivileges.Readers = user.UserPrivileges.Readers.Append(new()
                {
                    Author = Reader.CLIENT,
                    AuthorId = user.Clients[j].ClientId,
                    IsPermitted = faker.Random.Bool(0.8f),
                    Fields = user.Clients[j].RefreshToken!.TokenPrivileges.ReadsFields
                }).ToArray();
            if (user.Clients[j].RefreshToken!.TokenPrivileges.UpdatesFields.Length > 0)
                user.UserPrivileges.Updaters = user.UserPrivileges.Updaters.Append(new()
                {
                    Author = Updater.CLIENT,
                    AuthorId = user.Clients[j].ClientId,
                    IsPermitted = faker.Random.Bool(0.8f),
                    Fields = user.Clients[j].RefreshToken!.TokenPrivileges.UpdatesFields
                }).ToArray();
            if (user.Clients[j].RefreshToken!.TokenPrivileges.DeletesUser)
                user.UserPrivileges.Deleters = user.UserPrivileges.Deleters.Append(new()
                {
                    Author = Deleter.CLIENT,
                    AuthorId = user.Clients[j].ClientId,
                    IsPermitted = faker.Random.Bool(0.8f)
                }).ToArray();
        }

        return user;
    }

    public override bool Equals(object? obj) => obj != null && Equals(obj as User);

    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}
