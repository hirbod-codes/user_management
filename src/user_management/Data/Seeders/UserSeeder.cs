using Bogus;
using user_management.Models;
using user_management.Utilities;

namespace user_management.Data.Seeders;

public class UserSeeder
{
    public virtual User FakeUser(string userId, IEnumerable<User>? users = null, IEnumerable<Client>? clients = null, FakeUserOptions? fakeUserOptions = null, string? password = null)
    {
        users ??= Array.Empty<User>();
        clients ??= Array.Empty<Client>();
        fakeUserOptions ??= new();

        List<Privilege> privileges = StaticData.Privileges;

        Faker faker = new();

        User user = new()
        {
            Id = userId,
            FirstName = faker.Random.Bool(0.6f) ? faker.Name.FirstName() : null,
            MiddleName = faker.Random.Bool(0.6f) ? faker.Name.FirstName() : null,
            LastName = faker.Random.Bool(0.6f) ? faker.Name.LastName() : null,
            Email = faker.Internet.ExampleEmail(),
            Username = faker.Internet.UserName(),
            Password = password is not null ? new StringHelper().Hash(password) : new StringHelper().Hash(faker.Internet.Password()),
            PhoneNumber = faker.Random.Bool(0.4f) ? faker.Phone.PhoneNumber() : null,
            IsEmailVerified = faker.Random.Bool(0.7f),
            VerificationSecret = faker.Random.Bool(0.7f) ? faker.Random.String2(100) : null,
            CreatedAt = faker.Date.Between(DateTime.UtcNow.AddDays(-15), DateTime.UtcNow.AddDays(-1))
        };
        user.LoggedOutAt = user.CreatedAt.AddHours(faker.Random.Int(1, 10));
        user.VerificationSecretUpdatedAt = faker.Random.Bool(0.7f) ? user.CreatedAt.AddHours(faker.Random.Int(1, 10)) : null;

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
                user.AuthorizedClients = user.AuthorizedClients.Append(AuthorizedClient.FakeAuthorizedClient(pickedClients.ElementAt(i), userPrivileges)).ToArray();
        }
        else if (fakeUserOptions.RandomClients && clients.Count() == 1 && faker.Random.Bool())
            user.AuthorizedClients = user.AuthorizedClients.Append(AuthorizedClient.FakeAuthorizedClient(clients.ElementAt(0), userPrivileges)).ToArray();
        else if (!fakeUserOptions.RandomClients && clients.Count() > 1)
            for (int i = 0; i < clients.Count(); i++)
                user.AuthorizedClients = user.AuthorizedClients.Append(AuthorizedClient.FakeAuthorizedClient(clients.ElementAt(i), userPrivileges)).ToArray();
        else if (!fakeUserOptions.RandomClients && clients.Count() == 1 && faker.Random.Bool())
            user.AuthorizedClients = user.AuthorizedClients.Append(AuthorizedClient.FakeAuthorizedClient(clients.ElementAt(0), userPrivileges)).ToArray();

        if (fakeUserOptions.GiveUserPrivilegesToRandomUsers)
        {
            IEnumerable<User> pickedUsers = faker.PickRandom<User>(users, faker.Random.Int(0, users.Count()));
            for (int i = 0; i < pickedUsers.Count(); i++)
            {
                if (faker.Random.Bool())
                    user.UserPermissions.Readers = user.UserPermissions.Readers.Append(new()
                    {
                        Author = Reader.USER,
                        AuthorId = pickedUsers.ElementAt(i).Id,
                        IsPermitted = faker.Random.Bool(0.8f),
                        Fields = faker.PickRandom(User.GetReadableFields(), faker.Random.Int(0, User.GetReadableFields().Count())).ToArray()
                    }).ToArray();

                if (faker.Random.Bool())
                {
                    Field[] acceptableFields = User.GetUpdatableFields().Where(f =>
                        user.UserPermissions.Readers.FirstOrDefault(r =>
                            r.AuthorId == pickedUsers.ElementAt(i).Id
                            && r.IsPermitted
                            && r.Fields.FirstOrDefault(readerField => readerField.Name == f.Name && readerField.IsPermitted) != null
                        ) != null).ToArray();

                    user.UserPermissions.Updaters = user.UserPermissions.Updaters.Append(new()
                    {
                        Author = Updater.USER,
                        AuthorId = pickedUsers.ElementAt(i).Id,
                        IsPermitted = faker.Random.Bool(0.8f),
                        Fields = faker.PickRandom(acceptableFields, faker.Random.Int(0, acceptableFields.Count())).ToArray()
                    }).ToArray();
                }

                if (faker.Random.Bool()) user.UserPermissions.Deleters = user.UserPermissions.Deleters.Append(new()
                {
                    Author = Deleter.USER,
                    AuthorId = pickedUsers.ElementAt(i).Id,
                    IsPermitted = faker.Random.Bool()
                }).ToArray();

                if (faker.Random.Bool()) user.UserPermissions.AllReaders = new() { ArePermitted = faker.Random.Bool(0.8f), Fields = faker.PickRandom<Field>(User.GetReadableFields(), faker.Random.Int(0, User.GetReadableFields().Count())).ToArray() };

                if (faker.Random.Bool())
                {
                    Field[] acceptableFields = User.GetUpdatableFields().Where(f =>
                        user.UserPermissions.AllReaders != null
                        && user.UserPermissions.AllReaders.ArePermitted
                        && user.UserPermissions.AllReaders.Fields.FirstOrDefault(allReadersField => allReadersField.Name == f.Name && allReadersField.IsPermitted) != null).ToArray();

                    user.UserPermissions.AllUpdaters = new() { ArePermitted = faker.Random.Bool(0.8f), Fields = faker.PickRandom<Field>(acceptableFields, faker.Random.Int(0, acceptableFields.Count())).ToArray() };
                }
            }
        }

        if (fakeUserOptions.GiveUserPrivilegesToItSelf)
            user.UserPermissions = new()
            {
                Readers = new Reader[] { new() { Author = Reader.USER, AuthorId = user.Id, IsPermitted = true, Fields = User.GetReadableFields().ToArray() } },
                AllReaders = new() { ArePermitted = false, Fields = new Field[] { } },
                Updaters = new Updater[] { new() { Author = Updater.USER, AuthorId = user.Id, IsPermitted = true, Fields = User.GetUpdatableFields().ToArray() } },
                AllUpdaters = new() { ArePermitted = false, Fields = new Field[] { } },
            };

        // Giving privileges to authorized clients.
        for (int j = 0; j < user.AuthorizedClients.Length; j++)
        {
            if (user.AuthorizedClients[j].RefreshToken == null) continue;

            if (user.AuthorizedClients[j].RefreshToken!.TokenPrivileges.ReadsFields.Length > 0)
                user.UserPermissions.Readers = user.UserPermissions.Readers.Append(new()
                {
                    Author = Reader.CLIENT,
                    AuthorId = user.AuthorizedClients[j].ClientId,
                    IsPermitted = faker.Random.Bool(0.8f),
                    Fields = user.AuthorizedClients[j].RefreshToken!.TokenPrivileges.ReadsFields
                }).ToArray();
            if (user.AuthorizedClients[j].RefreshToken!.TokenPrivileges.UpdatesFields.Length > 0)
                user.UserPermissions.Updaters = user.UserPermissions.Updaters.Append(new()
                {
                    Author = Updater.CLIENT,
                    AuthorId = user.AuthorizedClients[j].ClientId,
                    IsPermitted = faker.Random.Bool(0.8f),
                    Fields = user.AuthorizedClients[j].RefreshToken!.TokenPrivileges.UpdatesFields
                }).ToArray();
            if (user.AuthorizedClients[j].RefreshToken!.TokenPrivileges.DeletesUser)
                user.UserPermissions.Deleters = user.UserPermissions.Deleters.Append(new()
                {
                    Author = Deleter.CLIENT,
                    AuthorId = user.AuthorizedClients[j].ClientId,
                    IsPermitted = faker.Random.Bool(0.8f)
                }).ToArray();
        }

        return user;
    }

    public virtual User GetAdminUser(string adminId, string adminUsername, string adminPassword, string adminEmail, string? adminPhoneNumber) => new()
    {
        Id = adminId,
        Privileges = StaticData.Privileges.ToArray(),
        UserPermissions = new UserPermissions()
        {
            AllReaders = new() { ArePermitted = false, Fields = Array.Empty<Field>() },
            AllUpdaters = new() { ArePermitted = false, Fields = Array.Empty<Field>() },
            Readers = new Reader[] { new() { Author = Reader.USER, AuthorId = adminId, IsPermitted = true, Fields = User.GetFields().ToArray() } },
            Updaters = new Updater[] { new() { Author = Updater.USER, AuthorId = adminId, IsPermitted = true, Fields = User.GetFields().ToArray() } },
            Deleters = new Deleter[] { new() { Author = Deleter.USER, AuthorId = adminId, IsPermitted = true } },
        },
        Username = adminUsername,
        Email = adminEmail,
        PhoneNumber = adminPhoneNumber,
        Password = new StringHelper().Hash(adminPassword),
        IsEmailVerified = true,
        VerificationSecret = new Faker().Random.String2(128),
        VerificationSecretUpdatedAt = DateTime.UtcNow.AddDays(-2),
        UpdatedAt = DateTime.UtcNow.AddDays(-2),
        CreatedAt = DateTime.UtcNow.AddDays(-3)
    };
}
